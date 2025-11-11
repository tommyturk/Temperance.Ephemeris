using Dapper;
using Microsoft.Data.SqlClient;
using Temperance.Ephemeris.Repositories.BackFill.Interfaces;
using Temperance.Ephemeris.Utilities.Helpers;

namespace Temperance.Ephemeris.Repositories.BackFill.Implementations
{
    public class SecurityQueueRepository : ISecurityQueueRepository
    {
        private readonly string _historicalDbConnectionString;

        private readonly ISqlHelper _sqlHelper;

        public SecurityQueueRepository(string historicalDbConnectionString, ISqlHelper sqlHelper)
        {
            _historicalDbConnectionString = historicalDbConnectionString;
            _sqlHelper = sqlHelper;
        }

        public async Task<bool> SecurityDataCheck(string symbol, string interval, DateTime startDate)
        {
            var tableName = _sqlHelper.SanitizeTableName(symbol, interval);

            await _sqlHelper.EnsureTableExists(tableName);

            using var connection = new SqlConnection(_historicalDbConnectionString);

            var query = $"SELECT COUNT(1) FROM {tableName}" +
                $"WHERE GETDATE(Timestamp) == @StartDate";
            var parameters = new { StartDate = startDate };
            var count = await connection.ExecuteScalarAsync<int>(query);
            return count > 0;
        }
    }
}
