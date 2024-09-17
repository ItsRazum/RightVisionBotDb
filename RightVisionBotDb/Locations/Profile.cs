using RightVisionBotDb.Data;
using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Lang;
using RightVisionBotDb.Models;
using RightVisionBotDb.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace RightVisionBotDb.Locations
{
    internal sealed class Profile : RvLocationBase, IRvLocation
    {

        #region Constructor

        public Profile(
            Bot bot,
            Keyboards inlineKeyboards,
            LocationManager locationManager,
            RvLogger logger,
            LogMessages logMessages,
            LocationsFront locationsFront)
            : base(bot, inlineKeyboards, locationManager, logger, logMessages, locationsFront)
        {
        }

        #endregion

        #region IRvLocation implementation

        public async Task HandleCallbackAsync(CallbackQuery callbackQuery, RvUser rvUser, ApplicationDbContext context, CancellationToken token)
        {
            switch (callbackQuery.Data)
            {
                case "mainmenu":
                    rvUser.Location = LocationManager["MainMenu"];
                    await Bot.Client.EditMessageTextAsync(
                        callbackQuery.Message!.Chat,
                        callbackQuery.Message.MessageId,
                        string.Format(Language.Phrases[rvUser.Lang].Messages.Common.Greetings, rvUser.Name),
                        replyMarkup: InlineKeyboards.MainMenu(rvUser),
                        cancellationToken: token);
                    await context.SaveChangesAsync(token);
                    break;
                case "forms":
                    await Bot.Client.EditMessageTextAsync(
                        callbackQuery.Message!.Chat,
                        callbackQuery.Message.MessageId,
                        Language.Phrases[rvUser.Lang].Messages.Common.SendFormRightNow,
                        replyMarkup: InlineKeyboards.About(rvUser),
                        cancellationToken: token);
                    break;
                case "back":

                    break;
            }
        }

        public Task HandleCommandAsync(Message message, RvUser rvUser, ApplicationDbContext context, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public override string ToString()
        {
            return LocationManager.LocationToString(this);
        }

        #endregion

    }
}
