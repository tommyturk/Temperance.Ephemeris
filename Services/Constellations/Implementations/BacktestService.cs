using Microsoft.Extensions.Logging;
using System.Text.Json;
using Temperance.Ephemeris.Models.Backtesting;
using Temperance.Ephemeris.Repositories.Constellations.Interfaces;
using Temperance.Ephemeris.Services.Constellations.Interfaces;

namespace Temperance.Ephemeris.Services.Constellations.Implementations
{
    public class BacktestService : IBacktestService
    {
        private readonly IBacktestRepository _repository;
        private readonly ILogger<BacktestService> _logger;

        public BacktestService(IBacktestRepository repository, ILogger<BacktestService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<BacktestRunModel> InitializeRunAsync(string strategyName, List<string> symbols, List<string> intervals, double capital, DateTime start, DateTime end, string parametersJson = "{}", int? sessionId = null, int? optimizationResultId = null)
        {
            var run = new BacktestRunModel
            {
                RunId = Guid.NewGuid(),
                SessionId = sessionId,
                StrategyName = strategyName,
                SymbolsJson = JsonSerializer.Serialize(symbols),
                IntervalsJson = JsonSerializer.Serialize(intervals),
                StartDate = start,
                EndDate = end,
                InitialCapital = (decimal)capital,
                StartTime = DateTime.UtcNow,
            };

            try
            {
                await _repository.CreateAsync(run);
                _logger.LogInformation("Started Backtest {RunId} for {Strategy}", run.RunId, strategyName);
                return run;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting backtest for {Strategy}", strategyName);
                throw;
            }
        }

        public async Task RecordCompletionAsync(Guid runId, double pnl, double totalReturn, double drawdown, double winRate, int totalTrades, double sharpeRatio)
        {
            var run = new BacktestRunModel
            {
                RunId = runId,
                TotalProfitLoss = (decimal)pnl,
                TotalReturn = (decimal)totalReturn,
                MaxDrawdown = (decimal)drawdown,
                WinRate = (decimal)winRate,
                TotalTrades = totalTrades,
                SharpeRatio = (decimal)sharpeRatio
            };

            try
            {
                await _repository.UpdateResultsAsync(run);
                _logger.LogInformation("Completed Backtest {RunId}. PnL: {PnL}, Sharpe: {Sharpe}", runId, pnl, sharpeRatio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording completion for {RunId}", runId);
                throw;
            }
        }

        public async Task RecordFailureAsync(Guid runId, string errorMessage)
        {
            try
            {
                await _repository.UpdateStatusAsync(runId, "Failed", errorMessage);
                _logger.LogWarning("Backtest {RunId} Failed: {Error}", runId, errorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording failure for {RunId}", runId);
                throw;
            }
        }

        public async Task<BacktestRunModel?> GetRunAsync(Guid runId)
        {
            return await _repository.GetByIdAsync(runId);
        }
    }
}
