using RightVisionBotDb.Data;
using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Lang;
using RightVisionBotDb.Models;
using RightVisionBotDb.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace RightVisionBotDb.Locations
{
    internal sealed class Start : RvLocationBase, IRvLocation
    {

        #region Constructor

        public Start(
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

        public async Task HandleCallbackAsync(CallbackQuery callbackQuery, RvUser rvUser, ApplicationDbContext context, CancellationToken token = default)
        {
            if (rvUser != null)
                switch (callbackQuery.Data)
                {
                    case "Ru":
                    case "Ua":
                    case "Kz":
                        rvUser.Lang = Enum.Parse<Enums.Lang>(callbackQuery.Data);
                        await Bot.Client.DeleteMessageAsync(rvUser.UserId, callbackQuery.Message!.MessageId, token);
                        await LocationsFront.MainMenu(rvUser, callbackQuery, context, token);
                        await Logger.Log(LogMessages.Registration(rvUser), rvUser, token);
                        break;
                }
        }

        public async Task HandleCommandAsync(Message message, RvUser rvUser, ApplicationDbContext context, CancellationToken token = default)
        {
            switch (message.Text)
            {
                //Также есть обработка на уровне бота в Bot.cs
                case "/start":
                    await Bot.Client.SendTextMessageAsync(message.Chat, "Choose Lang:", replyMarkup: InlineKeyboards.СhooseLang, cancellationToken: token);
                    break;
            }
        }

        public override string ToString()
        {
            return LocationManager.LocationToString(this);
        }

        #endregion

    }
}
