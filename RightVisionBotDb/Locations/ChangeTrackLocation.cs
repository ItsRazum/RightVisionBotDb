using RightVisionBotDb.Helpers;
using RightVisionBotDb.Services;
using RightVisionBotDb.Singletons;
using RightVisionBotDb.Types;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBotDb.Locations
{
    internal class ChangeTrackLocation : RvLocation
    {
        public ChangeTrackLocation(
            Bot bot, 
            LocationService locationService,
            RvLogger logger, 
            LocationsFront locationsFront) 
            : base(bot, locationService, logger, locationsFront)
        {
            RegisterCallbackCommand("back", BackToProfileCallback);
        }

        public override async Task HandleCommandAsync(CommandContext c, bool containsArgs, CancellationToken token = default)
        {
            if (c.Message.Text == null) return;

            c.RvContext.ParticipantForms.First(p => p.UserId == c.Message.From!.Id).Track = c.Message.Text;

            await Bot.Client.DeleteMessageAsync(c.Message.Chat, c.Message.MessageId, token);

            (var content, var keyboard) = 
                await ProfileHelper.Profile(
                    c.RvUser,
                    c, Telegram.Bot.Types.Enums.ChatType.Private, 
                    App.Configuration.RightVisionSettings.DefaultRightVision, 
                    token: token);

            await Bot.Client.SendTextMessageAsync(c.Message.Chat, content, replyMarkup: keyboard, cancellationToken: token);

            c.RvUser.Location = LocationService[nameof(MainMenu)];
        }

        private async Task BackToProfileCallback(CallbackContext c, CancellationToken token)
        {
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
    }
}
