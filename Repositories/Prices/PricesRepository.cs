using Dapper;
using Microsoft.Data.SqlClient;
using Temperance.Ephemeris.Models.Prices;
using Temperance.Ephemeris.Utilities.Helpers;

namespace Temperance.Ephemeris.Repositories.Prices
{
    public class PricesRepository : IPricesRepository
    {
        private readonly string _historicalConnectionString;
        private readonly ISqlHelper _sqlHelper;

        public PricesRepository(string historicalConnectionString, ISqlHelper sqlHelper)
        {
            _historicalConnectionString = historicalConnectionString;
            _sqlHelper = sqlHelper;
        }

        public async Task<List<PriceModel>> GetAllHistoricalPrices(string symbol, string interval, DateTime? startDate, DateTime? endDate)
        {
            var data = new List<PriceModel>();
            using (var connection = new SqlConnection(_historicalConnectionString))
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

        public async Task<bool> SecurityDataCheck(string symbol, string interval, DateTime startDate)
        {
            var tableName = _sqlHelper.SanitizeTableName(symbol, interval);

            await _sqlHelper.EnsureTableExists(tableName);

            using var connection = new SqlConnection(_historicalConnectionString);

            var query = $"SELECT COUNT(1) FROM {tableName}" +
                $"WHERE GETDATE(Timestamp) == @StartDate";
            var parameters = new { StartDate = startDate };
            var count = await connection.ExecuteScalarAsync<int>(query);
            return count > 0;
        }
    }
}