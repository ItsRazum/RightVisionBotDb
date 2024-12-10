using RightVisionBotDb.Locations;

namespace RightVisionBotDb.Interfaces
{
    public interface ILocationMapping
    {
        string LocationToString(RvLocation location);
        RvLocation StringToLocation(string key);
    }
}
