using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RightVisionBotDb.Locations;
using RightVisionBotDb.Services;

namespace RightVisionBotDb.Converters
{
    public class LocationConverter : JsonConverter<RvLocation>
    {
        private LocationService LocationService { get; }

        public LocationConverter(LocationService locationService)
        {
            LocationService = locationService;
        }

        public override void WriteJson(JsonWriter writer, RvLocation? value, JsonSerializer serializer)
        {
            writer.WriteValue(LocationService[value!.ToString()]);
        }

        public override RvLocation? ReadJson(JsonReader reader, Type objectType, RvLocation? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return LocationService[JToken.Load(reader).ToString()];
        }
    }
}
