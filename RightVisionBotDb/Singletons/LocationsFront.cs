using Microsoft.EntityFrameworkCore;
using RightVisionBotDb.Helpers;
using RightVisionBotDb.Locations;
using RightVisionBotDb.Models;
using RightVisionBotDb.Services;
using RightVisionBotDb.Text;
using RightVisionBotDb.Types;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBotDb.Singletons
{
    public sealed class LocationsFront
    {
        #region Properties

        private Bot Bot { get; }
        private LocationService LocationService { get; }
        private CriticFormService CriticFormService { get; }
        private ParticipantFormService ParticipantFormService { get; }
        private StudentFormService StudentFormService { get; }

        #endregion

        public LocationsFront(
            Bot bot,
            LocationService locationService,
            CriticFormService criticFormService,
            ParticipantFormService participantFormService,
            StudentFormService studentFormService)
        {
            Bot = bot;
            LocationService = locationService;
            CriticFormService = criticFormService;
            ParticipantFormService = participantFormService;
            StudentFormService = studentFormService;
        }

        public async Task MainMenu(CallbackContext c, CancellationToken token = default)
        {
            c.RvUser.Location = LocationService[nameof(Locations.MainMenu)];
            await Bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                string.Format(Phrases.Lang[c.RvUser.Lang].Messages.Common.Greetings, c.RvUser.Name),
                replyMarkup: KeyboardsHelper.MainMenu(c.RvUser),
                cancellationToken: token);
        }

        public async Task Profile(CallbackContext c, CancellationToken token, bool changeLocation = true)
        {
            if (changeLocation) c.RvUser.Location = LocationService[nameof(Locations.Profile)];

            var targetUserId = c.RvUser.UserId;
            var args = c.CallbackQuery.Data!.Split('-'); //[0]Command, [1]UserId, [2]?RightVision

            if (args.First() == "profile")
                targetUserId = long.Parse(args[1]);

            var rightvision = args.Length < 3 ? App.Configuration.RightVisionSettings.DefaultRightVision : args[2];
            var (content, keyboard) = await ProfileHelper.Profile(await c.DbContext.RvUsers.FirstAsync(u => u.UserId == targetUserId, token), c, c.CallbackQuery.Message!.Chat.Type, rightvision, token: token);
            await Bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                content,
                replyMarkup: keyboard,
                cancellationToken: token);
        }

        public async Task FormSelection(CallbackContext c, CancellationToken token = default)
        {
            await Bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                Phrases.Lang[c.RvUser.Lang].Messages.Common.SendFormRightNow,
                replyMarkup: KeyboardsHelper.FormSelection(c.RvUser),
                cancellationToken: token);
        }

        public async Task PermissionsList(CallbackContext c, RvUser targetRvUser, bool minimize, CancellationToken token = default)
        {
            (string content, InlineKeyboardMarkup? keyboard) = ProfileHelper.RvUserPermissions(c.RvUser, targetRvUser, minimize);
            await Bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                content,
                replyMarkup: keyboard,
                cancellationToken: token);
        }

        public async Task PunishmentsHistory(CallbackContext c, RvUser targetRvUser, bool showBans, bool showMutes, CancellationToken token = default)
        {
            if (targetRvUser.Punishments.Count == 0)
            {
                await Bot.Client.AnswerCallbackQueryAsync(
                    c.CallbackQuery.Id,
                    Phrases.Lang[c.RvUser.Lang].Profile.Punishments.Punishment.NoPunishments,
                    showAlert: true,
                    cancellationToken: token);

                return;
            }

            (string content, InlineKeyboardMarkup? keyboard) = ProfileHelper.RvUserPunishments(c, targetRvUser, showBans, showMutes);

            await Bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                content,
                replyMarkup: keyboard,
                cancellationToken: token);
        }

        public async Task CriticForm(CallbackContext c, int messageKey, CancellationToken token = default)
        {
            c.RvUser.Location = LocationService[nameof(Locations.CriticFormLocation)];
            await Bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                Phrases.Lang[c.RvUser.Lang].Messages.Common.StartingForm,
                cancellationToken: token);

            var (message, keyboard) = CriticFormService.Messages[messageKey](c.RvUser.Lang);

            await Bot.Client.SendTextMessageAsync(
                c.CallbackQuery.Message!.Chat,
                message,
                replyMarkup: keyboard,
                cancellationToken: token);
        }

        public async Task ParticipantForm(CallbackContext c, int messageKey, CancellationToken token = default)
        {
            c.RvUser.Location = LocationService[nameof(Locations.ParticipantFormLocation)];
            await Bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                Phrases.Lang[c.RvUser.Lang].Messages.Common.StartingForm,
                cancellationToken: token);

            var (message, keyboard) = ParticipantFormService.Messages[messageKey](c.RvUser.Lang);

            await Bot.Client.SendTextMessageAsync(
                c.CallbackQuery.Message!.Chat,
                message,
                replyMarkup: keyboard,
                cancellationToken: token);
        }

        public async Task StudentForm(CallbackContext c, int messageKey, CancellationToken token = default)
        {
            c.RvUser.Location = LocationService[nameof(StudentFormLocation)];
            await Bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                Phrases.Lang[c.RvUser.Lang].Messages.Common.StartingForm,
                cancellationToken: token);

            var (message, keyboard) = StudentFormService.Messages[messageKey](c.RvUser.Lang);

            await Bot.Client.SendTextMessageAsync(
                c.CallbackQuery.Message!.Chat,
                message,
                replyMarkup: keyboard,
                cancellationToken: token);
        }
     }
}
