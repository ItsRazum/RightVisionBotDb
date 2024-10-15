namespace RightVisionBotDb.Permissions;

[Flags]
public enum Permission
{
    /// <summary>
    /// Право на общение с ботом
    /// </summary>
    Messaging,
    /// <summary>
    /// Право на рассылку
    /// </summary>
    Sending,
    /// <summary>
    /// Право на отправку общих новостей
    /// </summary>
    News,
    /// <summary>
    /// Право на отправку новостей для участников
    /// </summary>
    MemberNews,
    /// <summary>
    /// Право на отправку технических новостей
    /// </summary>
    TechNews,
    /// <summary>
    /// Право на открытие профиля
    /// </summary>
    OpenProfile,
    /// <summary>
    /// Право на отправку заявки на судейство
    /// </summary>
    SendCriticForm,
    /// <summary>
    /// Право на отправку заявки на участие
    /// </summary>
    SendParticipantForm,
    /// <summary>
    /// Право на доступ к чату участников
    /// </summary>
    MemberChat,
    /// <summary>
    /// Право на отправку сообщений в чате участников
    /// </summary>
    ChattingInMemberChat,
    /// <summary>
    /// Право на нахождение в чате судей
    /// </summary>
    CriticChat,
    /// <summary>
    /// Право на отправку сообщений в чате судей
    /// </summary>
    ChattingInCriticChat,
    /// <summary>
    /// Право на открытие судейского меню</
    /// summary>
    CriticMenu,
    /// <summary>
    /// Право на открытие карточки ремикса
    /// </summary>
    TrackCard,
    /// <summary>
    /// Право на оценивание ремиксов
    /// </summary>
    Evaluation,
    /// <summary>
    /// Право на предварительное прослушивание
    /// </summary>
    PreListening,
    /// <summary>
    /// Право курировать кандидатов
    /// </summary>
    Curate,
    /// <summary>
    /// Право банить
    /// </summary>
    Ban,
    /// <summary>
    /// Право мутить
    /// </summary>
    Mute,
    /// <summary>
    /// Право изменять права других пользователей
    /// </summary>
    EditPermissions,
    /// <summary>
    /// Право на отправку пользователя в чёрный список
    /// </summary>
    BlacklistOn,
    /// <summary>
    /// Право на блокировку любой из кандидатур пользователя
    /// </summary>
    Block,
    /// <summary>
    /// Право на аннулирование любой из кандидатур пользователя
    /// </summary>
    Cancel,
    /// <summary>
    /// Право на чтение журнала аудита
    /// </summary>
    Audit,
    /// <summary>
    /// Право на выдачу наград
    /// </summary>
    Rewarding,
    /// <summary>
    /// Право на назначение пользователя на должность
    /// </summary>
    Grant,
    /// <summary>
    /// Право на выдачу привилегии
    /// </summary>
    GivePermission,
    /// <summary>
    /// Право на снятие пользователя с должности
    /// </summary>
    Degrade,
    /// <summary>
    /// Право на снятие с пользователя права
    /// </summary>
    DegradePermission,
    /// <summary>
    /// Право на разбан пользователя
    /// </summary>
    Unban,
    /// <summary>
    /// Право на размут пользователя
    /// </summary>
    Unmute,
    /// <summary>
    /// Право на удаление пользователя из чёрного списка
    /// </summary>
    BlacklistOff
}