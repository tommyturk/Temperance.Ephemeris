using Dapper;
using Microsoft.Data.SqlClient;
using Temperance.Ephemeris.Models.Constellations;
using Temperance.Ephemeris.Repositories.Constellations.Interfaces;
using Temperance.Ephemeris.Utilities.Helpers;

namespace Temperance.Ephemeris.Repositories.Constellations.Implementations
{
    public class WalkForwardSessionRepository : IWalkForwardSessionRepository
    {
        private readonly string _connectionString;
        private readonly ISqlHelper _sqlHelper;
        
        public WalkForwardSessionRepository(string connectionString, ISqlHelper sqlHelper)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _sqlHelper = sqlHelper ?? throw new ArgumentNullException(nameof(sqlHelper));
        }

        public async Task CreateAsync(WalkForwardSessionModel session)
        {
            const string sql = @"
            INSERT INTO [Constellations].[WalkForwardSessions] 
                (SessionId, StrategyName, Status, StartDate, EndDate, InitialCapital, CurrentCapital, CreatedAt,
                    OptimizationWindowYears, TradingWindowYears, Interval)
            VALUES 
                (@SessionId, @StrategyName, @Status, @StartDate, @EndDate, @InitialCapital, @CurrentCapital, @CreatedAt,
                    @OptimizationWindowYears, @TradingWindowYears, @Interval);";
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, session);
        }

        public async Task<WalkForwardSessionModel> GetSessionAsync(Guid sessionId)
        {
            const string sql = "SELECT * FROM [Constellations].[WalkForwardSessions] WHERE SessionId = @SessionId;";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<WalkForwardSessionModel>(sql, new { SessionId = sessionId });
        }

        public async Task<List<WalkForwardSessionModel>> GetAllSessions()
        {
            const string sql = @$"SELECT * FROM [Constellations].[WalkForwardSessions];
                ORDER BY CreatedAt DESC";
            using var connection = new SqlConnection(_connectionString);
            var sessions = await connection.QueryAsync<WalkForwardSessionModel>(sql);
            return sessions.ToList();
        }

        public async Task UpdateSessionCapitalAsync(Guid sessionId, double? finalCapital)
        {
            const string sql = @"
                UPDATE [Constellations].[WalkForwardSessions]
                SET CurrentCapital = @FinalCapital
                WHERE SessionId = @SessionId;";

            await using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, new { FinalCapital = finalCapital, SessionId = sessionId });
            return;
        }

        public async Task UpdateSessionStatusAsync(Guid sessionId, string status)
        {
            const string sql = "UPDATE [Constellations].[WalkForwardSessions] SET Status = @Status WHERE SessionId = @SessionId;";
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, new { Status = status, SessionId = sessionId });
        }
    }
}
