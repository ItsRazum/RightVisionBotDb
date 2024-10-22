using RightVisionBotDb.Lang;
using RightVisionBotDb.Models;
using RightVisionBotDb.Types;
using Telegram.Bot;

namespace RightVisionBotDb.Services
{
    public sealed class LocationsFront
    {
        #region Properties

        private Bot Bot { get; set; }
        private LocationManager LocationManager { get; set; }
        private Keyboards Keyboards { get; set; }
        private ProfileStringService ProfileStringService { get; set; }
        private CriticFormService CriticFormService { get; set; }

        #endregion

        public LocationsFront(
            Bot bot, 
            LocationManager locationManager, 
            Keyboards keyboards, 
            ProfileStringService profileStringService,
            CriticFormService criticFormService)
        {
            Bot = bot;
            LocationManager = locationManager;
            Keyboards = keyboards;
            ProfileStringService = profileStringService;
            CriticFormService = criticFormService;
        }

        public async Task MainMenu(CallbackContext c, CancellationToken token = default)
        {            
            c.RvUser.Location = LocationManager[nameof(Locations.MainMenu)];
            await Bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                string.Format(Language.Phrases[c.RvUser.Lang].Messages.Common.Greetings, c.RvUser.Name),
                replyMarkup: Keyboards.MainMenu(c.RvUser),
                cancellationToken: token);
        }

        public async Task Profile(CallbackContext c, CancellationToken token = default)
        {
            c.RvUser.Location = LocationManager[nameof(Locations.Profile)];
            await Bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                ProfileStringService.Private(c.RvUser, App.DefaultRightVision),
                replyMarkup: await Keyboards.Profile(c.RvUser, c.CallbackQuery.Message!.Chat.Type, App.DefaultRightVision, c.RvUser.Lang),
                cancellationToken: token);
        }

        public async Task FormSelection(CallbackContext c, CancellationToken token = default)
        {
            await Bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                Language.Phrases[c.RvUser.Lang].Messages.Common.SendFormRightNow,
                replyMarkup: Keyboards.FormSelection(c.RvUser),
                cancellationToken: token);
        }

        public async Task PermissionsList(CallbackContext c, RvUser targetRvUser, bool minimize, CancellationToken token = default)
        {
            await Bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                ProfileStringService.Permissions(c, c.DbContext.RvUsers.First(u => u == targetRvUser), minimize, c.RvUser.Lang),
                replyMarkup: Keyboards.PermissionsList(targetRvUser, minimize, targetRvUser.Permissions.Count > 10, c.RvUser.Lang),
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

            await Bot.Client.SendTextMessageAsync(
                c.CallbackQuery.Message!.Chat,
                CriticFormService.Messages[messageKey](c.RvUser.Lang),
                replyMarkup: Keyboards.ReplyBack(c.RvUser.Lang),
                cancellationToken: token);
        }
    }
}
