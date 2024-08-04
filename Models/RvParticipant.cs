using RightVisionBotDb.Types.Forms;

namespace RightVisionBotDb.Models
{
    public class RvParticipant : ParticipantForm
    {
        #region Constructor

        public RvParticipant(ParticipantForm form)
        {
            UserId = form.UserId;
            Name = form.Name;
            Telegram = form.Telegram;
            Link = form.Link;
            Rate = form.Rate;
            Category = form.Category;
            CuratorId = form.CuratorId;
        }

        #endregion
    }
}
