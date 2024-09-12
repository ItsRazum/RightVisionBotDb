using Newtonsoft.Json.Linq;
using RightVisionBotDb.Lang;
using RightVisionBotDb.Models;
using Telegram.Bot.Types;
using Telegram.Bot;
using RightVisionBotDb.Data;

namespace RightVisionBotDb.Services
{
    public sealed class LocationsFront
    {
        #region Properties

        private Bot Bot { get; set; }
        private LocationManager LocationManager { get; set; }
        private Keyboards Keyboards { get; set; }
        private ProfileStringService ProfileStringService { get; set; }

        #endregion

        public LocationsFront(Bot bot, LocationManager locationManager, Keyboards keyboards, ProfileStringService profileStringService) 
        {
            Bot = bot;
            LocationManager = locationManager;
            Keyboards = keyboards;
            ProfileStringService = profileStringService;
        }

        public async Task MainMenu(RvUser rvUser, CallbackQuery callbackQuery, ApplicationDbContext context, CancellationToken token = default)
        {
            rvUser.Location = LocationManager["MainMenu"];
            await Bot.Client.EditMessageTextAsync(
                callbackQuery.Message!.Chat,
                callbackQuery.Message.MessageId,
                string.Format(Language.Phrases[rvUser.Lang].Messages.Common.Greetings, rvUser.Name),
                replyMarkup: Keyboards.Hub(rvUser),
                cancellationToken: token);
            await context.SaveChangesAsync(token);
        }

        public async Task Profile(RvUser rvUser, CallbackQuery callbackQuery, ApplicationDbContext context, CancellationToken token = default)
        {
            rvUser.Location = LocationManager["Profile"];
            await Bot.Client.EditMessageTextAsync(
                callbackQuery.Message!.Chat,
                callbackQuery.Message.MessageId,
                ProfileStringService.Private(rvUser, App.DefaultRightVision),
                replyMarkup: Keyboards.Profile(rvUser, callbackQuery.Message!.Chat.Type, rvUser.Lang),
                cancellationToken: token);
            await context.SaveChangesAsync(token);
        }
    }
}
