using EasyForms.Attributes;
using EasyForms.Types;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RightVisionBotDb.Models
{
    public class StudentForm : Form, IForm
    {

        #region Properties

        [FormField(1)]
        public string Name { get; set; } = string.Empty;
        [FormField(2)]
        public string Link { get; set; } = string.Empty;
        public string? StudentClass { get; set; } = null;

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
        public string CallbackType { get; } = "st_";

        #endregion

        #endregion

        #region Constructor

        public StudentForm() 
        {
        }

        public StudentForm(long userId, string? telegram)
        {
            UserId = userId;
            Telegram = telegram ?? "x";
        }

        #endregion

    }
}
