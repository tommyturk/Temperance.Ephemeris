using Temperance.Ephemeris.Models.Backtesting;

namespace Temperance.Ephemeris.Services.Constellations.Interfaces
{
    public interface IBacktestService
    {
        Task<BacktestRunModel> InitializeRunAsync(string strategyName, List<string> symbols, List<string> intervals, double capital, DateTime start, DateTime end, string parametersJson = "{}", int? sessionId = null, int? optimizationResultId = null);
        Task RecordCompletionAsync(Guid runId, double pnl, double totalReturn, double drawdown, double winRate, int totalTrades, double sharpeRatio);
        Task RecordFailureAsync(Guid runId, string errorMessage);
        Task<BacktestRunModel?> GetRunAsync(Guid runId);
    }
}
