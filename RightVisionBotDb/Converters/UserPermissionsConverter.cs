using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Types;
using System.Diagnostics;

namespace RightVisionBotDb.Converters
{
    public class UserPermissionsConverter : JsonConverter<UserPermissions>
    {
        public override UserPermissions? ReadJson(JsonReader reader, Type objectType, UserPermissions? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var permissionsString = JToken.Load(reader).ToString();

            var parts = permissionsString.Split(':');
            Debug.WriteLine(string.Join(":", parts));
            var collection = JsonConvert.DeserializeObject<List<Permission>>(parts[0]);
            var removed = JsonConvert.DeserializeObject<List<Permission>>(parts[1]);
            return new UserPermissions(collection ?? [], removed ?? []);
        }

        public override void WriteJson(JsonWriter writer, UserPermissions? value, JsonSerializer serializer)
        {
            var permissionsJson = JsonConvert.SerializeObject(value!.Collection.Select(p => p.ToString()));
            var removedJson = JsonConvert.SerializeObject(value.Removed.Select(p => p.ToString()));

            writer.WriteValue($"[{permissionsJson}]:[{removedJson}]");
        }
    }
}
