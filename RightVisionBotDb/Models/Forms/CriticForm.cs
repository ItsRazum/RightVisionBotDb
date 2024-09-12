using RightVisionBotDb.Enums;
using RightVisionBotDb.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace RightVisionBotDb.Models.Forms
{
    public class CriticForm : IForm, INotifyPropertyChanged
    {

        #region Properties

        public string Name { get; set; }
        public string Link { get; set; }
        public string AboutYou { get; set; }
        public string WhyYou { get; set; }
        #region IForm Properties

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserId { get; set; }
        public string Telegram { get; set; }
        public int Rate { get; set; }
        public Category Category { get; set; }
        public long CuratorId { get; set; }
        public FormStatus Status { get; set; }

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

        #region INotifyPropertyChanged Implementation

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

    }
}
