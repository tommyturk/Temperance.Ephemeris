using Temperance.Ephemeris.Models.Constellations;

namespace Temperance.Ephemeris.Repositories.Constellations.Interfaces
{
    public interface IWalkForwardSessionRepository
    {
        Task CreateAsync(WalkForwardSessionModel session);
        Task<WalkForwardSessionModel> GetSessionAsync(Guid sessionId);
        Task<List<WalkForwardSessionModel>> GetAllSessions();
        Task UpdateSessionStatusAsync(Guid sessionId, string status);
        Task UpdateSessionCapitalAsync(Guid sessionId, double? finalCapital);
    }
}
