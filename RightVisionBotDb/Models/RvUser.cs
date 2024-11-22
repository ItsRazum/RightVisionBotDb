using RightVisionBotDb.Enums;
using RightVisionBotDb.Helpers;
using RightVisionBotDb.Locations;
using RightVisionBotDb.Types;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RightVisionBotDb.Models
{
    public class RvUser
    {

        #region Fields

        private RvLocation location;
        private string telegram = string.Empty;

        #endregion

        #region Properties

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserId { get; set; }
        public string Name { get; set; }
        public string Telegram 
        {
            get => string.IsNullOrEmpty(telegram) ? "x" : telegram;
            set => telegram = value; 
        }
        public Enums.Lang Lang { get; set; }
        public Status Status { get; set; } = Status.User;
        public Role Role { get; set; } = Role.None;
        public RvLocation Location
        {
            get => location;
            set
            {
                if (location == value) return;
                LocationChanged?.Invoke(this, (Location, value));
                location = value;
            }
        }
        public UserPermissions UserPermissions { get; set; }
        public List<RvPunishment> Punishments { get; set; } = [];
        public List<Reward> Rewards { get; set; } = [];

        #region Cooldown Properties

        [NotMapped] public System.Timers.Timer? Cooldown { get; set; }
        [NotMapped] private int Counter { get; set; }
        [NotMapped] private System.Timers.Timer? CounterCooldown { get; set; }
        [NotMapped]
        private int TimerInterval
            => Counter switch
            {
                < 10 => 1000,
                < 20 => 5000,
                < 25 => 10000,
                _ => 500
            };

        #endregion

        #endregion

        #region Constructors

        public RvUser(long userId, Enums.Lang lang, string name, string? telegram, RvLocation location)
        {
            UserId = userId;
            Lang = lang;
            Name = name;
            Location = location;

            if (string.IsNullOrEmpty(telegram))
                Telegram = string.Empty;
            else
                Telegram = telegram;

            UserPermissions = new UserPermissions(PermissionsHelper.Default);
            Punishments = [];
            Rewards = [];
        }

        public RvUser()
        {
            UserPermissions = [];
            Punishments = [];
            Rewards = [];
        }

        #endregion

        #region Methods

        #region Public

        public void ResetPermissions() => UserPermissions = new UserPermissions(PermissionsHelper.Layouts[Status] + PermissionsHelper.Layouts[Role]);
        public bool Has(Permission permission) => UserPermissions.Contains(permission);

        #endregion

        #endregion

        #region Events

        public EventHandler<(RvLocation, RvLocation)>? LocationChanged;

        #endregion

    }
}
