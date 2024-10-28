using RightVisionBotDb.Enums;

namespace RightVisionBotDb.Interfaces
{
    public interface IForm
    {

        #region Properties

        public string Name { get; set; }
        public long UserId { get; set; }
        public string Telegram { get; set; }
        public int Rate { get; set; }
        public Category Category { get; set; }
        public long CuratorId { get; set; }
        public FormStatus Status { get; set; }
        public string CallbackType { get; }

        #endregion

    }
}
