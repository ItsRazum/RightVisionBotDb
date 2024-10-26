using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using RightVisionBotDb.Data;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Helpers;
using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Lang;
using RightVisionBotDb.Lang.Phrases;
using RightVisionBotDb.Models;
using RightVisionBotDb.Singletons;
using RightVisionBotDb.Types;
using Serilog;
using System.Data;
using System.Globalization;
using System.Runtime.InteropServices;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBotDb.Locations
{
    public sealed class RootLocation : RootLocationBase
    {

        #region Constructor

        public RootLocation(
            Bot bot,
            LocationManager locationManager,
            RvLogger rvLogger,
            LocationsFront locationsFront,
            ILogger logger)
            : base(bot, locationManager, rvLogger, locationsFront, logger)
        {
            this
                .RegisterTextCommand("/start", StartCommand)
                .RegisterTextCommand("/hide", HideCommand)
                .RegisterTextCommand("/menu", MainMenuCommand)
                .RegisterTextCommand("назначить", AppointCommand)
                .RegisterTextCommand("+permission", );

            this
                .RegisterCallbackCommand("rvProperties", RvPropertiesCallback);
        }

        #endregion

        #region RootLocationBase overrides

        public override async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken token = default)
        {
            RvUser? rvUser = null;
            try
            {
                using var db = DatabaseHelper.GetApplicationDbContext();
                using var rvContext = DatabaseHelper.GetRightVisionContext(App.DefaultRightVision);

                (var from, rvUser, var chatType) = await GetRvUserAndChatType(update, db, token);
                bool containsArgs = false;

                if (rvUser.Telegram != from.Username)
                    rvUser.Telegram = from.Username ?? string.Empty;

                rvUser.LocationChanged += OnLocationChanged;

                if (update.CallbackQuery != null)
                {
                    var callbackQuery = update.CallbackQuery;
                    _logger.Information("=== {0} ===" +
                        $"\nCallbackId: {callbackQuery.Id}" +
                        $"\nCallbackData: {callbackQuery.Data}" +
                        $"\n" +
                        $"\nId отправителя: {from.Id}" +
                        $"\nUsername отправителя: {"@" + from.Username}" +
                        $"\nИмя отправителя: {from.FirstName}" +
                        $"\n" +
                        $"\nЧат: {chatType}" +
                        $"\nId чата: {callbackQuery.Message?.Chat.Id}" +
                        $"\n", "Входящий Callback");

                    var callbackContext = new CallbackContext(rvUser, callbackQuery, db, rvContext);

                    containsArgs = callbackContext.CallbackQuery.Data!.Contains('-');

                    await HandleCallbackAsync(callbackContext, containsArgs, token);

                    if (chatType != ChatType.Private)
                        await LocationManager[nameof(PublicChat)].HandleCallbackAsync(callbackContext, containsArgs, token);

                    else
                        await rvUser.Location.HandleCallbackAsync(callbackContext, containsArgs, token);

                    if (chatType == ChatType.Private && rvUser.Lang == Enums.Lang.Na)
                    {
                        await Bot.Client.DeleteMessageAsync(callbackQuery.Message!.Chat, callbackQuery.Message.MessageId, token);
                        await Bot.Client.SendTextMessageAsync(callbackQuery.Message.Chat, "Choose Lang:", replyMarkup: KeyboardsHelper.СhooseLang, cancellationToken: token);
                        return;
                    }
                }
                else if (update.Message != null)
                {
                    var message = update.Message;
                    _logger.Information("=== {0} ===" +
                        $"\nId отправителя: {from.Id}" +
                        $"\nUsername отправителя: {"@" + from.Username}" +
                        $"\nИмя отправителя: {from.FirstName}" +
                        $"\n" +
                        $"\nТекст сообщения: {message.Text}" +
                        $"\n" +
                        $"\nЧат: {chatType}" +
                        $"\nId чата: {message.Chat.Id}" +
                        $"\n", "Входящее сообщение");

                    var commandContext = new CommandContext(rvUser, message, db, rvContext);

                    containsArgs = commandContext.Message.Text != null && commandContext.Message.Text.Contains(' ');

                    if (chatType == ChatType.Private && rvUser.Lang == Enums.Lang.Na)
                    {
                        await Bot.Client.SendTextMessageAsync(message.Chat, "Choose Lang:", replyMarkup: KeyboardsHelper.СhooseLang, cancellationToken: token);
                        return;
                    }

                    await HandleCommandAsync(commandContext, containsArgs, token);

                    if (chatType != ChatType.Private)
                        await LocationManager[nameof(PublicChat)].HandleCommandAsync(commandContext, containsArgs, token);

                    else
                        await rvUser.Location.HandleCommandAsync(commandContext, containsArgs, token);
                }
                
                rvUser.LocationChanged -= OnLocationChanged;

                async void OnLocationChanged(object? sender, (IRvLocation, IRvLocation) e)
                {
                    await RvLogger.Log(LogMessagesHelper.UserChangedLocation(rvUser, e), rvUser, token);
                }

                if (db.ChangeTracker.HasChanges())
                    await db.SaveChangesAsync(token);

                if(rvContext.ChangeTracker.HasChanges())
                    await rvContext.SaveChangesAsync(token);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Произошла ошибка при обработке входящего обновления!");
                if (rvUser != null)
                {
                    await botClient.SendTextMessageAsync(rvUser.UserId, "Произошла непредвиденная ошибка", cancellationToken: token);
                    await botClient.SendTextMessageAsync(-4074101060,
                        $"Произошла ошибка: {ex.Message}" +
                        $"\n" +
                        $"\nСтек вызовов:" +
                        $"\n{ex.StackTrace}" +
                        $"\n" +
                        $"\nRvUser Id: {rvUser.UserId}" +
                        $"\nRvUser Telegram: {rvUser.Telegram}" +
                        $"\nRvUser Location: {rvUser.Location}" +
                        $"\nRvUser Lang: {rvUser.Lang}", cancellationToken: token);
                }
            }
        }

        public override Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        #endregion

        #region Methods

        #region Inner Methods

        private async Task<(User from, RvUser rvUser, ChatType chatType)> GetRvUserAndChatType(Update update, ApplicationDbContext context, CancellationToken token = default)
        {
            var userId = update.CallbackQuery?.From.Id ?? update.Message?.From?.Id;

            if (userId == null)
                throw new ArgumentNullException(nameof(userId));

            var rvUser = await context.RvUsers.FirstOrDefaultAsync(u => u.UserId == userId);
            var from = update.CallbackQuery?.From ?? update.Message?.From;

            if (from == null)
                throw new ArgumentNullException(nameof(from));

            if (rvUser == null)
            {
                rvUser = new RvUser(from.Id, Enums.Lang.Na, from.FirstName, from.Username, LocationManager[nameof(Start)]);
                context.RvUsers.Add(rvUser);
                await context.SaveChangesAsync(token);
            }

            var chatType = update.CallbackQuery?.Message?.Chat.Type ?? update.Message?.Chat.Type ?? ChatType.Private;

            return (from, rvUser, chatType);

        }

        #endregion

        #region Command Methods

        private async Task StartCommand(CommandContext c, CancellationToken token = default)
        {
            if (c.Message.Chat.Type == ChatType.Private)
            {
                var location = LocationManager[nameof(Start)];
                if (c.RvUser == null)
                {
                    var rvUser = new RvUser(c.Message.From!.Id, Enums.Lang.Na, c.Message.From.FirstName, c.Message.From.Username, location);
                    c.DbContext.RvUsers.Add(rvUser);
                    await c.DbContext.SaveChangesAsync(token);
                }
                else
                {
                    c.RvUser.Location = location;
                    await c.DbContext.SaveChangesAsync(token);
                }
            }
        }

        private async Task ProfileCommand(CommandContext c, CancellationToken token = default)
        {
            var targetRvUser = c.Message.ReplyToMessage == null
                ? c.RvUser
                : c.DbContext.RvUsers.FirstOrDefault(u => u.UserId == c.Message.ReplyToMessage.From!.Id);

            if (targetRvUser == null)
            {
                await Bot.Client.SendTextMessageAsync(
                    c.Message.Chat,
                    Language.Phrases[c.RvUser.Lang].Messages.Common.UserNotFound,
                    cancellationToken: token);
                return;
            }

            var (content, keyboard) = await ProfileHelper.Profile(targetRvUser, c.RvUser, c.Message.Chat.Type, App.DefaultRightVision, token);

            await Bot.Client.SendTextMessageAsync(
                c.Message.Chat,
                content,
                replyMarkup: keyboard,
                cancellationToken: token);
        }


        private async Task HideCommand(CommandContext c, CancellationToken token = default)
        {
            await Bot.Client.SendTextMessageAsync(
                c.Message.Chat,
                "Спрятано",
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: token);
        }

        private async Task MainMenuCommand(CommandContext c, CancellationToken token)
        {
            c.RvUser.Location = LocationManager[nameof(MainMenu)];
            await Bot.Client.SendTextMessageAsync(c.Message.Chat, "✅", replyMarkup: new ReplyKeyboardRemove(), cancellationToken: token);
            await Bot.Client.SendTextMessageAsync(
                c.Message.Chat,
                string.Format(Language.Phrases[c.RvUser.Lang].Messages.Common.Greetings, c.RvUser.Name),
                replyMarkup: KeyboardsHelper.MainMenu(c.RvUser),
                cancellationToken: token);
        }

        private async Task AppointCommand(CommandContext c, CancellationToken token = default)
        {
            var args = c.Message.Text!.Trim().Split(' ');
            RvUser? targetRvUser = null;
            bool replied = c.Message.ReplyToMessage != null;
            Role role = Role.None;

            (bool success, string message) result = (false, string.Empty);

            switch (args.Length)
            {
                case 1:
                    result = (false, "Для использования этой команды необходимо указать должность!");
                    break;
                case 2:
                    if (!replied)
                    {
                        result = (false, "Не указан целевой пользователь! Чтобы его указать - выбери его реплаем или впиши его Юзернейм/Id (что-то одно) перед должностью.");
                        break;
                    }
                    targetRvUser = await c.DbContext.RvUsers.FirstOrDefaultAsync(u => u.UserId == c.Message.ReplyToMessage!.From!.Id);

                    if (targetRvUser == null)
                    {
                        result = (false, "Запрашиваемый пользователь не зарегистрирован!");
                        break;
                    }
                    if (Enum.TryParse(args.Last(), out role))
                    {
                        result = (true, $"Пользователь успешно назначен на должность {role}!");
                        break;
                    }
                    result = (false, "Запрашиваемая должность не найдена!");
                    break;
                case 3:
                    if (replied)
                    {
                        result = (false, "Слишком много аргументов!");
                        break;
                    }

                    var userTag = args[1];
                    targetRvUser = long.TryParse(userTag, out var userId)
                        ? c.DbContext.RvUsers.FirstOrDefault(u => u.UserId == userId)
                        : c.DbContext.RvUsers.FirstOrDefault(u => "@" + u.Telegram == userTag);

                    if (targetRvUser == null)
                    {
                        result = (false, "Запрашиваемый пользователь не зарегистрирован!");
                        break;
                    }
                    if (Enum.TryParse(args.Last(), out role))
                    {
                        result = (true, $"Пользователь успешно назначен на должность {role}!");
                        break;
                    }
                    result = (false, "Запрашиваемая должность не найдена!");

                    break;
            }
            if (result.success)
            {
                targetRvUser!.Role = role;
                targetRvUser.ResetPermissions();

            }

            await Bot.Client.SendTextMessageAsync(
                c.Message.Chat,
                result.message,
                cancellationToken: token);
        }

        private async Task AddPermissionCommand(CommandContext c, CancellationToken token)
        {
            var args = c.Message.Text!.Split(' ');
            var replied = c.Message.ReplyToMessage != null;
            (RvUser? targetRvUser, Permission? permission, string message) result = (null, null, string.Empty);
            
            switch (args.Length)
            {
                case 1:
                    result.message = "Для использования этой команды необходимо указать должность!";
                    break;
                case 2:
                    {
                        if (!replied)
                        {
                            result.message = "Не указан целевой пользователь! Чтобы его указать - выбери его реплаем или впиши его Юзернейм/Id (что-то одно) перед должностью.";
                        }
                        else
                        {
                            result.targetRvUser = await c.DbContext.RvUsers.FirstOrDefaultAsync(u => u.UserId == c.Message.From!.Id, cancellationToken: token);
                            if (Enum.TryParse<Permission>(args.Last(), out var permission))
                            {
                                result.permission = permission;
                                result.message = $"Пользователю успешно выдано право Permission.{permission}!";
                                break;
                            }
                            result.message = "Запрашиваемое право не найдено!";
                        }
                    }
                    break;
                case 3:
                    {
                        if (replied)
                        {
                            result.message = "Слишком много аргументов!";
                            break;
                        }

                        var userTag = args[1];
                        result.targetRvUser = long.TryParse(userTag, out var userId)
                            ? c.DbContext.RvUsers.FirstOrDefault(u => u.UserId == userId)
                            : c.DbContext.RvUsers.FirstOrDefault(u => "@" + u.Telegram == userTag);

                        if (result.targetRvUser == null)
                        {
                            result.message = "Запрашиваемый пользователь не зарегистрирован!";
                            break;
                        }
                        if (Enum.TryParse<Permission>(args.Last(), out var permission))
                        {
                            result.permission = permission;
                            result.message = $"Пользователю успешно выдано право Permission.{permission}!";
                            break;
                        }
                        result.message = "Запрашиваемое право не найдено!";
                    }
                    break;
            }
            await Bot.Client.SendTextMessageAsync(
                c.Message.Chat,
                result.message,
                cancellationToken: token);

            if (result.targetRvUser != null && result.permission != null)
                result.targetRvUser.UserPermissions += (Permission)result.permission;
        }

        private async Task RvPropertiesCallback(CallbackContext c, CancellationToken token = default)
        {
            var rightvision = c.CallbackQuery.Data!.Split('-').Last();
            using var rvdb = DatabaseHelper.GetRightVisionContext(rightvision);
            var endDateString = rvdb.EndDate?.ToString("d", new CultureInfo("ru-RU")) ?? "н.в.";

            await Bot.Client.AnswerCallbackQueryAsync(c.CallbackQuery.Id, $"{rightvision}" +
                $"\n" +
                $"\nДата проведения: {rvdb.StartDate.ToString("d", new CultureInfo("ru-RU"))} - {endDateString}" +
                $"\nКоличество участников: {rvdb.ParticipantForms.Count(p => p.Status == FormStatus.Accepted)}",
                true,
                cancellationToken: token);
        }

        #endregion

        #endregion
    }
}
