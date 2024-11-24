using RightVisionBotDb.Models;
using RightVisionBotDb.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBotDb.Helpers
{
    public class ControlPanelHelper
    {
        public static (string, InlineKeyboardMarkup) MainPage(RvUser rvUser)
        {
            return (Phrases.Lang[rvUser.Lang].ControlPanel.Messages.Welcome, KeyboardsHelper.ControlPanelMainMenu(rvUser));
        }

        /*
        public static (string, InlineKeyboardMarkup) UsersExplorer(RvUser rvUser, int page)
        {
            
        }
        */
    }
}
