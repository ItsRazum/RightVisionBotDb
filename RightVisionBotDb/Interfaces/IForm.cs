using RightVisionBotDb.Enums;

namespace RightVisionBotDb.Interfaces
{
    public interface IForm
    {

        #region Properties

        public long UserId { get; set; }
        public string Telegram { get; set; }
        public int Rate { get; set; }
        public Category Category { get; set; }
        public long CuratorId { get; set; }
        public FormStatus Status { get; set; }

        #endregion

        #region Events

        public event EventHandler<Category>? FormAccepted;
        public event EventHandler? FormDenied;
        public event EventHandler? FormReset;

        #endregion

        #region Methods

        public void Accept(Category category);
        public void Deny();
        public void Reset();

        #endregion

    }
}
