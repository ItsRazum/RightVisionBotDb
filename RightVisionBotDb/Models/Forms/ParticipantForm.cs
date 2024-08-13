using RightVisionBotDb.Enums;
using RightVisionBotDb.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RightVisionBotDb.Models.Forms
{
    public class ParticipantForm : IForm
    {
        #region Properties

        public string Name { get; set; }
        public string Link { get; set; } = "0";
        public string Track { get; set; }

        #region IForm Properties

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserId { get; set; }
        public string Telegram { get; set; }
        public int Rate { get; set; } = 0;
        public Category Category { get; set; } = Category.None;
        public long CuratorId { get; set; } = 0;
        public FormStatus Status { get; set; } = FormStatus.NotFinished;

        #endregion

        #endregion

        #region IForm Events

        public event EventHandler<Category>? OnFormAccepted;
        public event EventHandler? OnFormDenied;
        public event EventHandler? OnFormReset;

        #endregion

        #region IForm Methods

        public void Accept(Category category)
        {

            OnFormAccepted?.Invoke(this, category);
        }

        public void Deny()
        {

            OnFormDenied?.Invoke(this, new EventArgs());
        }

        public void Reset()
        {

            OnFormReset?.Invoke(this, new EventArgs());
        }

        #endregion
    }
}
