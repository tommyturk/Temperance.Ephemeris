using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using Temperance.Ephemeris.Models.Backtesting;
using Temperance.Ephemeris.Repositories.Constellations.Interfaces;

namespace Temperance.Ephemeris.Repositories.Constellations.Implementations
{
    public class BacktestRepository : IBacktestRepository
    {
        private readonly string _connectionString;

        public BacktestRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnectionString")
                                ?? throw new ArgumentNullException("Connection string 'DefaultConnectionString' not found.");
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<Guid> CreateAsync(BacktestRunModel run)
        {
            if (run.RunId == Guid.Empty)
            {
                run.RunId = Guid.NewGuid();
            }

            const string sql = @"
                INSERT INTO [Constellations].[BacktestRuns]
                (
                    RunId, StrategyName, SymbolsJson, IntervalsJson, 
                    StartDate, EndDate, InitialCapital, Status, StartTime, 
                    OptimizationResultId, SessionId
                )
                VALUES
                (
                    @RunId, @StrategyName, @SymbolsJson, @IntervalsJson, 
                    @StartDate, @EndDate, @InitialCapital, @Status, @StartTime, 
                    @OptimizationResultId, @SessionId
                );";

            using var connection = CreateConnection();
            await connection.ExecuteAsync(sql, run);
            return run.RunId;
        }

        public async Task<BacktestRunModel?> GetByIdAsync(Guid runId)
        {
            const string sql = "SELECT * FROM [Constellations].[BacktestRuns] WHERE RunId = @RunId";
            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<BacktestRunModel>(sql, new { RunId = runId });
        }

        public async Task<IEnumerable<BacktestRunModel>> GetByStrategyAsync(string strategyName)
        {
            const string sql = @"
                SELECT TOP 100 * FROM [Constellations].[BacktestRuns] 
                WHERE StrategyName = @StrategyName 
                ORDER BY StartTime DESC";

            using var connection = CreateConnection();
            return await connection.QueryAsync<BacktestRunModel>(sql, new { StrategyName = strategyName });
        }

        public async Task<IEnumerable<BacktestRunModel>> GetRecentAsync(int limit = 100)
        {
            const string sql = @"
                SELECT TOP (@Limit) * FROM [Constellations].[BacktestRuns] 
                ORDER BY StartTime DESC";

            using var connection = CreateConnection();
            return await connection.QueryAsync<BacktestRunModel>(sql, new { Limit = limit });
        }

        public async Task UpdateStatusAsync(Guid runId, string status, string? errorMessage = null)
        {
            const string sql = @"
                UPDATE [Constellations].[BacktestRuns]
                SET Status = @Status,
                    ErrorMessage = COALESCE(@ErrorMessage, ErrorMessage),
                    EndTime = CASE WHEN @Status IN ('Completed', 'Failed') THEN SYSDATETIME() ELSE EndTime END
                WHERE RunId = @RunId";

            using var connection = CreateConnection();
            await connection.ExecuteAsync(sql, new { RunId = runId, Status = status, ErrorMessage = errorMessage });
        }

        public async Task UpdateResultsAsync(BacktestRunModel run)
        {
            const string sql = @"
                UPDATE [Constellations].[BacktestRuns]
                SET 
                    TotalProfitLoss = @TotalProfitLoss,
                    TotalReturn = @TotalReturn,
                    MaxDrawdown = @MaxDrawdown,
                    WinRate = @WinRate,
                    TotalTrades = @TotalTrades,
                    SharpeRatio = @SharpeRatio,
                    Status = 'Completed',
                    EndTime = SYSDATETIME()
                WHERE RunId = @RunId";

            using var connection = CreateConnection();
            await connection.ExecuteAsync(sql, run);
        }
    }
}
