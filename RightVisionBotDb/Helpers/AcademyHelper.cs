using RightVisionBotDb.Enums;
using RightVisionBotDb.Text;
using RightVisionBotDb.Types;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBotDb.Helpers
{
    public static class AcademyHelper
    {
        public static (string content, InlineKeyboardMarkup? keyboard) MainMenu(CallbackContext c)
        {
            var rvUser = c.RvUser;
            var phrases = Phrases.Lang[rvUser.Lang];

            var role = rvUser.Role is Role.Teacher or Role.Student
                ? rvUser.Role
                : Role.None;

            var sb = new StringBuilder(phrases.Academy.Greetings)
                .AppendLine(phrases.Academy.Properties.Role + Phrases.GetUserRoleString(role, rvUser.Lang))
                .AppendLine();

            if (!rvUser.Has(Permission.Academy))
            {
                sb.AppendLine(phrases.Academy.Accesses.NoAccess);
                return (sb.ToString(), KeyboardsHelper.InlineBack(rvUser.Lang));
            }

            var message = role switch
            {
                Role.Student => phrases.Academy.Accesses.Student,
                Role.Teacher => phrases.Academy.Accesses.Teacher,
                _ => phrases.Academy.Accesses.Allowed
            };

            sb.AppendLine(message);

            return (string.Empty, null);
        }
    }
}
