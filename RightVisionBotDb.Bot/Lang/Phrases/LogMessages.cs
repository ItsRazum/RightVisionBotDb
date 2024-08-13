using RightVisionBotDb.Models;

namespace RightVisionBotDb.Bot.Lang.Phrases
{
    public class LogMessages
    {
        #region Methods

        public string Registration(RvUser rvUser) => $"Зарегистрирован новый пользователь @{rvUser.Telegram} с языком {rvUser.Lang}";
        public string UserOpenedMenu(RvUser rvUser) => $"Пользователь @{rvUser.Telegram} открыл главное меню на языке {rvUser.Lang}";
        public string UserSubscribedToTheNews(RvUser rvUser) => $"Пользователь @{rvUser.Telegram} подписался на новостную рассылку";
        public string UserUnsubscribedFromTheNews(RvUser rvUser) => $"Пользователь @{rvUser.Telegram} отписался от новостной рассылки";
        public string UserOpenedCriticMenu(RvUser rvUser) => $"Пользователь @{rvUser.Telegram} открыл судейское меню";
        public string UserOpenedPreListening(RvUser rvUser) => $"Пользователь @{rvUser.Telegram} начал предварительное прослушивание";
        public string UserClosedPreListening(RvUser rvUser) => $"Пользователь @{rvUser.Telegram} закрыл предварительное прослушивание";
        public string UserCanceledParticipantForm(RvUser rvUser) => $"Пользователь @{rvUser.Telegram} отменил заполнение заявки на участие";
        public string UserCanceledCriticForm(RvUser rvUser) => $"Пользователь @{rvUser.Telegram} отменил заполнение заявки на судейство";
        public string UserTookCuratorshipCritic(RvUser rvUser, long curatorId) => $"Пользователь @{rvUser.Telegram} взял кураторство над судьёй Id:{curatorId}";
        public string UserTookCuratorshipParticipant(RvUser rvUser, long participantId) => $"Пользователь @{rvUser.Telegram} взял кураторство над участником Id:{participantId}";


        #endregion

    }
}
