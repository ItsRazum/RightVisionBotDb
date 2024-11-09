using RightVisionBotDb.Enums;
using RightVisionBotDb.Types;

namespace RightVisionBotDb.Helpers
{
    public static class PermissionsHelper
    {
        public static readonly UserPermissions Default =
        [
            Permission.Messaging,            Permission.OpenProfile,
            Permission.SendCriticForm,       Permission.SendParticipantForm
        ];

        public static readonly UserPermissions User =
        [
            Permission.Messaging,
            Permission.OpenProfile,
        ];

        public static readonly UserPermissions Critic = new(User + new UserPermissions
        {
            Permission.CriticMenu,
            Permission.CriticChat,
            Permission.Evaluation
        });

        public static readonly UserPermissions Participant = new(User + new UserPermissions
        {
            Permission.TrackCard,
            Permission.ParticipantChat
        });

        public static readonly UserPermissions ExParticipant = new(User + new UserPermissions
        {
            Permission.ParticipantChat
        });

        public static readonly UserPermissions CriticAndParticipant = new(User + Participant + Critic);

        public static readonly UserPermissions CriticAndExParticipant = new(User + ExParticipant + Critic);

        public static readonly UserPermissions Moderator = new(User + new UserPermissions
        {
            Permission.Mute,                Permission.Unmute,
            Permission.Cancel,              Permission.News,
            Permission.ParticipantNews
        });

        public static readonly UserPermissions SeniorModerator = new(Moderator + new UserPermissions
        {
            Permission.Ban,                 Permission.Unban,
            Permission.BlacklistOn,         Permission.BlacklistOff,
            Permission.EditPermissions,     Permission.Block
        });

        public static readonly UserPermissions Curator =
        [
            Permission.PreListening,
            Permission.Curate,
            Permission.Rewarding
        ];

        public static readonly UserPermissions Empty = [];

        public static readonly UserPermissions Developer =
        [
            Permission.Audit
        ];

        public static readonly UserPermissions Admin = new(User + CriticAndParticipant + SeniorModerator + Curator + Developer + new UserPermissions()
        {
            Permission.Degrade,             Permission.DegradePermission,
            Permission.GivePermission,      Permission.Grant,
            Permission.TechNews
        });

        public static readonly Dictionary<Enum, UserPermissions> Layouts = new()
        {
            { Status.User, User },
            { Status.Critic, Critic },
            { Status.Participant, Participant },
            { Status.ExParticipant, ExParticipant },
            { Status.CriticAndParticipant, CriticAndParticipant },
            { Status.CriticAndExParticipant, CriticAndExParticipant },

            { Role.Admin, Admin },
            { Role.Curator, Curator },
            { Role.Developer, Developer },
            { Role.Moderator, Moderator },
            { Role.SeniorModerator, SeniorModerator },

            { Role.Designer, Empty },
            { Role.Translator, Empty },
            { Role.None, Empty }
        };
    }
}
