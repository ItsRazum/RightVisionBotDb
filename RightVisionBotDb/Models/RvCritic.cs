using RightVisionBotDb.Models.Forms;

namespace RightVisionBotDb.Models
{
    public class RvCritic : CriticForm
    {
        #region Constructor

        public RvCritic()
        {
        }
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
