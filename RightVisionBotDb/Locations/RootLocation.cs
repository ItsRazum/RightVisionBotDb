using DryIoc.ImTools;
using Microsoft.EntityFrameworkCore;
using RightVisionBotDb.Data;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Helpers;
using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Lang;
using RightVisionBotDb.Models;
using RightVisionBotDb.Singletons;
using RightVisionBotDb.Types;
using Serilog;
using System.Globalization;
using System.Text;
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
                .RegisterTextCommand("назначить", AppointCommand, Permission.Grant)
                .RegisterTextCommand("/news", NewsCommand, Permission.News)
                .RegisterTextCommand("+permission", AddOrRemovePermissionCommand, Permission.GivePermission)
                .RegisterTextCommand("-permission", AddOrRemovePermissionCommand, Permission.GivePermission)
                .RegisterCallbackCommand("rvProperties", RvPropertiesCallback)
                .RegisterCallbackCommand("useControlPanel", UseControlPanelCallback)
                .RegisterCallbackCommand("backToProfile", BackToProfileCallback)
                .RegisterCallbackCommand("permissions_minimized", PermissionsCallback)
                .RegisterCallbackCommand("permissions_maximized", PermissionsCallback)
                .RegisterCallbackCommand("punishments_history", PunishmentsHistoryCallback)
                .RegisterCallbackCommand("punishments_hide", PunishmentsListActionCallback)
                .RegisterCallbackCommand("punishments_show", PunishmentsListActionCallback);
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
                        try
                        {
                            await Bot.Client.DeleteMessageAsync(callbackQuery.Message!.Chat, callbackQuery.Message.MessageId, token);
                        }
                        catch (Exception ex)
                        {
                            _logger.Warning(ex, "Не удалось удалить сообщение!");
                        }
                        await Bot.Client.SendTextMessageAsync(callbackQuery.Message!.Chat, "Choose Lang:", replyMarkup: KeyboardsHelper.СhooseLang, cancellationToken: token);
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

                    await HandleCommandAsync(commandContext, containsArgs, token);

                    if (chatType == ChatType.Private && rvUser.Lang == Enums.Lang.Na)
                    {
                        await Bot.Client.SendTextMessageAsync(message.Chat, "Choose Lang:", replyMarkup: KeyboardsHelper.СhooseLang, cancellationToken: token);
                    }
                    else
                    {
                        if (chatType != ChatType.Private)
                            await LocationManager[nameof(PublicChat)].HandleCommandAsync(commandContext, containsArgs, token);

                        else
                            await rvUser.Location.HandleCommandAsync(commandContext, containsArgs, token);
                    }
                }

                rvUser.LocationChanged -= OnLocationChanged;

                async void OnLocationChanged(object? sender, (IRvLocation, IRvLocation) e)
                {
                    await RvLogger.Log(LogMessagesHelper.UserChangedLocation(rvUser, e), rvUser, token);
                }

                if (db.ChangeTracker.HasChanges())
                    await db.SaveChangesAsync(token);

                if (rvContext.ChangeTracker.HasChanges())
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
            _logger.Fatal(exception, "Не удалось обработать ошибку внутри работы бота");
            throw exception;
        }

        #endregion

        #region Methods

        #region Inner Methods

        private async Task<(User from, RvUser rvUser, ChatType chatType)> GetRvUserAndChatType(Update update, ApplicationDbContext context, CancellationToken token = default)
        {
            var userId = update.CallbackQuery?.From.Id ?? update.Message?.From?.Id;

            if (userId == null)
                throw new ArgumentNullException(nameof(userId));

            var rvUser = await context.RvUsers.FirstOrDefaultAsync(u => u.UserId == userId, token);
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

        private async Task PunishmentsListActionCallback(CallbackContext c, CancellationToken token = default)
        {
            var args = c.CallbackQuery.Data!.Split('-');
            var targetUserId = long.Parse(args.Last());
            var punishmentType = Enum.Parse<PunishmentType>(args[1]);

            bool showBan = true;
            bool showMute = true;

            if (c.CallbackQuery.Message?.ReplyMarkup != null)
            {
                var callbacks = c.CallbackQuery.Message.ReplyMarkup.InlineKeyboard
                    .SelectMany(row => row)
                    .Select(button => button.CallbackData)
                    .Where(callback => callback != null)
                    .ToList();

                showBan = callbacks.Any(callback => callback!.Contains($"punishments_hide-Ban-{targetUserId}"));
                showMute = callbacks.Any(callback => callback!.Contains($"punishments_hide-Mute-{targetUserId}"));
            }

            switch (args.First())
            {
                case "punishments_hide":
                    if (punishmentType == PunishmentType.Ban) showBan = false;
                    else if (punishmentType == PunishmentType.Mute) showMute = false;
                    break;

                case "punishments_show":
                    if (punishmentType == PunishmentType.Ban) showBan = true;
                    else if (punishmentType == PunishmentType.Mute) showMute = true;
                    break;
            }

            var user = await c.DbContext.RvUsers.FirstAsync(u => u.UserId == targetUserId, token);
            await LocationsFront.PunishmentsHistory(c, user, showBan, showMute, token);
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
                    await c.DbContext.RvUsers.AddAsync(rvUser, token);
                }
                else
                {
                    c.RvUser.Location = location;
                    c.RvUser.Lang = Enums.Lang.Na;
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

            var (content, keyboard) = await ProfileHelper.Profile(targetRvUser, c, c.Message.Chat.Type, App.DefaultRightVision, token);

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
            var (extractedRvUser, args) = await CommandFormatHelper.ExtractRvUserFromArgs(c);

            string message = "Пользователь не найден или не указан!";

            if (extractedRvUser != null)
            {
                if (extractedRvUser == c.RvUser)
                    message = "Извини, но ты не можешь назначать самого себя!";

                else if (Enum.TryParse(args.First(), out Role role))
                {
                    if (role > c.RvUser.Role)
                        message = "Извини, но ты не можешь назначать на должность выше своей!";
                    else
                    {
                        extractedRvUser.Role = role;
                        extractedRvUser.ResetPermissions();
                        message = $"Пользователь успешно назначен на должность {role}!";
                        await Bot.Client.SendTextMessageAsync(
                            extractedRvUser.UserId,
                            string.Format(Language.Phrases[extractedRvUser.Lang].Messages.Common.UserAppointed, extractedRvUser.Name, role),
                            cancellationToken: token);
                    }
                }
                else
                    message = "Запрашиваемая должность не найдена!";
            }

            await Bot.Client.SendTextMessageAsync(
                c.Message.Chat,
                message,
                cancellationToken: token);
        }

        private async Task AddOrRemovePermissionCommand(CommandContext c, CancellationToken token = default)
        {
            var (extractedRvUser, args) = await CommandFormatHelper.ExtractRvUserFromArgs(c, token);

            string resultMessage = "Пользователь не найден или не указан!";

            if (extractedRvUser != null)
            {
                if (extractedRvUser == c.RvUser)
                    resultMessage = "Извини, но ты не можешь снимать права у самого себя!";

                else if (c.Message.Text!.StartsWith('~'))
                {
                    extractedRvUser.ResetPermissions();
                    resultMessage = $"Выполнен сброс прав до стандартных для пользователя.\n\nИспользованные шаблоны:\n{extractedRvUser.Status}\n{extractedRvUser.Role}";
                }

                else if (Enum.TryParse(args.Last(), out Permission permission))
                {
                    var actionType = c.Message.Text!.First();
                    switch (actionType)
                    {
                        case '+':
                            extractedRvUser.UserPermissions += permission;
                            resultMessage = $"Пользователю успешно выдано право Permission.{permission}";
                            break;
                        case '-':
                            extractedRvUser.UserPermissions -= permission;
                            resultMessage = $"С пользователя успешно снято право Permission.{permission}";
                            break;
                    }
                }
                else
                    resultMessage = "Запрашиваемое право не найдено!";
            }

            await Bot.Client.SendTextMessageAsync(
                c.Message.Chat,
                resultMessage,
                cancellationToken: token);
        }

        private async Task NewsCommand(CommandContext c, CancellationToken token)
        {
            var targetUsers = new HashSet<long>();
            var sb = new StringBuilder();

            var commandArgs = c.Message.Text!.Split(' ').RemoveAt(0).Take(3).ToList();

            sb.AppendLine("Распознаны следующие аргументы:");

            if (commandArgs.Contains("-n"))
            {
                sb.AppendLine("- Разослать подписчикам на новости (-n)");
                foreach (var rvUser in (await c.DbContext.RvUsers.ToListAsync(token)).Where(u => u.Has(Permission.News)))
                    targetUsers.Add(rvUser.UserId);

                commandArgs.Remove("-n");
            }

            if (commandArgs.Contains("-p"))
            {
                sb.AppendLine("- Разослать всем участникам (-p)");
                if (!c.RvUser.Has(Permission.ParticipantNews))
                {
                    sb.Append(" (У пользователя нет права)");
                }
                else
                {
                    foreach (var rvParticipant in (await c.RvContext.ParticipantForms.ToListAsync(token)).Where(p => p.Status == FormStatus.Accepted))
                        targetUsers.Add(rvParticipant.UserId);
                }
                commandArgs.Remove("-p");
            }

            if (commandArgs.Contains("-t"))
            {
                sb.AppendLine("- Отправить новость всем пользователям бота (-t)");
                if (!c.RvUser.Has(Permission.TechNews))
                {
                    sb.Append(" (У пользователя нет права)");
                }
                else
                {
                    targetUsers = [.. c.DbContext.RvUsers.Select(u => u.UserId)];
                }
                commandArgs.Remove("-t");
            }

            await RvLogger.Log(LogMessagesHelper.UserStartedNewsSending(c.RvUser, sb.ToString()), c.RvUser, token);

            var message = string.Join(' ', commandArgs);

            var successCount = 0;
            var failCount = 0;
            foreach (var userId in targetUsers)
            {
                try
                {
                    await Bot.Client.SendTextMessageAsync(userId, message, cancellationToken: token);
                    successCount++;
                }
                catch (Exception)
                {
                    failCount++;
                }
            }

            await Bot.Client.SendTextMessageAsync(-4074101060, $"Рассылка завершена. {successCount} получили новость, {failCount} не получили", cancellationToken: token);
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

        private async Task UseControlPanelCallback(CallbackContext c, CancellationToken token = default)
        {
            (var message, var keyboard) = ControlPanelHelper.MainPage(c.RvUser);
            await Bot.Client.SendTextMessageAsync(c.CallbackQuery.Message!.Chat, message, replyMarkup: keyboard, cancellationToken: token);
        }

        private async Task BackToProfileCallback(CallbackContext c, CancellationToken token = default)
        {
            var targetUserId = long.Parse(c.CallbackQuery.Data!.Split('-').Last());

            var targetRvUser = await c.DbContext.RvUsers.FirstOrDefaultAsync(u => u.UserId == targetUserId, token);

            if (targetRvUser == null)
            {
                await Bot.Client.EditMessageTextAsync(
                    c.CallbackQuery.Message!.Chat,
                    c.CallbackQuery.Message.MessageId,
                    Language.Phrases[c.RvUser.Lang].Messages.Common.UserNotFound,
                    cancellationToken: token);
                return;
            }

            (string message, InlineKeyboardMarkup? keyboard) = await ProfileHelper.Profile(targetRvUser, c, c.CallbackQuery.Message!.Chat.Type, App.DefaultRightVision, token);

            await Bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                message,
                replyMarkup: keyboard,
                cancellationToken: token);
        }

        private async Task PermissionsCallback(CallbackContext c, CancellationToken token = default)
        {
            var targetUserId = long.Parse(c.CallbackQuery.Data!.Split('-').Last());
            await LocationsFront.PermissionsList(c, await c.DbContext.RvUsers.FirstAsync(u => u.UserId == targetUserId, token), c.CallbackQuery.Data!.Contains("minimized"), token);
        }

        private async Task PunishmentsHistoryCallback(CallbackContext c, CancellationToken token = default)
        {
            var targetUserId = long.Parse(c.CallbackQuery.Data!.Split('-').Last());
            await LocationsFront.PunishmentsHistory(c, await c.DbContext.RvUsers.FirstAsync(u => u.UserId == targetUserId, token), true, true, token);
        }

        #endregion

        #endregion
    }
}
