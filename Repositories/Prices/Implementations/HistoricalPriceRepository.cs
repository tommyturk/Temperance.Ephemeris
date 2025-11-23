using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using Temperance.Ephemeris.Models.BackFill;
using Temperance.Ephemeris.Models.Prices;
using Temperance.Ephemeris.Repositories.Financials.Interfaces;
using Temperance.Ephemeris.Repositories.Prices.Interfaces;
using Temperance.Ephemeris.Utilities.Helpers;

namespace Temperance.Ephemeris.Repositories.Prices.Implementations
{
    public class HistoricalPriceRepository : IHistoricalPriceRepository
    {
        private readonly string _historicalPriceConnectionString;
        private readonly ISqlHelper _sqlHelper;
        private readonly ISecurityOverviewRepository _securitiesOverviewRepository;
        public HistoricalPriceRepository(string historicalPriceConnectionString, ISqlHelper sqlHelper, ISecurityOverviewRepository securityOverviewRepository)
        {
            _historicalPriceConnectionString = historicalPriceConnectionString;
            _sqlHelper = sqlHelper;
            _securitiesOverviewRepository = securityOverviewRepository;
        }

        public async Task<List<PriceModel>> GetAllHistoricalPrices(string symbol, string interval, DateTime? startDate, DateTime? endDate)
        {
            var data = new List<PriceModel>();
            using (var connection = new SqlConnection(_historicalPriceConnectionString))
            {
                await connection.OpenAsync();
                var tableName = _sqlHelper.SanitizeTableName(symbol, interval);
                await _sqlHelper.EnsureTableExists(tableName);
                var query = @$"SELECT * FROM {tableName}
                    WHERE Timestamp >= @StartDate AND Timestamp <= @EndDate";
                data.AddRange(await connection.QueryAsync<PriceModel>(query, new
                {
                    StartDate = startDate,
                    EndDate = endDate
                }));
                return data.ToList();
            }
        }

        public async Task<bool> DoesDailyDataExistAsync(string symbol)
        {
            var tableName = $"[Prices].[{symbol}_1d]";
            var sql = $"SELECT TOP 1 1 FROM {tableName}";
            using var connection = new SqlConnection(_historicalPriceConnectionString);
            await connection.OpenAsync();
            var result = await connection.QuerySingleOrDefaultAsync<int?>(sql);
            return result.HasValue;
        }

        public async Task<bool> SecurityDataCheck(string symbol, string interval, DateTime startDate)
        {
            var tableName = _sqlHelper.SanitizeTableName(symbol, interval);

            await _sqlHelper.EnsureTableExists(tableName);

            using var connection = new SqlConnection(_historicalPriceConnectionString);

            var query = @$"SELECT COUNT(1) FROM {tableName}
                WHERE GETDATE(Timestamp) == @StartDate";
            var parameters = new { StartDate = startDate };
            var count = await connection.ExecuteScalarAsync<int>(query);
            return count > 0;
        }

        public async Task<List<PriceModel>> GetMonthlyDataCoverage(string symbol, string interval, DateTime startDate, DateTime endDate)
        {
            var data = new List<PriceModel>();
            using var connection = new SqlConnection(_historicalPriceConnectionString);

            await connection.OpenAsync();
            var tableName = _sqlHelper.SanitizeTableName(symbol, interval);
            await _sqlHelper.EnsureTableExists(tableName);
            var query = @$"SELECT DISTINCT 
                    YEAR(Timestamp) AS Year,
                    MONTH(Timestamp) AS Month
                FROM {tableName}
                WHERE Timestamp >= @StartDate AND Timestamp <= @EndDate
                ORDER BY Year, Month;";

            data.AddRange(await connection.QueryAsync<PriceModel>(query, new
            {
                StartDate = startDate,
                EndDate = endDate
            }));
            return data.ToList();
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

                var batchSuccess = await InsertBatchPriceRecords(batch, timeInterval);
                success &= batchSuccess;

                if (!batchSuccess)
                {
                    Console.WriteLine($"Batch {i / batchSize + 1} failed for symbol {symbol}.");
                }
            }

            Console.WriteLine($"Update process for symbol {symbol} completed at {DateTime.UtcNow}. Success: {success}");
            return success;
        }

        public async Task<bool> UpdateHistoricalDailyPrices(List<PriceModel> prices, string symbol)
        {
            var success = true;
            const int batchSize = 5000;
            Console.WriteLine($"Starting update process for symbol {symbol} with {prices.Count} prices at {DateTime.UtcNow}");
            for (int i = 0; i < prices.Count; i += batchSize)
            {
                var batch = prices.Skip(i).Take(batchSize).ToList();
                Console.WriteLine($"Processing batch {i / batchSize + 1} with {batch.Count} records for symbol {symbol}.");
                var batchSuccess = await InsertBatchPriceRecords(batch, "1d");
                success &= batchSuccess;
                if (!batchSuccess)
                {
                    Console.WriteLine($"Batch {i / batchSize + 1} failed for symbol {symbol}.");
                }
            }
            Console.WriteLine($"Update process for symbol {symbol} completed at {DateTime.UtcNow}. Success: {success}");
            return success;
        }

        public async Task<IEnumerable<SecurityDataCoverageModel>> GetMonthlyDataCoverageAsync(string symbol, string interval, DateTime? startDate, DateTime? endDate)
        {
            using (var connection = new SqlConnection(_historicalPriceConnectionString))
            {
                await connection.OpenAsync();

                var tableName = _sqlHelper.SanitizeTableName(symbol, interval);

                await _sqlHelper.EnsureTableExists(tableName);

                var sql = $@"
                        SELECT
                            Symbol,
                            Interval,
                            Type,
                            YEAR([Timestamp]) AS [Year],
                            MONTH([Timestamp]) AS [Month]
                        FROM {tableName}";
                sql += startDate.HasValue ? " WHERE [Timestamp] >= @StartDate" : "";
                sql += endDate.HasValue ? (startDate.HasValue ? " AND " : " WHERE ") + " [Timestamp] <= @EndDate" : "";
                sql += @"
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

        private async Task<bool> InsertBatchPriceRecords(List<PriceModel> prices, string timeInterval)
        {
            if (!prices.Any())
                return false;

            var symbol = prices.First().Symbol;
            var tableName = _sqlHelper.SanitizeTableName(symbol, timeInterval);

            await _sqlHelper.EnsureTableExists(tableName);

            using var connection = new SqlConnection(_historicalPriceConnectionString);
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
                    foreach (var price in prices)
                        dataTable.Rows.Add(
                            price.SecurityID,
                            price.Symbol,
                            price.Timestamp,
                            price.OpenPrice,
                            price.HighPrice,
                            price.LowPrice,
                            price.ClosePrice,
                            price.AdjustedClosePrice,
                            price.DividendAmount,
                            price.SplitCoefficient,
                            price.Volume,
                            price.TimeInterval
                        );
                else
                    foreach (var price in prices)
                        dataTable.Rows.Add(
                            price.SecurityID,
                            symbol,
                            price.Timestamp,
                            price.OpenPrice,
                            price.HighPrice,
                            price.LowPrice,
                            price.ClosePrice,
                            price.Volume,
                            price.TimeInterval
                        );
                
                await bulkCopy.WriteToServerAsync(dataTable);

                Console.WriteLine($"Successfully bulk inserted {prices.Count} records into {tableName}.");
                return true;
            }
        }
    }
}
