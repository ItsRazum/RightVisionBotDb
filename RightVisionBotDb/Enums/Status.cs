namespace RightVisionBotDb.Enums;

[Flags]
public enum Status
{
    User,
    Participant,
    ExParticipant,
    Critic,
    CriticAndParticipant,
    CriticAndExParticipant
}
