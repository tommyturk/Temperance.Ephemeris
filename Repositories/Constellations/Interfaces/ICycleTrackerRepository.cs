using Temperance.Ephemeris.Models.Constellations;

namespace Temperance.Ephemeris.Repositories.Constellations.Interfaces
{
    public interface ICycleTrackerRepository
    {
        Task CreateCycle(CycleTrackerModel cycle);
    }
}
