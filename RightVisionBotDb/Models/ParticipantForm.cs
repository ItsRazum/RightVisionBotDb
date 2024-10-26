using EasyForms.Attributes;
using EasyForms.Types;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RightVisionBotDb.Models
{
    public class ParticipantForm : Form, IForm
    {

        #region Properties

        [FormField(1)]
        public string Name { get; set; } = string.Empty;
        [FormField(2)]
        public string Link { get; set; } = string.Empty;
        [FormField(4)]
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
        [FormField(3)]
        public int Rate { get; set; } = default;
        public Category Category { get; set; } = Category.None;
        public long CuratorId { get; set; } = default;
        public FormStatus Status { get; set; } = FormStatus.NotFinished;

        #endregion

        #endregion

        #region Constructors

        public ParticipantForm()
        {
        }

        public ParticipantForm(long userId, string? teleram)
        {
            UserId = userId;
            Telegram = teleram ?? string.Empty;
        }

        #endregion

    }
}
