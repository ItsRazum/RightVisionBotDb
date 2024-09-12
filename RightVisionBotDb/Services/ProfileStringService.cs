using RightVisionBotDb.Enums;
using RightVisionBotDb.Extensions.Enums;
using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Lang;
using RightVisionBotDb.Models;
using RightVisionBotDb.Models.Forms;
using System.Text;

namespace RightVisionBotDb.Services
{
    public class ProfileStringService
    {
        private Bot Bot { get; set; }
        private DatabaseService DatabaseService { get; set; }

        public ProfileStringService(Bot bot, DatabaseService databaseService)
        {
            Bot = bot;
            DatabaseService = databaseService;
        }

        public string Public(RvUser user, Enums.Lang lang, string rightvision)
        {
            var phrases = Language.Phrases[lang];
            StringBuilder sb;
            sb = new StringBuilder();
            sb.AppendLine(phrases.Profile.Headers.Global);

            sb.AppendLine(
                "\n———\n"
                + phrases.Profile.Properties.Status
                + Language.GetUserStatusString(user.Status, lang));

            if (user.Role != Role.None)
                sb.AppendLine(
                    phrases.Profile.Properties.Role
                    + Language.GetUserRoleString(user.Role, lang) + "\n");

            using (var db = DatabaseService.GetApplicationDbContext())
            {
                var criticForm = db.CriticForms.FirstOrDefault(c => c.UserId == user.UserId);
                if (criticForm != null)
                    sb.AppendLine(
                        phrases.Profile.Properties.CategoryCritic
                        + Language.GetCategoryString(criticForm.Category));
            }

            using (var db = DatabaseService.GetRightVisionContext(rightvision))
            {
                var memberForm = db.ParticipantForms.FirstOrDefault(m => m.Status == FormStatus.Accepted && m.UserId == user.UserId);
                if (memberForm != null)
                {
                    sb.AppendLine(
                        phrases.Profile.Properties.CategoryParticipant
                        + Language.GetCategoryString(memberForm.Category));

                    sb.AppendLine(
                        phrases.Profile.Properties.Track
                        + phrases.Profile.Track.Hidden);
                }
            }

            sb.AppendLine();

            sb.AppendLine(phrases.Profile.Rewards.Header);
            if (user.Rewards.Count > 0)
                foreach (var reward in user.Rewards.Collection)
                    sb.AppendLine($"{user.Rewards.Collection.IndexOf(reward)}: [{reward.Icon}] {reward.Description}");

            else
                sb.AppendLine(phrases.Profile.Rewards.NoRewards);

            return sb.ToString();
        }

        public string Private(RvUser rvUser, string rightvision)
        {
            var lang = rvUser.Lang;
            ParticipantForm? participantForm;
            CriticForm? criticForm;
            var phrases = Language.Phrases[lang];
            StringBuilder sb;
            sb = new StringBuilder();
            sb.AppendLine(phrases.Profile.Headers.Private);

            sb.AppendLine(
                "\n———\n"
                + phrases.Profile.Properties.Status
                + Language.GetUserStatusString(rvUser.Status, lang));

            if (rvUser.Role != Role.None)
                sb.AppendLine(
                    phrases.Profile.Properties.Role
                    + Language.GetUserRoleString(rvUser.Role, lang) + "\n");

            using (var db = DatabaseService.GetApplicationDbContext())
            {
                criticForm = db.CriticForms.FirstOrDefault(c => c.UserId == rvUser.UserId);
                if (criticForm != null)
                    sb.AppendLine(
                        phrases.Profile.Properties.CategoryCritic
                        + Language.GetCategoryString(criticForm.Category));
            }

            using (var db = DatabaseService.GetRightVisionContext(rightvision))
            {
                participantForm = db.ParticipantForms.FirstOrDefault(m => m.Status == FormStatus.Accepted && m.UserId == rvUser.UserId);
                if (participantForm != null)
                {
                    sb.AppendLine(
                        phrases.Profile.Properties.CategoryParticipant
                        + Language.GetCategoryString(participantForm.Category));

                    sb.AppendLine(
                        phrases.Profile.Properties.Track
                        + phrases.Profile.Track.Hidden);
                }
            }

            sb.AppendLine(
                phrases.Profile.Properties.Sending
                + (rvUser.Has(Permissions.Permission.Sending)
                ? phrases.Profile.Sending.Active
                : phrases.Profile.Sending.Inactive));

            sb.AppendLine(phrases.Profile.Properties.CandidacyStatus);
            sb.AppendLine(phrases.Profile.Forms.Properties.Critic + GetCandidateStatus(rvUser.Lang, criticForm));
            sb.AppendLine(phrases.Profile.Forms.Properties.Participant + GetCandidateStatus(rvUser.Lang, participantForm));

            sb.AppendLine(phrases.Profile.Rewards.Header);
            if (rvUser.Rewards.Count > 0)
                foreach (var reward in rvUser.Rewards.Collection)
                    sb.AppendLine($"{rvUser.Rewards.Collection.IndexOf(reward)}: [{reward.Icon}] {reward.Description}");

            else
                sb.AppendLine(phrases.Profile.Rewards.NoRewards);

            return sb.ToString();
        }

        private string GetCandidateStatus(Enums.Lang lang, IForm? form = null)
        {
            try
            {
                if (form != null)
                    return form.Status.ToString(lang);
                else
                    return Language.Phrases[lang].Profile.Forms.Status.Allowed;
            }
            catch
            {
                return Language.Phrases[lang].Profile.Forms.Status.CouldNotGet;
            }
        }
    }
}
