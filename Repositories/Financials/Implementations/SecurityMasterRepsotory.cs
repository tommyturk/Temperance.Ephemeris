using Dapper;
using Microsoft.Data.SqlClient;
using Temperance.Ephemeris.Repositories.Financials.Interfaces;

namespace Temperance.Ephemeris.Repositories.Financials.Implementations
{
    public class SecurityMasterRepsotory : ISecurityMasterRepsotory
    {
        private readonly string _connectionString;
        
        public SecurityMasterRepsotory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<DateTime?> GetSecurityIpoDate(string symbol)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $@"SELECT IpoDate FROM [TradingBotDb].[Financials].[SecuritiesMaster]
                WHERE Symbol == @Symbol";
            return await connection.ExecuteScalarAsync<DateTime?>(query);
        }
    }
}
 