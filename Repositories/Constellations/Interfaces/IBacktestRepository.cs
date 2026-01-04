using Temperance.Ephemeris.Models.Backtesting;

namespace Temperance.Ephemeris.Repositories.Constellations.Interfaces
{
    public interface IBacktestRepository
    {
        Task<Guid> CreateAsync(BacktestRunModel run);
        Task<BacktestRunModel?> GetByIdAsync(Guid runId);
        Task<IEnumerable<BacktestRunModel>> GetByStrategyAsync(string strategyName);
        Task<IEnumerable<BacktestRunModel>> GetRecentAsync(int limit = 100);
        Task UpdateStatusAsync(Guid runId, string status, string? errorMessage = null);
        Task UpdateResultsAsync(BacktestRunModel run);
    }
}
