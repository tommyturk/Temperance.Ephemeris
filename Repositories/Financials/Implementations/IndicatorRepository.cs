using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;
using Temperance.Ephemeris.Repositories.Financials.Interfaces;
using Temperance.Ephemeris. Models.Indicators;
using Temperance.Ephemeris.Utilities.Common;

namespace Temperance.Ephemeris.Repositories.Financials.Implementations
{
    public class IndicatorRepository : IIndicatorRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<IndicatorRepository> _logger;

        public IndicatorRepository(string connectionString, ILogger<IndicatorRepository> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }
        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        private async Task<bool> BackfillIndicatorAsync<T>(T indicatorData, string tableName) where T : IndicatorBaseModel
        {
            if (indicatorData?.Data == null || !indicatorData.Data.Any())
            {
                _logger.LogInformation("No values provided in the model for {TableName} backfill.", tableName);
                return true; 
            }

            var sql = $@"
                MERGE INTO [TradingBotDb].[Indicators].[{tableName}] AS target
                USING (SELECT @Date AS Date) AS source
                ON (target.Date = source.Date)
                WHEN NOT MATCHED THEN
                    INSERT (Date, Value, Interval)
                    VALUES (@Date, @Value, @Interval);";

            try
            {
                var parametersToInsert = indicatorData.Data.Select(valueSet => new
                {
                    Name = indicatorData.Name,
                    Date = valueSet.Date,
                    Value = valueSet.Value,
                    Interval = indicatorData.Interval
                }).ToList();

                using var connection = CreateConnection();

                var rowsAffected = await connection.ExecuteAsync(sql, parametersToInsert);

                _logger.LogInformation("Successfully processed backfill for {TableName}. Inserted {InsertedCount} new records out of {TotalCount} total value sets.", tableName, rowsAffected, parametersToInsert.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during the backfill process for {TableName}.", tableName);
                return false;
            }
        }

        public async Task UpdateMarketIndicatorBackFillStatus(int indicatorId, string status)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE [Historical].[BackFill].[MarketIndicatorQueue]
                SET 
                    [Status] = @Status,
                    [StartTime] = CASE WHEN @Status = 'Running' THEN GETUTCDATE() ELSE [StartTime] END
                WHERE 
                    [Id] = @IndicatorId;
            ";

            try
            {
                var parameters = new { Status = status, IndicatorId = indicatorId };
                var affectedRows = await connection.ExecuteAsync(sql, parameters);

                if (affectedRows > 0)
                {
                    _logger.LogDebug("Successfully updated status to '{Status}' for queue item Id {Id}.", status, indicatorId);
                }
                else
                {
                    _logger.LogWarning("Attempted to update status for queue item Id {Id}, but no matching record was found.", indicatorId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating status for queue item Id {Id}.", indicatorId);
                throw; 
            }
        }

        public async Task<bool> InflationBackfill(InflationModel inflationData)
        {
            return await BackfillIndicatorAsync(inflationData, "Inflation");
        }

        public async Task<bool> RealGdpBackfill(RealGdpModel realGdpData)
        {
            return await BackfillIndicatorAsync(realGdpData, "Real_GDP");
        }

        public async Task<bool> RealGdpPerCapitaBackFill(RealGdpPerCapitaModel realGdpPerCapitaData)
        {
            return await BackfillIndicatorAsync(realGdpPerCapitaData, "Real_GDP_Per_Capita");
        }

        public async Task<bool> UnemploymentRateBackfill(UnemploymentRatesModel unemploymentRateData)
        {
            return await BackfillIndicatorAsync(unemploymentRateData, "Unemployment_Rate");
        }

        public async Task<bool> FederalFundsRateBackFill(FederalFundInterestRateModel interestModel)
        {
            return await BackfillIndicatorAsync(interestModel, "Federal_Funds_Rate");
        }

        public async Task<bool> RealGdpPerCapitaBackFill(RealGdpModel realGdpPerCapitaData)
        {
            return await BackfillIndicatorAsync(realGdpPerCapitaData, "Real_GDP_Per_Capita");
        }

        public async Task<bool> TreasuryYieldsBackFill(TreasuryYieldsModel treasuryYieldsData)
        {
            if (treasuryYieldsData?.Data == null || !treasuryYieldsData.Data.Any())
            {
                _logger.LogInformation("No values provided in the model for Treasury_Yields backfill.");
                return true;
            }

            const string sql = @"
                MERGE INTO [Indicators].[Treasury_Yields] AS target
                USING (SELECT @Date AS Date, @Maturity AS Maturity) AS source
                ON (target.Date = source.Date AND target.Maturity = source.Maturity)
                WHEN NOT MATCHED THEN
                    INSERT (Date, Value, Interval, Maturity)
                    VALUES (@Date, @Value, @Interval, @Maturity);
            ";

            try
            {
                var parametersToInsert = treasuryYieldsData.Data.Select(valueSet => new
                {
                    Indicator = treasuryYieldsData.Name,
                    Date = valueSet.Date,
                    Value = valueSet.Value,
                    Interval = treasuryYieldsData.Interval,
                    Maturity = treasuryYieldsData.Maturity 
                }).ToList();

                using var connection = CreateConnection();
                var rowsAffected = await connection.ExecuteAsync(sql, parametersToInsert);

                _logger.LogInformation("Successfully processed backfill for Treasury_Yields ({Maturity}). Inserted {InsertedCount} new records.", treasuryYieldsData.Maturity, rowsAffected);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during the backfill process for Treasury_Yields.");
                return false;
            }
        }

        public async Task<List<IndicatorBackFillStatus>> CheckMarketIndicatorBackfillQueue()
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT [Id], [Indicator], [Interval], [Status], [StartTime], [EndTime]
                FROM [Historical].[BackFill].[MarketIndicatorQueue]
                WHERE 
                    [Status] = 'Pending' 
                    AND [StartTime] <= GETUTCDATE()
                ORDER BY 
                    [StartTime] ASC;
            ";

            try
            {
                var dueItems = await connection.QueryAsync<IndicatorBackFillStatus>(sql);
                return dueItems.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve due indicator items from the queue.");
                return new List<IndicatorBackFillStatus>();
            }
        }

        public async Task InsertIndicatorQueueItem(IndicatorBackFillStatus indicator)
        {
            using var connection = CreateConnection();
            const string sql = @"
                INSERT INTO [Historical].[BackFill].[MarketIndicatorQueue] (Indicator, Interval, Status, StartTime)
                VALUES (@Indicator, @Interval, @Status, @StartTime);";
            try
            {
                await connection.ExecuteAsync(sql, indicator);
                _logger.LogInformation("Scheduled next queue item for {Indicator} ({Interval}) at {StartTime}.",
                    indicator.Indicator, indicator.Interval, indicator.StartTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to insert next indicator queue item for {Indicator}.", indicator.Indicator);
                throw;
            }
        }
    }
}
