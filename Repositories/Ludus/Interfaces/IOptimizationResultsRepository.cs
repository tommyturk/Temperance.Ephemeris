using Temperance.Ephemeris.Models.Ludus;

namespace Temperance.Ephemeris.Repositories.Ludus.Interfaces
{
    public interface IOptimizationResultsRepository
    {
        Task<int> SaveOptimizationResultAsync(OptimizationResultModel result);
    }
}
