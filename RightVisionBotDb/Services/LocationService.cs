using DryIoc;
using RightVisionBotDb.Locations;

namespace RightVisionBotDb.Services
{
    public class LocationService : Dictionary<string, RvLocation>
    {

        private readonly IContainer _container;

        public LocationService(IContainer container)
        {
            _container = container;
        }

        public LocationService RegisterLocation<TLocation>() where TLocation : RvLocation
        {
            var locationKey = typeof(TLocation).Name;
            _container.Register<TLocation>(Reuse.Singleton);
            Add(locationKey, _container.Resolve<TLocation>());

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