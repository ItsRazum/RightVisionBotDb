﻿using Microsoft.EntityFrameworkCore;
using RightVisionBotDb.Data.Contexts;
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
    public sealed class LocationsFront(
        Bot bot,
        LocationService LocationService,
        CriticFormService CriticFormService,
        ParticipantFormService ParticipantFormService,
        StudentFormService StudentFormService,
        TrackCardService TrackCardService)
    {
        public async Task MainMenu(CallbackContext c, CancellationToken token = default)
        {
            c.RvUser.Location = LocationService[nameof(Locations.MainMenu)];
            await bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                string.Format(Phrases.Lang[c.RvUser.Lang].Messages.Common.Greetings, c.RvUser.Name),
                replyMarkup: KeyboardsHelper.MainMenu(c.RvUser),
                cancellationToken: token);
        }

        public async Task MainMenu(CommandContext c, CancellationToken token = default)
        {
            c.RvUser.Location = LocationService[nameof(Locations.MainMenu)];
            await bot.Client.SendTextMessageAsync(
                c.Message!.Chat,
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
            await bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                content,
                replyMarkup: keyboard,
                cancellationToken: token);
        }

        public async Task FormSelection(CallbackContext c, CancellationToken token = default)
        {
            await bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                Phrases.Lang[c.RvUser.Lang].Messages.Common.SendFormRightNow,
                replyMarkup: KeyboardsHelper.FormSelection(c.RvUser),
                cancellationToken: token);
        }

        public async Task PermissionsList(CallbackContext c, RvUser targetRvUser, bool minimize, CancellationToken token = default)
        {
            (string content, InlineKeyboardMarkup? keyboard) = await ProfileHelper.RvUserPermissions(c, targetRvUser, minimize, token);
            await bot.Client.EditMessageTextAsync(
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
                await bot.Client.AnswerCallbackQueryAsync(
                    c.CallbackQuery.Id,
                    Phrases.Lang[c.RvUser.Lang].Profile.Punishments.Punishment.NoPunishments,
                    showAlert: true,
                    cancellationToken: token);

                return;
            }

            (string content, InlineKeyboardMarkup? keyboard) = ProfileHelper.RvUserPunishments(c, targetRvUser, showBans, showMutes);

            await bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                content,
                replyMarkup: keyboard,
                cancellationToken: token);
        }

        public async Task CriticForm(CallbackContext c, int messageKey, CancellationToken token = default)
        {
            c.RvUser.Location = LocationService[nameof(Locations.CriticFormLocation)];
            await bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                Phrases.Lang[c.RvUser.Lang].Messages.Common.StartingForm,
                cancellationToken: token);

            var (message, keyboard) = CriticFormService.Messages[messageKey](c.RvUser.Lang);

            await bot.Client.SendTextMessageAsync(
                c.CallbackQuery.Message!.Chat,
                message,
                replyMarkup: keyboard,
                cancellationToken: token);
        }

        public async Task ParticipantForm(CallbackContext c, int messageKey, CancellationToken token = default)
        {
            c.RvUser.Location = LocationService[nameof(ParticipantFormLocation)];
            await bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                Phrases.Lang[c.RvUser.Lang].Messages.Common.StartingForm,
                cancellationToken: token);

            var (message, keyboard) = ParticipantFormService.Messages[messageKey](c.RvUser.Lang);

            await bot.Client.SendTextMessageAsync(
                c.CallbackQuery.Message!.Chat,
                string.Format(message, App.Configuration.RightVisionSettings.DefaultRightVision),
                replyMarkup: keyboard,
                cancellationToken: token);
        }

        public async Task StudentForm(CallbackContext c, int messageKey, CancellationToken token = default)
        {
            c.RvUser.Location = LocationService[nameof(StudentFormLocation)];
            await bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                Phrases.Lang[c.RvUser.Lang].Messages.Common.StartingForm,
                cancellationToken: token);

            var (message, keyboard) = StudentFormService.Messages[messageKey](c.RvUser.Lang);

            await bot.Client.SendTextMessageAsync(
                c.CallbackQuery.Message!.Chat,
                message,
                replyMarkup: keyboard,
                cancellationToken: token);
        }

        public async Task EditTrack(CallbackContext c, CancellationToken token = default)
        {
            c.RvUser.Location = LocationService[nameof(ChangeTrackLocation)];
            await bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                Phrases.Lang[c.RvUser.Lang].Messages.Participant.EnterNewTrack,
                replyMarkup: KeyboardsHelper.InlineBack(c.RvUser.Lang),
                cancellationToken: token);
        }

        public async Task TrackCard(CallbackContext c, CancellationToken token = default)
        {
            var form = await c.RvContext.ParticipantForms.FirstAsync(p => p.UserId == c.RvUser.UserId, token);
            var userTrackCard = form.TrackCard;
            if (userTrackCard == null)
            {
                form.TrackCard = new();
                ((RightVisionDbContext)c.RvContext).Entry(form).State = EntityState.Modified;

                userTrackCard = form.TrackCard;
            }

            c.RvUser.Location = LocationService[nameof(TrackCardLocation)];
            await bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                TrackCardService.GetStatus(userTrackCard!),
                replyMarkup: KeyboardsHelper.InlineBack(c.RvUser.Lang),
                cancellationToken: token);
        }
     }
}
