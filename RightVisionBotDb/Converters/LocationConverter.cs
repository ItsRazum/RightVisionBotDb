using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Singletons;

namespace RightVisionBotDb.Converters
{
    public class LocationConverter : JsonConverter<IRvLocation>
    {
        private LocationManager LocationManager { get; }

        public LocationConverter(LocationManager locationManager)
        {
            LocationManager = locationManager;
        }

        public override void WriteJson(JsonWriter writer, IRvLocation? value, JsonSerializer serializer)
        {
            writer.WriteValue(LocationManager[value!.ToString()]);
        }

        public override IRvLocation? ReadJson(JsonReader reader, Type objectType, IRvLocation? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return LocationManager[JToken.Load(reader).ToString()];
        }
    }
}
