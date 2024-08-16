using RightVisionBotDb.Lang;
using RightVisionBotDb.Common;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Models;
using System.Text;
using Telegram.Bot.Types.Enums;

namespace RightVisionBotDb.Services
{
    internal static class Profile
    {
        /*
        public static string Get(RvUser user, Enums.Lang lang, ChatType type)
        {
            string category = user.Category switch
            {
                Category.Bronze => "🥉Bronze",
                Category.Silver => "🥈Silver",
                Category.Gold => "🥇Gold",
                Category.Brilliant => "💎Brilliant",
                _ => string.Empty
            };

            StringBuilder sb;
            if (type is ChatType.Private)
            {
                sb = new StringBuilder();
                sb.AppendLine(Language.GetPhrase("Profile_Private_Header", lang));

                //Статус и должность
                if (user.Role is Role.None)
                    sb.AppendLine(Language.GetUserStatusString(user.Status, lang));
                else
                    sb.AppendLine(Language.GetUserStatusString(user.Status, lang)
                        + "\n"
                        + string.Format(Language.GetPhrase("Profile_Role", lang), Language.GetUserRoleString(user.Role, lang)));

                //Подписка на новости
                sb.AppendLine(string.Format(Language.GetPhrase("Profile_Sending_Status", lang),
                    Language.GetPhrase(!user.Has(Permission.Sending) 
                    ? "Profile_Sending_Status_Inactive" 
                    : "Profile_Sending_Status_Active", lang)));

                if(user.Status is not Status.User)
                {
                    sb.AppendLine(user.Status switch
                    {
                        Status.Member 
                        or Status.ExMember => string.Format(
                            "\n" + Language.GetPhrase("Profile_Member_Layout", lang),
                            category,
                            RvMember.Get(UserId).TrackStr),
                        Status.Critic => string.Format(
                            "\n" + Language.GetPhrase("Profile_Critic_Layout", lang),
                            category),
                        Status.CriticAndMember 
                        or Status.CriticAndExMember => string.Format(
                            "\n" + Language.GetPhrase("Profile_CriticAndMember_Layout", lang),
                            category,
                            RvMember.Get(UserId).TrackStr),
                            _ => "Неизвестная ошибка"
                    });

                    return sb.ToString();
                };
            }
        }
        */

        /*
        private static string GetCandidateStatus(long userId, Role role)
        {
            try
            {
                var query = $"SELECT `status` FROM RV_{role}s WHERE `userId` = {userId};";
                var idList = Database.Read(query, "status");
                return idList.FirstOrDefault() switch
                {
                    "denied" => Language.GetPhrase("Profile_Form_Status_Blocked", RvUser.Get(userId).Lang),
                    "waiting" => Language.GetPhrase("Profile_Form_Status_Waiting", RvUser.Get(userId).Lang),
                    "unfinished" => Language.GetPhrase("Profile_Form_Status_Unfinished", RvUser.Get(userId).Lang),
                    null => Language.GetPhrase("Profile_Form_Status_Allowed", RvUser.Get(userId).Lang),
                    "bronze" => Language.GetPhrase("Profile_Form_Status_Accepted", RvUser.Get(userId).Lang),
                    "silver" => Language.GetPhrase("Profile_Form_Status_Accepted", RvUser.Get(userId).Lang),
                    "gold" => Language.GetPhrase("Profile_Form_Status_Accepted", RvUser.Get(userId).Lang),
                    "brilliant" => Language.GetPhrase("Profile_Form_Status_Accepted", RvUser.Get(userId).Lang),
                    _ => Language.GetPhrase("Profile_Form_Status_UnderConsideration", RvUser.Get(userId).Lang)
                };
            }
            catch
            {
                return "Не удалось получить статус!";
            }
        }
        */
    }
}
