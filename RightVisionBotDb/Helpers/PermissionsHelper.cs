using RightVisionBotDb.Enums;
using RightVisionBotDb.Types;

namespace RightVisionBotDb.Helpers
{
    public static class PermissionsHelper
    {
        public static UserPermissions Default = new()
        {
            Permission.Messaging,            Permission.OpenProfile,
            Permission.SendCriticForm,       Permission.SendParticipantForm
        };

        public static UserPermissions User = new()
        {
            Permission.Messaging,
            Permission.OpenProfile,
        };

        public static UserPermissions Critic = new(User + new UserPermissions
        {
            Permission.CriticMenu,           Permission.CriticChat,
            Permission.ChattingInCriticChat, Permission.Evaluation
        });

        public static UserPermissions Member = new(User + new UserPermissions
        {
            Permission.TrackCard,            Permission.MemberChat,
            Permission.ChattingInMemberChat,
        });

        public static UserPermissions ExMember = new(User + new UserPermissions
        {
            Permission.MemberChat,
            Permission.ChattingInMemberChat,
        });

        public static UserPermissions CriticAndMember = new(User + Member + Critic);

        public static UserPermissions CriticAndExMember = new(User + ExMember + Critic);

        public static UserPermissions Moderator = new(User + new UserPermissions
        {
            Permission.Mute,                Permission.Unmute,
            Permission.Cancel,              Permission.News,
            Permission.ParticipantNews
        });

        public static UserPermissions SeniorModerator = new(Moderator + new UserPermissions
        {
            Permission.Ban,                 Permission.Unban,
            Permission.BlacklistOn,         Permission.BlacklistOff,
            Permission.EditPermissions,     Permission.Block
        });

        public static UserPermissions Curator = new()
        {
            Permission.PreListening,        Permission.Curate,
            Permission.Rewarding
        };

        public static UserPermissions Empty = new();

        public static UserPermissions Developer = new()
        {
            Permission.Audit
        };

        public static UserPermissions Admin = new(User + CriticAndMember + SeniorModerator + Curator + Developer + new UserPermissions()
        {
            Permission.Degrade,             Permission.DegradePermission,
            Permission.GivePermission,      Permission.Grant,
            Permission.TechNews
        });

        public static readonly Dictionary<Enum, UserPermissions> Layouts = new()
        {
            { Status.User, User },
            { Status.Critic, Critic },
            { Status.Member, Member },
            { Status.ExMember, ExMember },
            { Status.CriticAndMember, CriticAndMember },
            { Status.CriticAndExMember, CriticAndExMember },

            { Role.Admin, Admin },
            { Role.Curator, Curator },
            { Role.Developer, Developer },
            { Role.Moderator, Moderator },
            { Role.SeniorModerator, SeniorModerator },

            { Role.TechAdmin, Empty },
            { Role.Designer, Empty },
            { Role.Translator, Empty },
            { Role.None, Empty }
        };
    }
}
