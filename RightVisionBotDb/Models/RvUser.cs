using RightVisionBotDb.Enums;
using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Permissions;
using RightVisionBotDb.Types;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Timers;

namespace RightVisionBotDb.Models
{
    public class RvUser
    {

        #region Fields

        private IRvLocation location;

        #endregion

        #region Properties

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserId { get; set; }
        public string Name { get; set; }
        public string Telegram { get; set; }
        public Enums.Lang Lang { get; set; }
        public Status Status { get; set; } = Status.User;
        public Role Role { get; set; } = Role.None;
        public IRvLocation Location
        {
            get => location;
            set
            {
                if (location == value) return;
                LocationChanged?.Invoke(this, (Location, value));
                location = value;
            }
        }
        public UserPermissions Permissions { get; set; }
        public RvPunishments Punishments { get; set; }
        public Rewards Rewards { get; set; }

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

        public RvUser(long userId, Enums.Lang lang, string name, string? telegram, IRvLocation location)
        {
            UserId = userId;
            Lang = lang;
            Name = name;
            Location = location;

            if (string.IsNullOrEmpty(telegram))
                Telegram = string.Empty;
            else
                Telegram = telegram;

            Permissions = new UserPermissions(UserId);
            Punishments = new RvPunishments(UserId);
            Rewards = new Rewards(UserId);
        }

        public RvUser()
        {
            Permissions = new UserPermissions(UserId);
            Punishments = new RvPunishments(UserId);
            Rewards = new Rewards(UserId);
        }

        #endregion

        #region Methods

        #region Private

        private void CooldownSettings()
        {
            Counter++;
            if (Counter == 1)
            {
                CounterCooldown = new System.Timers.Timer(30000);
                CounterCooldown.Elapsed += CounterCooldownElapsed;
                CounterCooldown.Start();
            }

            if (Cooldown is { Enabled: true }) return;
            Cooldown = new System.Timers.Timer(TimerInterval);
            Cooldown.Elapsed += CooldownElapsed;
            Cooldown.Start();
        }

        private void CooldownElapsed(object? sender, ElapsedEventArgs e) => Cooldown?.Stop();
        private void CounterCooldownElapsed(object? sender, ElapsedEventArgs e)
        {
            CounterCooldown?.Stop();
            Counter = 0;
        }

        #endregion

        #region Public

        public void ResetPermissions() => Permissions = new UserPermissions(RightVisionBotDb.Permissions.Permissions.Layouts[Status] + RightVisionBotDb.Permissions.Permissions.Layouts[Role], UserId);
        public bool Has(Permission permission) => Permissions.Contains(permission);

        #endregion

        #endregion

        #region Events

        public EventHandler<(IRvLocation, IRvLocation)>? LocationChanged;

        #endregion

    }
}
