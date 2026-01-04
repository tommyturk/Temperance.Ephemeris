using Dapper;
using Microsoft.Data.SqlClient;
using Temperance.Ephemeris.Models.Financials;
using Temperance.Ephemeris.Repositories.Financials.Interfaces;

namespace Temperance.Ephemeris.Repositories.Financials.Implementations
{
    public class SecurityMasterRepository : ISecurityMasterRepsotory
    {
        private readonly string _connectionString;
        
        public SecurityMasterRepository(string connectionString)
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

        public async Task<List<SecurityMaster>> GetAll()
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $@"SELECT * FROM [TradingBotDb].[Financials].[SecuritiesMaster]";
            var result = await connection.QueryAsync<SecurityMaster>(query);
            return result.ToList();
        }
    }
}
 