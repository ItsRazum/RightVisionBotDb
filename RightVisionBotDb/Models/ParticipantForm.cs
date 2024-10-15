using RightVisionBotDb.Enums;
using RightVisionBotDb.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RightVisionBotDb.Models
{
    public class ParticipantForm : IForm
    {

        #region Properties

        public string Name { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string Track { get; set; } = string.Empty;

        #region Legacy

        /// <summary>
        /// Legacy, размечать в форме не рекомендуется
        /// </summary>
        public string? Country { get; set; }
        /// <summary>
        /// Legacy, размечать в форме не рекомендуется
        /// </summary>
        public string? City { get; set; }

        #endregion

        #region IForm Properties

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserId { get; set; } = default;
        public string Telegram { get; set; } = string.Empty;
        public int Rate { get; set; } = default;
        public Category Category { get; set; } = Category.None;
        public long CuratorId { get; set; } = default;
        public FormStatus Status { get; set; } = FormStatus.NotFinished;

        #endregion

        #endregion

        #region IForm Events

        public event EventHandler<Category>? FormAccepted;
        public event EventHandler? FormDenied;
        public event EventHandler? FormReset;

        #endregion

        #region IForm Methods

        public void Accept(Category category)
        {

            FormAccepted?.Invoke(this, category);
        }

        public void Deny()
        {

            FormDenied?.Invoke(this, new EventArgs());
        }

        public void Reset()
        {

            FormReset?.Invoke(this, new EventArgs());
        }

        #endregion

    }
}
