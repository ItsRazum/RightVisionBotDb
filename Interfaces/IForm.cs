using RightVisionBotDb.Enums;

namespace RightVisionBotDb.Interfaces
{
    interface IForm
    {
        #region Properties

        public long UserId { get; set; }
        public string Telegram { get; set; }
        public int Rate { get; set; }
        public Category Category { get; set; }
        public long CuratorId { get; set; }

        #endregion

        #region Events

        public event EventHandler<Category>? OnFormAccepted;
        public event EventHandler? OnFormDenied;
        public event EventHandler? OnFormReset;

        #endregion

        #region Methods

        public void Accept(Category category);
        public void Deny();
        public void Reset();

        #endregion
    }
}
