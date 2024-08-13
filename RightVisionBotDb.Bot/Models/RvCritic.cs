using RightVisionBotDb.Models.Forms;

namespace RightVisionBotDb.Bot.Models
{
    public class RvCritic : CriticForm
    {
        #region Constructor

        public RvCritic(CriticForm form)
        {
            UserId = form.UserId;
            Name = form.Name;
            Telegram = form.Telegram;
            Link = form.Link;
            AboutYou = form.AboutYou;
            WhyYou = form.WhyYou;
            Category = form.Category;
            CuratorId = form.CuratorId;
        }

        #endregion
    }
}
