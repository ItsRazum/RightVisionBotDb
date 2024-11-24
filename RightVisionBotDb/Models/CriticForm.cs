using EasyForms.Attributes;
using EasyForms.Types;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RightVisionBotDb.Models
{
    public class CriticForm : Form, IForm
    {

        #region Properties

        [FormField(1)]
        public string Name { get; set; } = string.Empty;
        [FormField(2)]
        public string Link { get; set; } = string.Empty;
        [FormField(4)]
        public string AboutYou { get; set; } = string.Empty;
        [FormField(5)]
        public string WhyYou { get; set; } = string.Empty;

        #region IForm Properties

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserId { get; set; } = 0;
        public string Telegram { get; set; } = string.Empty;
        [FormField(3)]
        public int Rate { get; set; } = 0;
        public Category Category { get; set; } = Category.None;
        public long CuratorId { get; set; } = 0;
        public FormStatus Status { get; set; } = FormStatus.NotFinished;
        public string CallbackType { get; } = "c_";

        #endregion

        #endregion

        #region Constructor

        public CriticForm()
        {
        }

        public CriticForm(long userId, string? telegram)
        {
            UserId = userId;
            Telegram = telegram ?? string.Empty;
        }

        #endregion

    }
}