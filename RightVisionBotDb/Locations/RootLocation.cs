using Microsoft.EntityFrameworkCore;
using RightVisionBotDb.Data.Contexts;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Helpers;
using RightVisionBotDb.Models;
using RightVisionBotDb.Services;
using RightVisionBotDb.Singletons;
using RightVisionBotDb.Text;
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
            LocationService locationService,
            RvLogger rvLogger,
            LocationsFront locationsFront,
            ILogger logger)
            : base(bot, locationService, rvLogger, locationsFront, logger)
        {
            this
                .RegisterTextCommand("/start", StartCommand)
                .RegisterTextCommand("/hide", HideCommand)
                .RegisterTextCommand("/menu", MainMenuCommand)
                .RegisterTextCommand("назначить", AppointCommand, Permission.Grant)
                .RegisterTextCommand("/news", NewsCommand, Permission.News)
                .RegisterTextCommand("/profile", ProfileCommand)
                .RegisterCallbackCommand("profile", ProfileCallback)
                .RegisterCallbackCommand("participations", ParticipationsCallback)
                .RegisterCallbackCommand("rvProperties", RvPropertiesCallback)
                .RegisterCallbackCommand("useControlPanel", UseControlPanelCallback)
                .RegisterCallbackCommand("backToProfile", BackToProfileCallback)
                .RegisterCallbackCommand("punishments_history", PunishmentsHistoryCallback)
                .RegisterCallbackCommands(["permissions_minimized", "permissions_maximized"], PermissionsCallback)
                .RegisterCallbackCommands(["punishments_hide", "punishments_show"], PunishmentsListActionCallback);
        }

        #endregion

        #region RootLocationBase overrides

        public override async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken token = default)
        {
            var db = DatabaseHelper.GetApplicationDbContext();
            var rvContext = DatabaseHelper.GetRightVisionContext(App.Configuration.RightVisionSettings.DefaultRightVision);
            var academyContext = DatabaseHelper.GetAcademyDbContext(App.Configuration.AcademySettings.DefaultAcademy);

            RvUser? rvUser = null;
            try
            {
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

                    var callbackContext = new CallbackContext(rvUser, callbackQuery, db, rvContext, academyContext);

                    containsArgs = callbackContext.CallbackQuery.Data!.Contains('-');

                    await HandleCallbackAsync(callbackContext, containsArgs, token);

                    if (chatType != ChatType.Private)
                        await LocationService[nameof(PublicChat)].HandleCallbackAsync(callbackContext, containsArgs, token);

                    else
                        await rvUser.Location.HandleCallbackAsync(callbackContext, containsArgs, token);

                    if (chatType == ChatType.Private && rvUser.Lang == Lang.Na)
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

                    if (message.Caption != null)
                        message.Text = message.Caption;

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

                    var commandContext = new CommandContext(rvUser, message, db, rvContext, academyContext);

                    containsArgs = commandContext.Message.Text != null && commandContext.Message.Text.Contains(' ');

                    await HandleCommandAsync(commandContext, containsArgs, token);

                    if (chatType == ChatType.Private && rvUser.Lang == Lang.Na)
                    {
                        await Bot.Client.SendTextMessageAsync(message.Chat, "Choose Lang:", replyMarkup: KeyboardsHelper.СhooseLang, cancellationToken: token);
                    }
                    else
                    {
                        if (chatType != ChatType.Private)
                            await LocationService[nameof(PublicChat)].HandleCommandAsync(commandContext, containsArgs, token);

                        else
                            await rvUser.Location.HandleCommandAsync(commandContext, containsArgs, token);
                    }
                }

                rvUser.LocationChanged -= OnLocationChanged;

                async void OnLocationChanged(object? sender, (RvLocation, RvLocation) e)
                {
                    await RvLogger.Log(LogMessagesHelper.UserChangedLocation(rvUser, e), rvUser, token);
                }

                if (db.ChangeTracker.HasChanges())
                    await db.SaveChangesAsync(token);

                if (rvContext.ChangeTracker.HasChanges())
                    await rvContext.SaveChangesAsync(token);

                if (academyContext.ChangeTracker.HasChanges())
                    await academyContext.SaveChangesAsync(token);
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
            finally
            {
                await db.DisposeAsync();
                await rvContext.DisposeAsync();
                await academyContext.DisposeAsync();
            }
        }

        public override Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken token = default)
        {
            _logger.Fatal(exception, "Не удалось обработать ошибку внутри работы бота");
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

            var rvUser = await context.RvUsers.FirstOrDefaultAsync(u => u.UserId == userId, token);
            var from = update.CallbackQuery?.From ?? update.Message?.From;

            if (from == null)
                throw new ArgumentNullException(nameof(from));

            if (rvUser == null)
            {
                rvUser = new RvUser(from.Id, Lang.Na, from.FirstName, from.Username, LocationService[nameof(Start)]);
                context.RvUsers.Add(rvUser);
                await context.SaveChangesAsync(token);
            }

            var chatType = update.CallbackQuery?.Message?.Chat.Type ?? update.Message?.Chat.Type ?? ChatType.Private;

            return (from, rvUser, chatType);

        }

        private async Task PunishmentsListActionCallback(CallbackContext c, CancellationToken token = default)
        {
            var args = c.CallbackQuery.Data!.Split('-'); //[0]Command, [1]PunishmentType, [2]UserId
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
                var location = LocationService[nameof(Start)];
                if (c.RvUser == null)
                {
                    var rvUser = new RvUser(c.Message.From!.Id, Lang.Na, c.Message.From.FirstName, c.Message.From.Username, location);
                    await c.DbContext.RvUsers.AddAsync(rvUser, token);
                }
                else
                {
                    c.RvUser.Location = location;
                    c.RvUser.Lang = Lang.Na;
                }
            }
        }

        private async Task ProfileCommand(CommandContext c, CancellationToken token)
        {
            var args = c.Message.Text!.Split(' ');

            var (extractedRvUser, _) = args.Length > 1 || c.Message.ReplyToMessage != null
                ? await CommandFormatHelper.ExtractRvUserFromArgs(c, token)
                : (c.RvUser, null);

            if (extractedRvUser == null)
            {
                await Bot.Client.SendTextMessageAsync(c.Message.Chat, Phrases.Lang[c.RvUser.Lang].Messages.Common.UserNotFound, cancellationToken: token);
                return;
            }

            (string message, InlineKeyboardMarkup? keyboard) = await ProfileHelper.Profile(extractedRvUser, c, c.Message.Chat.Type, App.Configuration.RightVisionSettings.DefaultRightVision, token: token);

            await Bot.Client.SendTextMessageAsync(
                c.Message.Chat,
                message,
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
            if (c.Message.Chat.Type == ChatType.Private)
            {
                c.RvUser.Location = LocationService[nameof(MainMenu)];
                await Bot.Client.SendTextMessageAsync(c.Message.Chat, "✅", replyMarkup: new ReplyKeyboardRemove(), cancellationToken: token);
                await Bot.Client.SendTextMessageAsync(
                    c.Message.Chat,
                    string.Format(Phrases.Lang[c.RvUser.Lang].Messages.Common.Greetings, c.RvUser.Name),
                    replyMarkup: KeyboardsHelper.MainMenu(c.RvUser),
                    cancellationToken: token);
            }
        }

        private async Task AppointCommand(CommandContext c, CancellationToken token = default)
        {
            var (extractedRvUser, args) = await CommandFormatHelper.ExtractRvUserFromArgs(c, token);

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
                        extractedRvUser.UserPermissions += PermissionsHelper.Layouts[role];
                        message = $"Пользователь успешно назначен на должность {role}!";
                        await Bot.Client.SendTextMessageAsync(
                            extractedRvUser.UserId,
                            string.Format(Phrases.Lang[extractedRvUser.Lang].Messages.Common.UserAppointed, extractedRvUser.Name, role),
                            cancellationToken: token);
                        ((ApplicationDbContext)c.DbContext).Entry(extractedRvUser).State = EntityState.Modified;
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

        private async Task NewsCommand(CommandContext c, CancellationToken token)
        {
            var targetUsers = new HashSet<long>();
            var sb = new StringBuilder();
            var commandParts = c.Message.Text!.Split(' ');
            var commandArgs = commandParts.Skip(1).Take(3).ToList();
            var argsCount = 1;

            sb.AppendLine("Распознаны следующие аргументы:");
            if (commandArgs.Contains("-n"))
            {
                sb.AppendLine("- Разослать подписчикам на новости (-n)");
                foreach (var rvUser in (await c.DbContext.RvUsers.ToListAsync(token)).Where(u => u.Has(Permission.News)))
                    targetUsers.Add(rvUser.UserId);
                commandArgs.Remove("-n");
                argsCount++;
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
                argsCount++;
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
                argsCount++;
            }
            await RvLogger.Log(LogMessagesHelper.UserStartedNewsSending(c.RvUser, sb.ToString()), c.RvUser, token);
            var message = string.Join(' ', commandParts.Skip(argsCount));


            var thread = new Thread(async () =>
            {
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
            });
            thread.Start();
        }

        private async Task ProfileCallback(CallbackContext c, CancellationToken token = default)
        {
            await LocationsFront.Profile(c, token, false);
        }

        private async Task ParticipationsCallback(CallbackContext c, CancellationToken token = default)
        {
            var args = c.CallbackQuery.Data!.Split('-'); //[0]Command, [1]UserId, [2]RightVision
            var targetRvUser = c.DbContext.RvUsers.First(u => u.UserId == long.Parse(args[1]));
            var (content, markup) = await ProfileHelper.RvUserParticipations(c, targetRvUser);

            await Bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                content,
                replyMarkup: markup,
                cancellationToken: token);
        }

        private async Task RvPropertiesCallback(CallbackContext c, CancellationToken token = default)
        {
            var rightvision = c.CallbackQuery.Data!.Split('-').Last();
            using var rvdb = DatabaseHelper.GetRightVisionContext(rightvision);
            var phrases = Phrases.Lang[c.RvUser.Lang];
            var endDateString = rvdb.EndDate?.ToString("d", new CultureInfo("ru-RU")) ?? phrases.Messages.Additional.Present;

            await Bot.Client.AnswerCallbackQueryAsync(c.CallbackQuery.Id, $"{rightvision}" +
                $"\n" +
                $"\n{phrases.Profile.RvProperties.Date}{rvdb.StartDate.ToString("d", new CultureInfo("ru-RU"))} - {endDateString}" +
                $"\n{phrases.Profile.RvProperties.ParticipantsCount}{rvdb.ParticipantForms.Count(p => p.Status == FormStatus.Accepted)}",
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
                    Phrases.Lang[c.RvUser.Lang].Messages.Common.UserNotFound,
                    cancellationToken: token);
                return;
            }

            (string message, InlineKeyboardMarkup? keyboard) = await ProfileHelper.Profile(targetRvUser, c, c.CallbackQuery.Message!.Chat.Type, App.Configuration.RightVisionSettings.DefaultRightVision, token: token);

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
