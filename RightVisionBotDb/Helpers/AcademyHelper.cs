using RightVisionBotDb.Enums;
using RightVisionBotDb.Text;
using RightVisionBotDb.Types;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBotDb.Helpers
{
    public static class AcademyHelper
    {
        public static(string content, InlineKeyboardMarkup? keyboard) MainMenu(CallbackContext c)
        {
            var phrases = Phrases.Lang[c.RvUser.Lang];
            var rvUser = c.RvUser;
            var role = rvUser.Role;

            if (role is not (Role.Teacher or Role.Student))
                role = Role.None;

            var sb = new StringBuilder(phrases.Academy.Greetings)
                .AppendLine(phrases.Academy.Properties.Role + Phrases.GetUserRoleString(role, c.RvUser.Lang));

            if (!rvUser.Has(Permission.Academy))
            {
                sb
                    .AppendLine()
                    .AppendLine(phrases.Academy.Accesses.NoAccess);

                return (sb.ToString(), KeyboardsHelper.InlineBack(rvUser.Lang));
            }
            
            return (string.Empty, null);
        }
    }
}
