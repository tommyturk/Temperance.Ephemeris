using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Temperance.Ephemeris.Models.Ludus;
using Temperance.Ephemeris.Repositories.Ludus.Interfaces;

namespace Temperance.Ephemeris.Repositories.Ludus.Implementations
{
    public class OptimizationResultsRepository : IOptimizationResultsRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OptimizationResultsRepository> _logger;

        public OptimizationResultsRepository(IConfiguration configuration, ILogger<OptimizationResultsRepository> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<int> SaveOptimizationResultAsync(OptimizationResultModel result)
        {
            if (result.OptimizedParameters == null)
            {
                _logger.LogWarning("Cannot save result with null parameters for {Strategy}/{Symbol}",
                    result.StrategyName, result.Symbol);
                return 0;
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            const string sql = @"
                INSERT INTO Ludus.StrategyOptimizedParameters 
                    (StrategyName, Symbol, Interval, OptimizedParametersJson, Metrics, TrainingInfo, StartDate, EndDate, CreatedAt)
                VALUES 
                    (@StrategyName, @Symbol, @Interval, @OptimizedParametersJson, @Metrics, @TrainingInfo, @StartDate, @EndDate, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            await using var connection = new SqlConnection(connectionString);

            var newId = await connection.ExecuteScalarAsync<int>(sql, new
            {
                result.StrategyName,
                result.Symbol,
                result.Interval,
                OptimizedParametersJson = JsonSerializer.Serialize(result.OptimizedParameters),
                Metrics = JsonSerializer.Serialize(result.Metrics),
                TrainingInfo = JsonSerializer.Serialize(result.TrainingInfo),
                result.StartDate,
                result.EndDate,
                CreatedAt = DateTime.UtcNow
            });

            _logger.LogInformation(
                "Successfully saved new universal parameters for {Strategy} on {Symbol}/{Interval} with new record ID {Id}",
                result.StrategyName, result.Symbol, result.Interval, newId);

            return newId;
        }
    }
}
