using RightVisionBotDb.Common;
using RightVisionBotDb.Data;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Types;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Timers;

namespace RightVisionBotDb.Models
{
    public class RvUser
    {
        #region Properties

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserId { get; set; }
        public string Name { get; set; }
        public string Telegram { get; set; }
        public Enums.Lang Lang { get; set; }
        public Category Category { get; set; } = Category.None;
        public Status Status { get; set; } = Status.User;
        public Role Role { get; set; } = Role.None;
        public RvLocation Location { get; set; } = RvLocation.MainMenu;
        public string Profile { get; set; }
        public UserPermissions Permissions { get; set; }
        public RvPunishments Punishments { get; set; }
        public Rewards Rewards { get; set; }

        #region Cooldown Properties

        [NotMapped] public System.Timers.Timer? Cooldown { get; set; }
        [NotMapped] private int Counter { get; set; }
        [NotMapped] private System.Timers.Timer? CounterCooldown { get; set; }
        [NotMapped] private int TimerInterval 
            => Counter switch
            {
                < 10 => 1000,
                < 20 => 5000,
                < 25 => 10000,
                _ => 500
            };

        #endregion

        #endregion

        #region Constructor

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

        public void ResetPermissions() => Permissions = new UserPermissions(Common.Permissions.Layouts[Status] + Common.Permissions.Layouts[Role], UserId);
        public bool Has(Permission permission) => Permissions.Contains(permission);
        public static RvUser Get(long userId) => Db.Context.RvUsers.First(x => x.UserId == userId);


        #endregion

        #endregion

    }
}
