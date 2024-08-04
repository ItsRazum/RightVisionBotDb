using RightVisionBotDb.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Serilog;
using Newtonsoft.Json;
using System.Globalization;

namespace RightVisionBotDb.Types
{
    public class Reward
    {
        public Reward(string icon, string description)
        {
            Icon = icon;
            Description = description;
        }

        public string Icon { get; set; }
        public string Description { get; set; }
    }

    public class Rewards
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long RvUserId { get; private set; }
        public Dictionary<int, Reward> Collection { get; } = new();

        public Rewards(long rvUserId = 0)
        {
            RvUserId = rvUserId;
        }

        public override string ToString()
        {
            StringBuilder sb = new("[ ");
            foreach (var reward in Collection)
                sb.Append("\"" + reward.Value.Icon + ":" + reward.Key + ":" + reward.Value.Description + "\", ");

            try
            {
                sb.Remove(sb.Length - 2, 2);
            }
            catch
            {
                Log.Logger.Warning("Не удалось обрезать {Rewards.ToString()}. Возможно, коллекция пуста");
            }
            sb.Append(" ]");
            return sb.ToString();
        }

        public static Rewards FromString(string s)
        {
            var parts = s.Split(':', 2);
            var userId = long.Parse(parts[0]);
            var value = parts[1];

            var collection = JsonConvert.DeserializeObject<List<string>>(value);
            if (collection != null)
            {
                var rewards = new Rewards();
                foreach (var item in collection)
                {
                    try
                    {
                        var values = item.Split(':');
                        var reward
                            = new Reward(
                                values[0],
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

        public int Count => Collection.Count;

        public void Add(Reward reward)
        {
            Collection.Add(Collection.Count + 1, reward);

            Db.Context.SaveChanges();
        }
    }
}
