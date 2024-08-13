using DryIoc;
using RightVisionBotDb.Bot.Keyboards.InlineKeyboards;
using RightVisionBotDb.Models;
using RightVisionBotDb.Types;
using Telegram.Bot;

namespace RightVisionBotDb.Bot.Locations
{
    internal class Start : RvLocation
    {
        public Start()
        {
            Markup = App.Container.Resolve<InlineKeyboards>().СhooseLang;
            UserAdded += OnUserAdded;
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
