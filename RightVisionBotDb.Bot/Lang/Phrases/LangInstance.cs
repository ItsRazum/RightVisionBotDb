namespace RightVisionBotDb.Bot.Lang.Phrases
{
    public sealed class LangInstance
    {
        public KeyboardButtons KeyboardButtons { get; set; }
        public Profile Profile { get; set; }
        public Messages Messages { get; set; }
    }

    public class KeyboardButtons
    {
        public string MainMenu { get; set; }
        public string MyProfile { get; set; }
        public string EditTrack { get; set; }
        public string SendTrack { get; set; }
        public string PermissionsList { get; set; }
        public string PunishmentsHistory { get; set; }
        public string Back { get; set; }
        public string About { get; set; }
        public string Apply { get; set; }
        public string Sending_Subscribe { get; set; }
        public string Sending_Unsubscribe { get; set; }
        public string CriticFormVariationOne { get; set; }
        public string CriticFormVariationTwo { get; set; }
        public string MemberFormVariationOne { get; set; }
        public required string MemberFormVariationTwo { get; set; }
        public required CriticMenu CriticMenu { get; set; }
    }

    public class CriticMenu
    {
        public string Open { get; set; }
        public string PreListening { get; set; }
        public string Voting { get; set; }
    }

    public class Profile
    {
        public ProfileHeaders Headers { get; set; }
        public ProfileRoles Roles { get; set; }
        public ProfilePermissions Permissions { get; set; }
        public ProfileStatusHeaders StatusHeaders { get; set; }
        public ProfileProperties Properties { get; set; }
        public ProfileLayouts Layouts { get; set; }
        public ProfileSending Sending { get; set; }
        public ProfileForms Forms { get; set; }
        public ProfileRewards Rewards { get; set; }
        public ProfileTrack Track { get; set; }
        public ProfilePunishments Punishments { get; set; }
    }

    public class ProfileHeaders
    {
        public string Global { get; set; }
        public string Private { get; set; }
    }

    public class ProfileRoles
    {
        public string Designer { get; set; }
        public string Translator { get; set; }
        public string Moderator { get; set; }
        public string SeniorModerator { get; set; }
        public string TechAdmin { get; set; }
        public string Developer { get; set; }
        public string Curator { get; set; }
        public string Admin { get; set; }
    }

    public class ProfileProperties
    {
        public string Status { get; set; }
        public string Role { get; set; }
        public string CategoryGeneral { get; set; }
        public string CategoryParticipant { get; set; }
        public string Sending { get; set; }
        public string CandidacyStatus { get; set; }
        public string CategoryCritic { get; set; }
        public string Track { get; set; }
    }

    public class ProfilePermissions
    {
        public string Header { get; set; }
        public string Header_Global { get; set; }
        public string FullList { get; set; }
        public string AddedList { get; set; }
        public string BlockedList { get; set; }
    }

    public class ProfileStatusHeaders
    {
        public string User { get; set; }
        public string Member { get; set; }
        public string ExMember { get; set; }
        public string Critic { get; set; }
        public string CriticAndMember { get; set; }
        public string CriticAndExMember { get; set; }
    }

    public class ProfileLayouts
    {
        public string Member { get; set; }
        public string Critic { get; set; }
        public string CriticAndMember { get; set; }
    }

    public class ProfileSending
    {
        public string Header { get; set; }
        public string Active { get; set; }
        public string Inactive { get; set; }
    }

    public class ProfileForms
    {
        public FormsProperties Properties { get; set; }
        public ProfileFormStatus Status { get; set; }
    }

    public class ProfileFormStatus
    {
        public string Allowed { get; set; }
        public string NotFinished { get; set; }
        public string Waiting { get; set; }
        public string UnderConsideration { get; set; }
        public string Accepted { get; set; }
        public string Denied { get; set; }
        public string Blocked { get; set; }
        public string CouldNotGet { get; set; }
    }

    public class ProfileRewards
    {
        public string Header { get; set; }
        public string NoRewards { get; set; }
    }

    public class ProfileTrack
    {
        public string Hidden { get; set; }
    }

    public class ProfilePunishments
    {
        public string Contacts { get; set; }
        public string MuteNotification { get; set; }
        public string BanNotification { get; set; }
        public string UnbanNotification { get; set; }
        public string BlacklistNotification { get; set; }
        public string BlacklistOffNotificatiom { get; set; }
        public string HareKickNotification { get; set; }
        public ProfilePunishment Punishment { get; set; }
    }

    public class ProfilePunishment
    {
        public string InMembers { get; set; }
        public string InCritics { get; set; }
        public string Reason { get; set; }
        public string DateTo { get; set; }
        public string NoPunishments { get; set; }
    }

    public class Messages
    {
        public MessagesCommon Common { get; set; }
        public MessagesCritic Critic { get; set; }
        public MessagesParticipant Participant { get; set; }
    }

    public class MessagesCommon
    {
        public string Greetings { get; set; }
        public string About { get; set; }
        public string LanguageSelected { get; set; }
        public string ChooseRole { get; set; }
        public string DontEnterZero { get; set; }
        public string NoPermission { get; set; }
        public string FormsBlocked { get; set; }
        public string SendFormRightNow { get; set; }
        public string EnrollmentClosed { get; set; }
    }

    public class MessagesCritic
    {
        public string EnterName { get; set; }
        public string EnterLink { get; set; }
        public string IncorrectFormat { get; set; }
        public string EnterRate { get; set; }
        public string EnterAboutYou { get; set; }
        public string EnterWhyYou { get; set; }
        public string EnterPreview { get; set; }
        public string FormSubmitted { get; set; }
        public string FormAccepted { get; set; }
        public string FormDenied { get; set; }
        public string FormCanceled { get; set; }
        public string FormBlocked { get; set; }
    }

    public class MessagesParticipant
    {
        public string EnterName { get; set; }
        public string EnterLink { get; set; }
        public string EnterRate { get; set; }
        public string EnterTrack { get; set; }
        public string FormSubmitted { get; set; }
        public string FormAccepted { get; set; }
        public string FormDenied { get; set; }
        public string FormCanceled { get; set; }
        public string FormBlocked { get; set; }
        public string PreListeningBlocked { get; set; }
        public string PreListeningCategoryChanged { get; set; }
        public string EnterNewTrack { get; set; }
        public string TrackUpdated { get; set; }
        public ParticipantTrackCard TrackCard { get; set; }
    }

    public class ParticipantTrackCard
    {
        public TrackCardProperties Properties { get; set; }
        public string CreatingCard { get; set; }
        public string CreatingCardSuccess { get; set; }
        public string HereItIs { get; set; }
        public string CardFull { get; set; }
        public string SendTrack { get; set; }
        public string TrackNotSent { get; set; }
        public string TrackHereItIs { get; set; }
        public string SendTrackInstruction { get; set; }
        public string SendTrackSuccess { get; set; }
        public string SendImage { get; set; }
        public string ImageNotSent { get; set; }
        public string ImageHereItIs { get; set; }
        public string SendImageInstruction { get; set; }
        public string SendImageSuccess { get; set; }
        public string SendText { get; set; }
        public string TextNotSent { get; set; }
        public string TextHereItIs { get; set; }
        public string SendTextInstruction { get; set; }
        public string SendTextSuccess { get; set; }

        public string CheckTrack { get; set; }
        public string CheckImage { get; set; }
        public string CheckText { get; set; }

        public string TrackReceived { get; set; }
        public string TrackNotReceived { get; set; }

        public string ImageReceived { get; set; }
        public string ImageNotReceived { get; set; }
    }

    public class TrackCardProperties
    {
        public string AudioFile { get; set; }
        public string ImageFile { get; set; }
        public string TextFile { get; set; }
    }

    public class FormsProperties
    {
        public string Participant { get; set; }
        public string Critic { get; set; }
    }
}
