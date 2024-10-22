using RightVisionBotDb.Enums;
using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Models;
using RightVisionBotDb.Services;
using RightVisionBotDb.Types;
using Serilog;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace RightVisionBotDb.Locations
{
    public sealed class RootLocation : RootLocationBase
    {

        #region Constructor

        public RootLocation(
            Bot bot,
            Keyboards keyboards,
            LocationManager locationManager,
            RvLogger rvLogger,
            LogMessages logMessages,
            LocationsFront locationsFront,
            ILogger logger,
            DatabaseService databaseService)
            : base(bot, keyboards, locationManager, rvLogger, logMessages, locationsFront, logger, databaseService)
        {
            this
                .RegisterTextCommand("/start", StartCommand);

            this
                .RegisterCallbackCommand("rvProperties", RvPropertiesCallback);
        }

        #endregion

        public override async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken token = default)
        {
            var message = update.Message;
            var callbackQuery = update.CallbackQuery;

            RvUser? rvUser = null;
            using var db = DatabaseService.GetApplicationDbContext();

            ChatType chatType;
            bool containsArgs = false;

            try
            {
                if (callbackQuery != null)
                {
                    _logger.Information("=== {0} ===" +
                        $"\nCallbackId: {callbackQuery.Id}" +
                        $"\nCallbackData: {callbackQuery.Data}" +
                        $"\n" +
                        $"\nId отправителя: {callbackQuery.From.Id}" +
                        $"\nUsername отправителя: {"@" + callbackQuery.From.Username}" +
                        $"\nИмя отправителя: {callbackQuery.From.FirstName}" +
                        $"\n" +
                        $"\nЧат: {callbackQuery.Message?.Chat.Type}" +
                        $"\nId чата: {callbackQuery.Message?.Chat.Id}" +
                        $"\n", "Входящий Callback");

                    rvUser = db.RvUsers.FirstOrDefault(u => u.UserId == callbackQuery.From.Id);
                    if (rvUser != null)
                    {
                        var callbackContext = new CallbackContext(rvUser, callbackQuery, db);

                        containsArgs = callbackContext.CallbackQuery.Data!.Contains('-');
                        chatType = callbackQuery.Message!.Chat.Type;

                        await HandleCallbackAsync(callbackContext, containsArgs, token);

                        rvUser.LocationChanged += OnLocationChanged;

                        if (chatType != ChatType.Private)
                            await LocationManager[nameof(PublicChat)].HandleCallbackAsync(callbackContext, containsArgs, token);

                        else
                            await rvUser.Location.HandleCallbackAsync(callbackContext, containsArgs, token);
                    }
                    else
                    {
                        if (callbackQuery.Message!.Chat.Type == ChatType.Private)
                            await botClient.DeleteMessageAsync(callbackQuery.Message.Chat, callbackQuery.Message.MessageId, token);

                        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "Похоже, твои данные повреждены или утеряны. Для дальнейшего взаимодействия с ботом тебе необходимо повторно выбрать свой язык", cancellationToken: token);
                        rvUser = new RvUser(callbackQuery.From.Id, Enums.Lang.Na, callbackQuery.From.FirstName, callbackQuery.From.Username, LocationManager[nameof(Start)]);
                        var success = false;
                        try
                        {
                            await botClient.SendTextMessageAsync(rvUser.UserId, "Choose Lang:", replyMarkup: Keyboards.СhooseLang, cancellationToken: token);
                            success = true;
                        }
                        catch
                        {
                            _logger.Warning("Произошла попытка отправить сообщение пользователю, с которым ещё не было чата!");
                        }

                        if (success)
                        {
                            db.RvUsers.Add(rvUser);
                            await db.SaveChangesAsync(token);
                        }
                    }
                }
                else if (message != null && message.From != null)
                {
                    _logger.Information("=== {0} ===" +
                        $"\nId отправителя: {message.From?.Id}" +
                        $"\nUsername отправителя: {"@" + message.From?.Username}" +
                        $"\nИмя отправителя: {message.From?.FirstName}" +
                        $"\n" +
                        $"\nТекст сообщения: {message.Text}" +
                        $"\n" +
                        $"\nЧат: {message.Chat.Type}" +
                        $"\nId чата: {message.Chat.Id}" +
                        $"\n", "Входящее сообщение");

                    rvUser = db.RvUsers.FirstOrDefault(u => u.UserId == message.From!.Id);

                    if (rvUser != null)
                    {
                        var commandContext = new CommandContext(rvUser, message, db);

                        containsArgs = commandContext.Message.Text != null && commandContext.Message.Text.Contains(' ');
                        chatType = message.Chat.Type;

                        await HandleCommandAsync(commandContext, containsArgs, token);

                        rvUser.LocationChanged += OnLocationChanged;

                        if (chatType != ChatType.Private)
                            await LocationManager[nameof(PublicChat)].HandleCommandAsync(commandContext, containsArgs, token);

                        else
                            await rvUser.Location.HandleCommandAsync(commandContext, containsArgs, token);
                    }
                }
                if (rvUser != null)
                    rvUser.LocationChanged -= OnLocationChanged;

                async void OnLocationChanged(object? sender, (IRvLocation, IRvLocation) e)
                {
                    await RvLogger.Log(LogMessages.UserChangedLocation(rvUser, e), rvUser, token);
                }

                if (db.ChangeTracker.HasChanges())
                    await db.SaveChangesAsync(token);
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

        private async Task RvPropertiesCallback(CallbackContext c, CancellationToken token = default)
        {
            var rightvision = c.CallbackQuery.Data!.Split('-').Last();
            using var rvdb = DatabaseService.GetRightVisionContext(rightvision);
            var endDateString = rvdb.EndDate?.ToString("d", new CultureInfo("ru-RU")) ?? "н.в.";

            await Bot.Client.AnswerCallbackQueryAsync(c.CallbackQuery.Id, $"{rightvision}" +
                $"\n" +
                $"\nДата проведения: {rvdb.StartDate.ToString("d", new CultureInfo("ru-RU"))} - {endDateString}" +
                $"\nКоличество участников: {rvdb.ParticipantForms.Count(p => p.Status == FormStatus.Accepted)}",
                true,
                cancellationToken: token);
        }
    }
}
