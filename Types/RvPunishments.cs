using RightVisionBotDb.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using Serilog;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace RightVisionBotDb.Types
{
    public class RvPunishment
    {
        #region Fields

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long RvUserId { get; private set; }
        public PunishmentType Type { get; set; }
        public long GroupId { get; set; }
        public string? Reason { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        #endregion

        #region Constructor

        public RvPunishment(PunishmentType type, long groupId, string? reason, DateTime from, DateTime to)
        {
            Type = type;
            GroupId = groupId;
            Reason = reason;
            From = from;
            To = to;
        }

        #endregion

        #region Enums

        public enum PunishmentType
        {
            Ban,
            Mute
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append($"{Type};");
            sb.Append(GroupId + ";");
            sb.Append(Reason + ";");
            sb.Append(From.ToString(CultureInfo.GetCultureInfo("en-US").DateTimeFormat) + ";");
            sb.Append(To.ToString(CultureInfo.GetCultureInfo("en-US").DateTimeFormat) + ",");
            return sb.ToString();
        }

        #endregion

    }

    public class RvPunishments
    {
        public List<RvPunishment> Collection { get; } = new();
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long RvUserId { get; private set; }

        public RvPunishments(long rvUserId = 0)
        {
            RvUserId = rvUserId;
        }

        public static RvPunishments FromString(string s)
        {
            var parts = s.Split(':', 2);
            var userId = long.Parse(parts[0]);
            var value = parts[1];

            var collection = JsonConvert.DeserializeObject<List<string>>(value);
            if (collection != null)
            {
                var punishments = new RvPunishments();
                foreach(var item in collection)
                {
                    try
                    {
                        var values = item.Split(';');
                        var punishment
                            = new RvPunishment(
                                Enum.Parse<RvPunishment.PunishmentType>(values[0]),
                                long.Parse(values[1]),
                                values[2],
                                DateTime.Parse(values[3], CultureInfo.GetCultureInfo("en-US")),
                                DateTime.Parse(values[4], CultureInfo.GetCultureInfo("en-US"))
                                );
                        punishments.Add(punishment);
                    }
                    catch 
                    {
                        Log.Logger.Error($"Не удалось преобразовать строку {item} в RvPunishment");
                    }
                }
                return punishments;
            }
            else
            {
                Log.Logger.Error("Не удалось преобразовать строку в коллекцию ({RvPunisments.FromString()})");
                throw new NullReferenceException(nameof(value));
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new("[ ");
            foreach (var punishment in Collection)
                sb.Append("\"" + punishment.ToString() + "\", ");

            try
            {
                sb.Remove(sb.Length - 2, 2);
            }
            catch
            {
                Log.Logger.Warning("Не удалось обрезать {RvPunishments.ToString()}. Возможно, коллекция пуста");
            }
            sb.Append(" ]");
            return sb.ToString();
        }

        public void Add(RvPunishment punishment)
        {
            Collection.Add(punishment);
            Collection.Reverse();

            Db.Context.SaveChanges();
        }
    }
}
