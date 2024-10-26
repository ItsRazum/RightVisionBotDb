using RightVisionBotDb.Lang;
using RightVisionBotDb.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBotDb.Helpers
{
    public class ControlPanelHelper
    {
        public static (string, InlineKeyboardMarkup) MainPage(RvUser rvUser)
        {
            return (Language.Phrases[rvUser.Lang].ControlPanel.Messages.Welcome, KeyboardsHelper.ControlPanelMainMenu(rvUser));
        }

        public static (string, InlineKeyboardMarkup) UsersExplorer(RvUser rvUser)
        {
            
        }
    }
}
