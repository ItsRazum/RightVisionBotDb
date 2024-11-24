using RightVisionBotDb.Locations;
using RightVisionBotDb.Models;

namespace RightVisionBotDb.Helpers
{
    public static class LogMessagesHelper
    {
        #region Methods

        public static string Registration(RvUser rvUser) => $"Зарегистрирован новый пользователь @{rvUser.Telegram} с языком {rvUser.Lang}";
        public static string UserOpenedMenu(RvUser rvUser) => $"Пользователь @{rvUser.Telegram} открыл главное меню на языке {rvUser.Lang}";
        public static string UserSubscribedToTheNews(RvUser rvUser) => $"Пользователь @{rvUser.Telegram} подписался на новостную рассылку";
        public static string UserUnsubscribedFromTheNews(RvUser rvUser) => $"Пользователь @{rvUser.Telegram} отписался от новостной рассылки";
        public static string UserOpenedCriticMenu(RvUser rvUser) => $"Пользователь @{rvUser.Telegram} открыл судейское меню";
        public static string UserOpenedPreListening(RvUser rvUser) => $"Пользователь @{rvUser.Telegram} начал предварительное прослушивание";
        public static string UserClosedPreListening(RvUser rvUser) => $"Пользователь @{rvUser.Telegram} закрыл предварительное прослушивание";
        public static string UserCanceledParticipantForm(RvUser rvUser) => $"Пользователь @{rvUser.Telegram} отменил заполнение заявки на участие";
        public static string UserCanceledCriticForm(RvUser rvUser) => $"Пользователь @{rvUser.Telegram} отменил заполнение заявки на судейство";
        public static string UserTookCuratorshipCritic(RvUser rvUser, long curatorId) => $"Пользователь @{rvUser.Telegram} взял кураторство над судьёй Id:{curatorId}";
        public static string UserTookCuratorshipParticipant(RvUser rvUser, long participantId) => $"Пользователь @{rvUser.Telegram} взял кураторство над участником Id:{participantId}";
        public static string UserChangedLocation(RvUser rvUser, (RvLocation, RvLocation) locations) => $"@{rvUser.Telegram}: Смена локации с {locations.Item1} на {locations.Item2}";
        public static string UserStartedNewsSending(RvUser rvUser, string args) => $"Пользователь @{rvUser.Telegram} начал новостную рассылку\n{args}";

        #endregion
    }
}
