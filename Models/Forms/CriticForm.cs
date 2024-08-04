using RightVisionBotDb.Enums;
using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Models;

namespace RightVisionBotDb.Types.Forms
{
    public class CriticForm : IForm
    {
        #region Properties

        public string Name { get; set; } = "0";
        public string Link { get; set; } = "0";
        public string AboutYou { get; set; } = "0";
        public string WhyYou { get; set; } = "0";

        #region IForm Properties

        public long UserId { get; set; }
        public string Telegram { get; set; }
        public int Rate { get; set; } = 0;
        public Category Category { get; set; } = Category.None;
        public long CuratorId { get; set; } = 0;

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
