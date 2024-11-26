using DryIoc;
using RightVisionBotDb.Locations;

namespace RightVisionBotDb.Services
{
    public class LocationService : Dictionary<string, RvLocation>
    {

        public LocationService()
        {
        }

        public LocationService RegisterLocation<TLocation>(string locationKey) where TLocation : RvLocation
        {
            App.Container.Register<TLocation>(Reuse.Singleton);
            Add(locationKey, App.Container.Resolve<TLocation>());

            return this;
        }

        public string LocationToString(RvLocation location)
        {
            return this.FirstOrDefault(x => x.Value.GetType() == location.GetType()).Key;
        }

        public RvLocation StringToLocation(string locationName)
        {
            if (TryGetValue(locationName, out var location))
                return location;

            throw new KeyNotFoundException($"Location '{locationName}' not found.");
        }
    }
}