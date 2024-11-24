using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RightVisionBotDb.Locations;
using RightVisionBotDb.Singletons;

namespace RightVisionBotDb.Converters
{
    public class LocationConverter : JsonConverter<RvLocation>
    {
        private LocationManager LocationManager { get; }

        public LocationConverter(LocationManager locationManager)
        {
            LocationManager = locationManager;
        }

        public override void WriteJson(JsonWriter writer, RvLocation? value, JsonSerializer serializer)
        {
            writer.WriteValue(LocationManager[value!.ToString()]);
        }

        public override RvLocation? ReadJson(JsonReader reader, Type objectType, RvLocation? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return LocationManager[JToken.Load(reader).ToString()];
        }
    }
}
