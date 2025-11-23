using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Data;
using Temperance.Ephemeris.Models.BackFill;
using Temperance.Ephemeris.Models.Prices;
using Temperance.Ephemeris.Repositories_Old.Securities;
using Temperance.Ephemeris.ModelsHelpers;

namespace Temperance.Ephemeris.Repositories_Old.HistoricalPrices
{
    public class HistoricalPriceRepository : IHistoricalPriceRepository
    {
        private readonly string _historicalPriceConnectionString;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _tableLocks = new();
        private readonly ISecuritiesOverviewRepository _securitiesOverviewRepository;
        private readonly ISqlHelper _sqlHelper;
        private readonly ILogger<HistoricalPriceRepository> _logger;
        public HistoricalPriceRepository(string connectionString, ISecuritiesOverviewRepository securitiesOverviewRepository,
            ISqlHelper sqlHelpler, ILogger<HistoricalPriceRepository> logger)
        {
            _historicalPriceConnectionString = connectionString;
            _securitiesOverviewRepository = securitiesOverviewRepository;
            _sqlHelper = sqlHelpler;
            _logger = logger;
            if (string.IsNullOrWhiteSpace(_historicalPriceConnectionString))
                _logger.LogCritical("FATAL: SecuritiesOverviewRepository created with a NULL or EMPTY connection string!");
            else
            {
                var builder = new SqlConnectionStringBuilder(_historicalPriceConnectionString);
                _logger.LogInformation("SecuritiesOverviewRepository instance created.");
                _logger.LogInformation("Attempting to connect to SERVER: [{Server}], DATABASE: [{Database}]", builder.DataSource, builder.InitialCatalog);
            }
        }

        public async Task<bool> DoesDailyDataExistAsync(string symbol)
        {
            var tableName = $"[Prices].[{symbol}_1d]";
            var sql = $"SELECT TOP 1 1 FROM {tableName}";
            using (var connection = new SqlConnection(_historicalPriceConnectionString))
            {
                await connection.OpenAsync();
                try
                {
                    var result = await connection.QuerySingleOrDefaultAsync<int?>(sql);
                    return result.HasValue;
                }
                catch
                {
                    // Table likely doesn't exist, which means no data
                    return false;
                }
            }
        }

        public async Task<IEnumerable<SecurityDataCoverageModel>> GetMonthlyDataCoverageAsync(string symbol, string interval)
        {
            using (var connection = new SqlConnection(_historicalPriceConnectionString))
            {
                await connection.OpenAsync();

                var tableName = _sqlHelper.SanitizeTableName(symbol, interval);

                await _sqlHelper.EnsureTableExists(tableName);

                var sql = $@"
                        SELECT
                            YEAR([Timestamp]) AS [Year],
                            MONTH([Timestamp]) AS [Month]
                        FROM {tableName}
                        GROUP BY
                            YEAR([Timestamp]),
                            MONTH([Timestamp])
                        ORDER BY
                            [Year], [Month];";
                try
                {
                    return await connection.QueryAsync<SecurityDataCoverageModel>(sql, new { TableName = tableName });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error querying data coverage for {tableName}: {ex.Message}");
                    return Enumerable.Empty<SecurityDataCoverageModel>();
                }
            }
        }

        public async Task<List<PriceModel>> GetSecurityHistoricalPrices(string symbol, string interval)
        {
            var prices = new List<PriceModel>();
            using (var connection = new SqlConnection(_historicalPriceConnectionString))
            {
                await connection.OpenAsync();

                var tableName = _sqlHelper.SanitizeTableName(symbol, interval);

                await _sqlHelper.EnsureTableExists(tableName);

                string query = $@"
                SELECT TOP(10) *
                FROM {tableName}
                WHERE Symbol = @Symbol AND TimeInterval = @TimeInterval
                ORDER BY [Timestamp] DESC";

                var data = await connection.QueryAsync<PriceModel>(query, new
                {
                    Symbol = symbol,
                    TimeInterval = interval
                });

                prices = data.ToList();
            }

            return prices;
        }

        public async Task<DateTime?> GetMostRecentTimestamp(string symbol, string interval)
        {
            var result = new DateTime?();
            using (var connection = new SqlConnection(_historicalPriceConnectionString))
            {
                await connection.OpenAsync();
                var tableName = _sqlHelper.SanitizeTableName(symbol, interval);

                await _sqlHelper.EnsureTableExists(tableName);

                var query = $"SELECT TOP 1 Timestamp FROM {tableName} " +
                            "WHERE Symbol = @Symbol AND TimeInterval = @TimeInterval " +
                            "ORDER BY Timestamp DESC";

                result = await connection.QueryFirstOrDefaultAsync<DateTime?>(query, new { Symbol = symbol, TimeInterval = interval });
            }

            return result;
        }

        public async Task<decimal> GetLatestPriceAsync(string symbol, DateTime backtestTimestamp)
        {
            //using var connection = new SqlConnection(_connectionString);
            //string query = @"
            //SELECT TOP 1 ClosePrice 
            //FROM [TradingBotDb].[Financials].[HistoricalPrices]
            //WHERE Symbol = @Symbol AND Timestamp <= @BacktestTimestamp
            //ORDER BY Timestamp DESC"
            //;

            //var latestPrice = await _connection.QueryFirstOrDefaultAsync<decimal>(
            //    query,
            //    new { Symbol = symbol, BacktestTimestamp = backtestTimestamp }
            //);

            //if (latestPrice == 0)
            //    throw new Exception($"No historical data found for {symbol} before {backtestTimestamp}.");

            //return latestPrice;
            return new decimal();
        }

        public async Task<bool> UpdateHistoricalIntradayPrices(List<PriceModel> prices, string symbol, string timeInterval)
        {
            var success = true;
            const int batchSize = 5000;

            Console.WriteLine($"Starting update process for symbol {symbol} with {prices.Count} prices at {DateTime.UtcNow}");

            var securityId = await _securitiesOverviewRepository.GetSecurityId(symbol);

            for (int i = 0; i < prices.Count; i += batchSize)
            {
                var batch = prices.Skip(i).Take(batchSize).ToList();

                Console.WriteLine($"Processing batch {i / batchSize + 1} with {batch.Count} records for symbol {symbol}.");

                var batchSuccess = await InsertBatchPriceRecords(securityId, batch, timeInterval);
                success &= batchSuccess;

                if (!batchSuccess)
                {
                    Console.WriteLine($"Batch {i / batchSize + 1} failed for symbol {symbol}.");
                }
            }

            Console.WriteLine($"Update process for symbol {symbol} completed at {DateTime.UtcNow}. Success: {success}");
            return success;
        }

        

        public async Task<List<PriceModel>> GetHistoricalPricesForMonth(string symbol, string timeInterval, DateTime startDate, DateTime endDate)
        {
            using (var connection = new SqlConnection(_historicalPriceConnectionString))
            {
                await connection.OpenAsync();

                var parameters = new { Symbol = symbol, TimeInterval = timeInterval, StartDate = startDate, EndDate = endDate };
                var tableName = _sqlHelper.SanitizeTableName(symbol, timeInterval);
                var query = $"SELECT * FROM {tableName} " +
                    "WHERE Symbol = @Symbol AND TimeInterval = @TimeInterval AND Timestamp >= @StartDate AND Timestamp <= @EndDate";
                var result = await connection.QueryAsync<PriceModel>(query, parameters);
                return result.ToList();
            }
        }

        public async Task<List<PriceModel>> GetHistoricalPrices(string symbol, string interval)
        {
            var tableName = _sqlHelper.SanitizeTableName(symbol, interval);
            using (var connection = new SqlConnection(_historicalPriceConnectionString))
            {
                await connection.OpenAsync();

                return (await connection.QueryAsync<PriceModel>(
                    $@"SELECT * FROM {tableName} 
                   ORDER BY Timestamp")).ToList();
            }
        }

        private async Task<bool> InsertBatchPriceRecords(int securityId, List<PriceModel> prices, string timeInterval)
        {
            if (securityId == 0 || !prices.Any())
                return false;

            var symbol = prices.First().Symbol;
            var tableName = _sqlHelper.SanitizeTableName(symbol, timeInterval);

            await _sqlHelper.EnsureTableExists(tableName);

            using (var connection = new SqlConnection(_historicalPriceConnectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    using (var bulkCopy = new SqlBulkCopy(connection))
                    {
                        bulkCopy.DestinationTableName = tableName;

                        bulkCopy.ColumnMappings.Add("SecurityID", "SecurityID");
                        bulkCopy.ColumnMappings.Add("Symbol", "Symbol");
                        bulkCopy.ColumnMappings.Add("Timestamp", "Timestamp");
                        bulkCopy.ColumnMappings.Add("OpenPrice", "OpenPrice");
                        bulkCopy.ColumnMappings.Add("HighPrice", "HighPrice");
                        bulkCopy.ColumnMappings.Add("LowPrice", "LowPrice");
                        bulkCopy.ColumnMappings.Add("ClosePrice", "ClosePrice");
                        if (timeInterval == "1d")
                        {
                            bulkCopy.ColumnMappings.Add("AdjustedClosePrice", "AdjustedClosePrice");
                            bulkCopy.ColumnMappings.Add("DividendAmount", "DividendAmount");
                            bulkCopy.ColumnMappings.Add("SplitCoefficient", "SplitCoefficient");
                        }
                        bulkCopy.ColumnMappings.Add("Volume", "Volume");
                        bulkCopy.ColumnMappings.Add("TimeInterval", "TimeInterval");


                        using var dataTable = new DataTable();

                        dataTable.Columns.Add("SecurityID", typeof(int));
                        dataTable.Columns.Add("Symbol", typeof(string));
                        dataTable.Columns.Add("Timestamp", typeof(DateTime));
                        dataTable.Columns.Add("OpenPrice", typeof(decimal));
                        dataTable.Columns.Add("HighPrice", typeof(decimal));
                        dataTable.Columns.Add("LowPrice", typeof(decimal));
                        dataTable.Columns.Add("ClosePrice", typeof(decimal));
                        if (timeInterval == "1d")
                        {
                            dataTable.Columns.Add("AdjustedClosePrice", typeof(decimal));
                            dataTable.Columns.Add("DividendAmount", typeof(decimal));
                            dataTable.Columns.Add("SplitCoefficient", typeof(decimal));
                        }
                        dataTable.Columns.Add("Volume", typeof(long));
                        dataTable.Columns.Add("TimeInterval", typeof(string));

                        if (timeInterval == "1d")
                        {
                            foreach (var price in prices)
                            {
                                dataTable.Rows.Add(
                                    securityId,
                                    symbol,
                                    price.Timestamp,
                                    price.OpenPrice,
                                    price.HighPrice,
                                    price.LowPrice,
                                    price.ClosePrice,
                                    price.AdjustedClosePrice,
                                    price.DividendAmount,
                                    price.SplitCoefficient,
                                    price.Volume,
                                    timeInterval
                                );
                            }
                        }
                        else
                        {
                            foreach (var price in prices)
                            {
                                dataTable.Rows.Add(
                                    securityId,
                                    symbol,
                                    price.Timestamp,
                                    price.OpenPrice,
                                    price.HighPrice,
                                    price.LowPrice,
                                    price.ClosePrice,
                                    price.Volume,
                                    timeInterval
                                );
                            }
                        }
                        await bulkCopy.WriteToServerAsync(dataTable);

                        Console.WriteLine($"Successfully bulk inserted {prices.Count} records into {tableName}.");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error inserting into {tableName}: {ex.Message}");
                    return false;
                }
            }
        }

        public async Task<List<PriceModel>> GetAllHistoricalPrices(string symbol, string interval, DateTime? startDate, DateTime? endDate)
        {
            var data = new List<PriceModel>();
            using (var connection = new SqlConnection(_historicalPriceConnectionString))
            {
                await connection.OpenAsync();
                var tableName = _sqlHelper.SanitizeTableName(symbol, interval);
                await _sqlHelper.EnsureTableExists(tableName);
                var query = $"SELECT * FROM {tableName} " +
                    "WHERE Timestamp >= @StartDate AND Timestamp <= @EndDate";
                data.AddRange(await connection.QueryAsync<PriceModel>(query, new
                {
                    StartDate = startDate,
                    EndDate = endDate
                }));
                return data.ToList();
            }
        }
        public async Task<List<PriceModel>> GetAllHistoricalPrices(List<string> symbols, List<string> intervals)
        {
            var data = new List<PriceModel>();

            using (var connection = new SqlConnection(_historicalPriceConnectionString))
            {
                await connection.OpenAsync();

                foreach (var symbol in symbols)
                {
                    foreach (var interval in intervals)
                    {
                        var tableName = _sqlHelper.SanitizeTableName(symbol, interval);
                        await _sqlHelper.EnsureTableExists(tableName);
                        var query = $"SELECT * FROM {tableName}";
                        data.AddRange(await connection.QueryAsync<PriceModel>(query));
                    }
                }
                return data.ToList();
            }
        }

        public async Task<bool> CheckIfBackfillExists(string symbol, string interval)
        {
            using (var connection = new SqlConnection(_historicalPriceConnectionString))
            {
                await connection.OpenAsync();
                var tableName = _sqlHelper.SanitizeTableName(symbol, interval);
                await _sqlHelper.EnsureTableExists(tableName);

                var query = $"SELECT " +
                    $"CASE " +
                    $"  WHEN (SELECT COUNT(DISTINCT YEAR([Timestamp]))" +
                    $"          FROM {tableName}) > 20" +
                    $"  THEN CAST(1 AS BIT)" +
                    $"  ELSE CAST(0 AS BIT)" +
                    $"END AS Result;";

                var data = await connection.QueryFirstOrDefaultAsync<bool>(query);
                return data;
            }
        }

        public async Task<bool> DeleteHistoricalPrices(string symbol, string interval)
        {
            var tableName = _sqlHelper.SanitizeTableName(symbol, interval);

            using (var connection = new SqlConnection(_historicalPriceConnectionString))
            {
                await connection.OpenAsync();
                var checkIfTableExists = await _sqlHelper.TableExists(tableName);
                if (!checkIfTableExists)
                    return true;

                var query = $"DROP TABLE {tableName}";
                return await connection.ExecuteAsync(query).ContinueWith(t => t.Result > 0);
            }
        }

        public async Task<List<BackfillStatus>> CheckBackFillQueue()
        {
            Console.WriteLine($"Connection String in backfill queue: {_historicalPriceConnectionString}");
            var result = new List<BackfillStatus>();
            using (var connection = new SqlConnection(_historicalPriceConnectionString))
            {
                await connection.OpenAsync();
                var query = @"
                    SELECT 
                        [Symbol], 
                        [Type],
                        [Interval], 
                        [Status], 
                        [StartTime], 
                        [EndTime]
                    FROM [Historical].[BackFill].[SecurityQueue]
                    WHERE Status IS NULL OR Status = 'Running'
                    ORDER BY [Type];
                ";

                var data = await connection.QueryAsync<SecurityQueueModel>(query);
                result = data.ToList();

                return result;
            }
        }

        public async Task<bool> UpdateBackFillStatus(SecurityQueueModel backFillStatus)
        {
            var result = 0;
            using (var connection = new SqlConnection(_historicalPriceConnectionString))
            {
                await connection.OpenAsync();
                var query = "UPDATE [Historical].[BackFill].[SecurityQueue] " +
                    "SET Status = @Status, StartTime = @StartDate, EndTime = @EndTime " +
                    "WHERE Symbol = @Symbol AND Interval = @Interval";

                result = await connection.ExecuteAsync(query, new
                {
                    Symbol = backFillStatus.Symbol,
                    Interval = backFillStatus.Interval,
                    Status = backFillStatus.Status.ToString(),
                    StartDate = backFillStatus.StartTime.HasValue ? backFillStatus.StartTime.Value : (DateTime?)null,
                    EndTime = backFillStatus.Status == SecurityQueueStatus.Completed ? DateTime.UtcNow : (DateTime?)null,
                });

                return result > 0;
            }
        }

        public async Task<bool> CompleteBackFillStatus(SecurityQueueModel backFillStatus, string symbol, string interval)
        {
            using (var connection = new SqlConnection(_historicalPriceConnectionString))
            {
                await connection.OpenAsync();
                var query = "UPDATE [Historical].[BackFill].[SecurityQueue] " +
                        "SET Status = @Status, EndTime = @EndTime " +
                        "WHERE Symbol = @Symbol AND Interval = @Interval";
                var result = await connection.ExecuteAsync(query, new
                {
                    Symbol = symbol,
                    Interval = interval,
                    Status = backFillStatus.Status.ToString(),
                    EndTime = backFillStatus.Status == SecurityQueueStatus.Completed ? DateTime.UtcNow : (DateTime?)null,
                });

                return result > 0;
            }
        }

        public async Task<bool> UpdateAllBackFillStatus(SecurityQueueStatus status)
        {
            var result = 0;
            using (var connection = new SqlConnection(_historicalPriceConnectionString))
            {
                await connection.OpenAsync();
                var query = "UPDATE [Historical].[BackFill].[SecurityQueue] " +
                            "SET Status = @Status, StartTime = @StartDate, EndTime = @EndTime " +
                            "WHERE Status IS NULL OR Status = 'Running'";

                result = await connection.ExecuteAsync(query, new
                {
                    Status = status.ToString(),
                    StartDate = status == SecurityQueueStatus.Running ? DateTime.UtcNow : (DateTime?)null,
                    EndTime = status == SecurityQueueStatus.Completed ? (DateTime?)null : DateTime.UtcNow,
                });

                return result > 0;
            }
        }

        

        private async Task<bool> CheckCoverageExistsAsync(string symbol, string interval, string type, int year, int month)
        {
            await using (var connection = new SqlConnection(_historicalPriceConnectionString))
            {
                await connection.OpenAsync();
                var query = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM [Historical].[BackFill].[SecurityDataCoverage]
                    WHERE Symbol = @Symbol
                      AND Interval = @Interval
                      AND Type = @Type
                      AND Year = @Year
                      AND Month = @Month
                ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END";

                return await connection.QuerySingleAsync<bool>(query, new { Symbol = symbol, Interval = interval, Type = type, Year = year, Month = month });
            }
        }
    }
}
