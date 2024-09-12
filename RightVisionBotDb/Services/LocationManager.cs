using DryIoc;
using RightVisionBotDb.Interfaces;

namespace RightVisionBotDb.Services
{
    public class LocationManager : Dictionary<string, IRvLocation>
    {

        public LocationManager(IContainer container)
        {
        }

        public LocationManager RegisterLocation(string locationName, Type locationType)
        {
            if (!typeof(IRvLocation).IsAssignableFrom(locationType))
                throw new ArgumentException($"Тип {locationType} не реализует интерфейс IRvLocation");

            Add(locationName, (IRvLocation)App.Container.Resolve(locationType));

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