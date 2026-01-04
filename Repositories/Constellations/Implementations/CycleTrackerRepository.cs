using Dapper;
using Microsoft.Data.SqlClient;
using Temperance.Ephemeris.Models.Constellations;
using Temperance.Ephemeris.Repositories.Constellations.Interfaces;
using Temperance.Ephemeris.Utilities.Helpers;

namespace Temperance.Ephemeris.Repositories.Constellations.Implementations
{
    public class CycleTrackerRepository : ICycleTrackerRepository
    {
        private readonly string _connectionString;
        private readonly ISqlHelper _sqlHelper;
        public CycleTrackerRepository(string connectionString, ISqlHelper sqlHelper)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _sqlHelper = sqlHelper ?? throw new ArgumentNullException(nameof(sqlHelper));
        }

        public async Task CreateCycle(CycleTrackerModel cycle) 
        {             
            const string sql = @"
            INSERT INTO [Constellations].[CycleTracker] 
                (CycleId, StrategyName, StartDate, EndDate, Status, CreatedAt)
            VALUES 
                (@CycleId, @StrategyName, @StartDate, @EndDate, @Status, @CreatedAt);";
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, cycle);
        }
    }
}
