using Microsoft.EntityFrameworkCore;
using RightVisionBotDb.Helpers;
using RightVisionBotDb.Models;
using RightVisionBotDb.Services;
using RightVisionBotDb.Singletons;
using RightVisionBotDb.Types;
using Telegram.Bot;

namespace RightVisionBotDb.Locations
{
    internal class TrackCardLocation : RvLocation
    {
        private readonly TrackCardService _trackCardService;

        public TrackCardLocation(
            Bot bot, 
            LocationService locationService, 
            RvLogger logger, 
            LocationsFront locationsFront,
            TrackCardService trackCardService) 
            : base(bot, locationService, logger, locationsFront)
        {
            _trackCardService = trackCardService;

            this
                .RegisterTextCommand("/track", TrackCommand)
                .RegisterTextCommand("/text", TextCommand)
                .RegisterTextCommand("/image", ImageCommand)
                .RegisterTextCommand("/visual", VisualCommand)
                .RegisterCallbackCommand("back", BackCallback);
        }

        private async Task VisualCommand(CommandContext c, CancellationToken token)
        {
            var trackCard = await GetParticipantTrackCard(c, token);
            if (trackCard != null)
                await _trackCardService.HandleVisualAsync(trackCard, c, token);
        }

        private async Task TrackCommand(CommandContext c, CancellationToken token)
        {
            var form = await GetParticipantTrackCard(c, token);
            if (form != null)
                await _trackCardService.HandleTrackAsync(form, c, token);
        }

        private async Task TextCommand(CommandContext c, CancellationToken token)
        {
            var form = await GetParticipantTrackCard(c, token);
            if (form != null)
                await _trackCardService.HandleTextAsync(form, c, token);
        }

        private async Task ImageCommand(CommandContext c, CancellationToken token)
        {
            var form = await GetParticipantTrackCard(c, token);
            if (form != null)
                await _trackCardService.HandleImageAsync(form, c, token);
        }

        private async Task BackCallback(CallbackContext c, CancellationToken token)
        {
            c.RvUser.Location = LocationService[nameof(MainMenu)];
            (var content, var keyboard) = await ProfileHelper.Profile(
                c.RvUser,
                c, Telegram.Bot.Types.Enums.ChatType.Private,
                App.Configuration.RightVisionSettings.DefaultRightVision,
                token: token);

            await Bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                content,
                replyMarkup: keyboard,
                cancellationToken: token);
        }

        private async Task<ParticipantForm> GetParticipantTrackCard(CommandContext c, CancellationToken token)
        {
            return await c.RvContext.ParticipantForms.FirstAsync(p => p.UserId == c.RvUser.UserId, token);
        }
    }
}
