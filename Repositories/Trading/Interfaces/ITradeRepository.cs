using Temperance.Ephemeris.Models.Trading;

namespace Temperance.Ephemeris.Repository.Trading.Interfaces
{
    public interface ITradeRepository
    {
        Task<int> SaveTradeAsync(Trade trade);
        Task<int> ExecuteOrderAsync(Order order);
        Task<int> UpdatePositionAsync(Position position);
        Task<int> LogStrategyAsync(StrategyLog log);
        Task CheckTradeExitsAsync();
    }
}
