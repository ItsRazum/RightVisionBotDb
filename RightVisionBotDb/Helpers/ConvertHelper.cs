using Newtonsoft.Json;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Types;
using Serilog;
using System.Globalization;
using System.Text;

namespace RightVisionBotDb.Helpers
{
    internal class ConvertHelper
    {

        #region string <-> UserPermissions

        public static UserPermissions StringToPermissions(string value)
        {
            try
            {
                var parts = value.Split(':');
                var collection = JsonConvert.DeserializeObject<List<Permission>>(parts[0]);
                var removed = JsonConvert.DeserializeObject<List<Permission>>(parts[1]);
                return new UserPermissions(collection ?? [], removed ?? []);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Произошла ошибка при преобразовании строки в права доступа");
                throw;
            }
        }

        public static string PermissionsToString(UserPermissions value) 
        {
            StringBuilder sb = new("[ ");
            foreach (var perm in value)
                sb.Append($"\"{perm}\",");

            sb.Append(']');

            sb.Append(":[");
            foreach (var blockedPerm in value.Removed)
                sb.Append($"\"{blockedPerm}\",");

            sb.Append(']');
            return sb.ToString();
        }

        #endregion

        #region string <-> RvPunishments

        public static List<RvPunishment> StringToRvPunishments(string value) 
        {
            var collection = JsonConvert.DeserializeObject<List<string>>(value);
            if (collection != null)
            {
                var punishments = new List<RvPunishment>();
                foreach (var item in collection)
                {
                    try
                    {
                        var values = item.Split(';');
                        var punishment
                            = new RvPunishment(
                                Enum.Parse<PunishmentType>(values[0]),
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

        public static string RvPunishmentsToString(List<RvPunishment> value)
        {
            StringBuilder sb = new("[ ");
            foreach (var punishment in value)
                sb.Append("\"" + punishment.ToString() + "\", ");

            try
            {
                if (value.Count > 0)
                    sb.Remove(sb.Length - 2, 2);
            }
            catch
            {
                Log.Logger.Warning("Не удалось обрезать {RvPunishments.ToString()}. Возможно, коллекция пуста");
            }
            sb.Append(" ]");
            return sb.ToString();
        }

        #endregion

        #region string <-> Rewards

        public static List<Reward> StringToRewards(string value) 
        {
            var collection = JsonConvert.DeserializeObject<List<string>>(value);
            if (collection != null)
            {
                var rewards = new List<Reward>();
                foreach (var item in collection)
                {
                    try
                    {
                        var values = item.Split(':');
                        var reward
                            = new Reward(
                                values[1],
                                values[2]
                                );
                        rewards.Add(reward);
                    }
                    catch
                    {
                        Log.Logger.Error($"Не удалось преобразовать строку {item} в Reward");
                    }
                }
                return rewards;
            }
            else
            {
                Log.Logger.Error("Не удалось преобразовать строку в коллекцию ({RvPunisments.FromString()})");
                throw new NullReferenceException(nameof(value));
            }
        }

        public static string RewardsToString(List<Reward> value)
        {
            StringBuilder sb = new("[ ");
            foreach (var reward in value)
                sb.Append("\"" + (value.IndexOf(reward) + 1) + reward.Icon + ":" + ":" + reward.Description + "\", ");

            try
            {
                if (value.Count > 0)
                    sb.Remove(sb.Length - 2, 2);
            }
            catch
            {
                Log.Logger.Warning("Не удалось обрезать {Rewards.ToString()}. Возможно, коллекция пуста");
            }
            sb.Append(" ]");
            return sb.ToString();
        }
        #endregion
    }
}
