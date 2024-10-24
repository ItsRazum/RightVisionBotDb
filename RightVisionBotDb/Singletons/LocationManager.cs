using DryIoc;
using RightVisionBotDb.Interfaces;

namespace RightVisionBotDb.Singletons
{
    public class LocationManager : Dictionary<string, IRvLocation>
    {

        public LocationManager()
        {
        }

        public LocationManager RegisterLocation<TLocation>(string locationKey) where TLocation : IRvLocation
        {
            App.Container.Register<TLocation>(Reuse.Singleton);
            Add(locationKey, App.Container.Resolve<TLocation>());

            return this;
        }

        public string LocationToString(IRvLocation location)
        {
            return this.FirstOrDefault(x => x.Value.GetType() == location.GetType()).Key;
        }

        public IRvLocation StringToLocation(string locationName)
        {
            if (TryGetValue(locationName, out var location))
                return location;

            throw new KeyNotFoundException($"Location '{locationName}' not found.");
        }
    }
}