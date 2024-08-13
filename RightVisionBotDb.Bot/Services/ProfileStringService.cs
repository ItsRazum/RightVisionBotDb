using DryIoc;
using RightVisionBotDb.Bot.Extensions.Enums;
using RightVisionBotDb.Bot.Lang;
using RightVisionBotDb.Bot.Models;
using RightVisionBotDb.Core;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Models;
using RightVisionBotDb.Models.Forms;
using System.Text;

namespace RightVisionBotDb.Bot.Services
{
    public class ProfileStringService
    {
        public string Public(RvUser user, Enums.Lang lang, Bot bot)
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

            var criticForm = App.Container.Resolve<Bot>().Core.GetAcceptedCriticForm(user.UserId);
            if (criticForm != null)
                sb.AppendLine(
                    phrases.Profile.Properties.CategoryCritic
                    + Language.GetCategoryString(criticForm.Category));

            var memberForm = App.Container.Resolve<Bot>().Core.GetAcceptedParticipantForm(user.UserId, "RightVision24");
            if (memberForm != null)
            {
                sb.AppendLine(
                    phrases.Profile.Properties.CategoryParticipant
                    + Language.GetCategoryString(memberForm.Category));

                sb.AppendLine(
                    phrases.Profile.Properties.Track
                    + phrases.Profile.Track.Hidden);
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

        public string Private(RvUser user, Enums.Lang lang, Bot bot)
        {
            var phrases = Language.Phrases[lang];
            StringBuilder sb;
            sb = new StringBuilder();
            sb.AppendLine(phrases.Profile.Headers.Private);

            sb.AppendLine(
                "\n———\n"
                + phrases.Profile.Properties.Status
                + Language.GetUserStatusString(user.Status, lang));

            if (user.Role != Role.None)
                sb.AppendLine(
                    phrases.Profile.Properties.Role
                    + Language.GetUserRoleString(user.Role, lang) + "\n");

            var criticForm = App.Container.Resolve<Bot>().Core.GetAcceptedCriticForm(user.UserId);
            if (criticForm != null)
                sb.AppendLine(
                    phrases.Profile.Properties.CategoryCritic
                    + Language.GetCategoryString(criticForm.Category));

            var participantForm = App.Container.Resolve<Bot>().Core.GetAcceptedParticipantForm(user.UserId, "RightVision24");
            if (participantForm != null)
            {
                sb.AppendLine(
                    phrases.Profile.Properties.CategoryParticipant
                    + Language.GetCategoryString(participantForm.Category));

                sb.AppendLine(
                    phrases.Profile.Properties.Track
                    + participantForm.Track);
            }
            sb.AppendLine(
                phrases.Profile.Properties.Sending
                + (user.Has(Permissions.Permission.Sending)
                ? phrases.Profile.Sending.Active
                : phrases.Profile.Sending.Inactive));

            sb.AppendLine(phrases.Profile.Properties.CandidacyStatus);
            sb.AppendLine(phrases.Profile.Forms.Properties.Critic + GetCandidateStatus(user.Lang, bot.Core.GetCriticForm(user.UserId)));

            sb.AppendLine(phrases.Profile.Rewards.Header);
            if (user.Rewards.Count > 0)
                foreach (var reward in user.Rewards.Collection)
                    sb.AppendLine($"{user.Rewards.Collection.IndexOf(reward)}: [{reward.Icon}] {reward.Description}");

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
