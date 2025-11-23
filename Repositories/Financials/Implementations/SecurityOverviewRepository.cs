using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Temperance.Ephemeris.Repositories.Financials.Interfaces;
using Temperance.Ephemeris.Utilities.Helpers;

namespace Temperance.Ephemeris.Repositories.Financials.Implementations
{
    public class SecurityOverviewRepository : ISecurityOverviewRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<SecurityOverviewRepository> _logger;
        private readonly ISqlHelper _sqlHelper;

        public SecurityOverviewRepository(ILogger<SecurityOverviewRepository> logger, ISqlHelper sqlHelper, string connectionString)
        {
            _logger = logger;
            _sqlHelper = sqlHelper;
            _connectionString = connectionString;
        }

        public async Task<int> GetSecurityId(string symbol)
        {
            if (symbol == null)
                return 0;
            using var connection = new SqlConnection(_connectionString);
            var query = "SELECT SecurityID FROM [TradingBotDb].[Financials].[Securities] WHERE Symbol = @Symbol";
            var securityId = await connection.ExecuteScalarAsync<int>(query, new { Symbol = symbol });

            if (securityId == 0)
            {
                var insertQuery = @"
                    INSERT INTO [TradingBotDb].[Financials].[Securities] (Symbol) 
                    VALUES (@Symbol); 
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";
                securityId = await connection.ExecuteScalarAsync<int>(insertQuery, new { Symbol = symbol });
            }

            return securityId;
        }
    }
}
