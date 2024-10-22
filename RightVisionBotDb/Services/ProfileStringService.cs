using RightVisionBotDb.Enums;
using RightVisionBotDb.Extensions.Enums;
using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Lang;
using RightVisionBotDb.Models;
using RightVisionBotDb.Permissions;
using RightVisionBotDb.Types;
using System.Text;
using System.Linq;

namespace RightVisionBotDb.Services
{
    public class ProfileStringService
    {
        private Bot Bot { get; set; }
        private DatabaseService DatabaseService { get; set; }
        private Keyboards Keyboards;

        public ProfileStringService(
            Bot bot,
            DatabaseService databaseService,
            Keyboards keyboards)
        {
            Bot = bot;
            DatabaseService = databaseService;
            Keyboards = keyboards;
        }

        #region Methods

        #region Public methods

        public string Public(RvUser targetRvUser, RvUser rvUser, string rightvision)
        {
            var lang = rvUser.Lang;
            var phrases = Language.Phrases[lang];
            StringBuilder sb;
            sb = new StringBuilder();

            string header = targetRvUser == rvUser
                ? phrases.Profile.Headers.Private
                : string.Format(phrases.Profile.Headers.Global, targetRvUser.Name);

            sb.AppendLine(header);

            sb.AppendLine(
                "———\n"
                + phrases.Profile.Properties.Status
                + Language.GetUserStatusString(targetRvUser.Status, lang));

            if (targetRvUser.Role != Role.None)
                sb.AppendLine(
                    phrases.Profile.Properties.Role
                    + Language.GetUserRoleString(targetRvUser.Role, lang) + "\n");

            using (var db = DatabaseService.GetApplicationDbContext())
            {
                var criticForm = db.CriticForms.FirstOrDefault(c => c.UserId == targetRvUser.UserId && c.Status == FormStatus.Accepted);
                if (criticForm != null)
                    sb.AppendLine(
                        phrases.Profile.Properties.CategoryCritic
                        + Language.GetCategoryString(criticForm.Category));
            }

            using (var db = DatabaseService.GetRightVisionContext(rightvision))
            {
                var memberForm = db.ParticipantForms.FirstOrDefault(m => m.Status == FormStatus.Accepted && m.UserId == targetRvUser.UserId);
                if (memberForm != null)
                {
                    sb.AppendLine(
                        phrases.Profile.Properties.CategoryParticipant
                        + Language.GetCategoryString(memberForm.Category));

                    if (memberForm.Country != null)
                        sb.AppendLine(
                            phrases.Profile.Properties.Country
                            + memberForm.Country);

                    if (memberForm.City != null)
                        sb.AppendLine(
                            phrases.Profile.Properties.City
                            + memberForm.City);

                    sb.AppendLine(
                        phrases.Profile.Properties.Track +
                        (db.Status == RightVisionStatus.Relevant  
                        ? phrases.Profile.Track.Hidden
                        : memberForm.Track
                        )
                    );
                }
            }

            sb.AppendLine();

            sb.AppendLine(phrases.Profile.Rewards.Header);
            if (targetRvUser.Rewards.Count > 0)
                foreach (var reward in targetRvUser.Rewards.Collection)
                    sb.AppendLine($"{targetRvUser.Rewards.Collection.IndexOf(reward)}: [{reward.Icon}] {reward.Description}");

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
            sb.AppendLine(string.Format(phrases.Profile.Headers.Private, rvUser.Name));

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
                + (rvUser.Has(Permission.Sending)
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

        public string Permissions(CallbackContext c, RvUser targetRvUser, bool minimize, Enums.Lang lang)
        {
            var rvUser = c.RvUser;

            StringBuilder sb = new(
                c.RvUser == targetRvUser 
                ? Language.Phrases[lang].Profile.Permissions.Header 
                : string.Format(Language.Phrases[lang].Profile.Permissions.HeaderGlobal, targetRvUser.Name));

            IEnumerable<Permission> permissions;
            UserPermissions layout = new(RightVisionBotDb.Permissions.Permissions.Layouts[rvUser.Status] + RightVisionBotDb.Permissions.Permissions.Layouts[rvUser.Role]);

            sb.AppendLine();
            if (minimize)
            {
                var standartCount = 10;
                permissions = rvUser.Permissions.Take(standartCount);

                foreach (var permission in permissions)
                    sb.AppendLine("• " + permission);

                if (rvUser.Permissions.Count >= standartCount)
                    sb.AppendLine("...");
            }
            else
            {
                permissions = rvUser.Permissions;

                foreach (var permission in permissions)
                    sb.AppendLine("• " + permission);
            }

            var addedList = AddedPermissionsList(layout, rvUser.Permissions);
            if (addedList.Count > 0)
            {
                sb.AppendLine(Language.Phrases[lang].Profile.Permissions.BlockedList);
                foreach (var permission in addedList)
                    sb.AppendLine("+ " + permission);
            }

            var blockedList = BlockedPermissionsList(layout, rvUser.Permissions);
            if (blockedList.Count > 0)
            {
                sb.AppendLine(Language.Phrases[lang].Profile.Permissions.BlockedList);
                foreach (var permission in blockedList)
                    sb.AppendLine("- " + permission);
            }

            return sb.ToString();
        }

        #endregion

        #region Private methods

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

        private List<Permission> AddedPermissionsList(UserPermissions layout, UserPermissions userPermissions)
        {
            var addedList = new List<Permission>();
            foreach (var permission in userPermissions.Where(permission => !layout.Contains(permission)))
                addedList.Add(permission);

            return addedList;
        }

        private List<Permission> BlockedPermissionsList(UserPermissions layout, UserPermissions userPermissions)
        {
            var blockedList = new List<Permission>();
            foreach (var permission in layout.Collection.Where(permission => !userPermissions.Contains(permission)))
                blockedList.Add(permission);

            return blockedList;
        }

        #endregion

        #endregion
    }
}
