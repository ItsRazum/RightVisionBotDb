using DryIoc;
using RightVisionBotDb.Bot.Keyboards.InlineKeyboards;
using RightVisionBotDb.Bot.Lang;
using RightVisionBotDb.Models;
using RightVisionBotDb.Types;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace RightVisionBotDb.Bot.Locations
{
    internal class Start : RvLocation
    {
        public Start(Bot bot)
        {
            Markup = App.Container.Resolve<InlineKeyboards>().СhooseLang;
            UserAdded += OnUserAdded;
            _bot = bot;
        }

        private Bot _bot;

        public override async Task HandleCallbackAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var rvUser = _bot.Core.GetRvUser(callbackQuery.From.Id);

            if (rvUser != null)
                switch (callbackQuery.Data) 
                {
                    case "Ru":
                    case "Ua":
                    case "Kz":
                        rvUser.Lang = Enum.Parse<Enums.Lang>(callbackQuery.Data);
                        await _bot.Client.DeleteMessageAsync(rvUser.UserId, callbackQuery.Message!.MessageId, cancellationToken);
                        await _bot.Client.SendTextMessageAsync(rvUser.UserId, Language.Phrases[rvUser.Lang].Messages.Common.Greetings);
                        break;
                }
        }

        public override Task HandleCommandAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public override string Text(Enums.Lang lang) => "Choose Lang";

        public override void AddNewUser(RvUser rvUser)
        {
            RvUsers.Add(rvUser);
            rvUser.Location = this;
            InvokeUserAdded(this, rvUser);
        }

        public async void OnUserAdded(object? sender, RvUser rvUser)
        {
            var bot = App.Container.Resolve<Bot>();
            await bot.Client.SendTextMessageAsync(rvUser.UserId, Text(rvUser.Lang), replyMarkup: Markup);
        }
    }
}
