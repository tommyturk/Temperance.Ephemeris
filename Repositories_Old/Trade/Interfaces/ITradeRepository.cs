using Temperance.Delphi.Models.Trading;

namespace Temperance.Ephemeris.Repositories.Trade.Interfaces
{
    public interface ITradeRepository
    {
        Task<int> SaveTradeAsync(Models.Trading.Trade trade);
        Task<int> ExecuteOrderAsync(Order order);
        Task<int> UpdatePositionAsync(Position position);
        Task<int> LogStrategyAsync(StrategyLog log);
        Task CheckTradeExitsAsync();
    }
}
