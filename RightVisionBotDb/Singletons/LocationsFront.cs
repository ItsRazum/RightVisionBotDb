﻿using RightVisionBotDb.Helpers;
using RightVisionBotDb.Lang;
using RightVisionBotDb.Models;
using RightVisionBotDb.Services;
using RightVisionBotDb.Types;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBotDb.Singletons
{
    public sealed class LocationsFront
    {
        #region Properties

        private Bot Bot { get; set; }
        private LocationManager LocationManager { get; }
        private CriticFormService CriticFormService { get; }
        private ParticipantFormService ParticipantFormService { get; }

        #endregion

        public LocationsFront(
            Bot bot,
            LocationManager locationManager,
            CriticFormService criticFormService,
            ParticipantFormService participantFormService)
        {
            Bot = bot;
            LocationManager = locationManager;
            CriticFormService = criticFormService;
            ParticipantFormService = participantFormService;
        }

        public async Task MainMenu(CallbackContext c, CancellationToken token = default)
        {
            c.RvUser.Location = LocationManager[nameof(Locations.MainMenu)];
            await Bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                string.Format(Language.Phrases[c.RvUser.Lang].Messages.Common.Greetings, c.RvUser.Name),
                replyMarkup: KeyboardsHelper.MainMenu(c.RvUser),
                cancellationToken: token);
        }

        public async Task Profile(CallbackContext c, CancellationToken token = default)
        {
            c.RvUser.Location = LocationManager[nameof(Locations.Profile)];
            var profile = await ProfileHelper.Profile(c.RvUser, c.RvUser, c.CallbackQuery.Message!.Chat.Type, App.DefaultRightVision, token);
            await Bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                profile.content,
                replyMarkup: profile.keyboard,
                cancellationToken: token);
        }

        public async Task FormSelection(CallbackContext c, CancellationToken token = default)
        {
            await Bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                Language.Phrases[c.RvUser.Lang].Messages.Common.SendFormRightNow,
                replyMarkup: KeyboardsHelper.FormSelection(c.RvUser),
                cancellationToken: token);
        }

        public async Task PermissionsList(CallbackContext c, RvUser targetRvUser, bool minimize, CancellationToken token = default)
        {
            (string content, InlineKeyboardMarkup? keyboard) = ProfileHelper.RvUserPermissions(c, targetRvUser, minimize);
            await Bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                content,
                replyMarkup: keyboard,
                cancellationToken: token);
        }

        public async Task PunishmentsList(CallbackContext c, RvUser targetRvUser, CancellationToken token = default)
        {
            await Bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                ProfileHelper.RvUserPermissions(c, c.DbContext.RvUsers.First(u => u == targetRvUser), minimize, c.RvUser.Lang),
                replyMarkup: KeyboardsHelper.PermissionsList(targetRvUser, minimize, targetRvUser.UserPermissions.Count > 10, c.RvUser.Lang),
                cancellationToken: token);
        }

        public async Task CriticForm(CallbackContext c, int messageKey, CancellationToken token = default)
        {
            c.RvUser.Location = LocationManager[nameof(Locations.CriticFormLocation)];
            await Bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                Language.Phrases[c.RvUser.Lang].Messages.Common.StartingForm,
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
            c.RvUser.Location = LocationManager[nameof(Locations.ParticipantFormLocation)];
            await Bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                Language.Phrases[c.RvUser.Lang].Messages.Common.StartingForm,
                cancellationToken: token);

            var (message, keyboard) = ParticipantFormService.Messages[messageKey](c.RvUser.Lang);

            await Bot.Client.SendTextMessageAsync(
                c.CallbackQuery.Message!.Chat,
                message,
                replyMarkup: keyboard,
                cancellationToken: token);
        }
    }
}
