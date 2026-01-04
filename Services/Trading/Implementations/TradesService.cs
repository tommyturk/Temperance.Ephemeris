using Temperance.Ephemeris.Models.Trading;
using Temperance.Ephemeris.Repository.Trading.Interfaces;
using Temperance.Ephemeris.Services.Trading.Interfaces;

namespace Temperance.Ephemeris.Services.Trading.Implementations
{
    public class TradesService : ITradesService
    {
        private readonly ITradeRepository _tradeRepository;
        public TradesService(ITradeRepository tradeRepository)
        {
            _tradeRepository = tradeRepository;
        }

        public async Task<int> ExecuteOrderAsync(Order order)
        {
            return await _tradeRepository.ExecuteOrderAsync(order);
        }

        public Task<int> LogStrategyAsync(StrategyLog log)
        {
            return _tradeRepository.LogStrategyAsync(log);
        }

        public async Task<int> SaveTradeAsync(Trade trade)
        {
            return await _tradeRepository.SaveTradeAsync(trade);
        }

        public async Task<int> UpdatePositionAsync(Position position)
        {
            return await _tradeRepository.UpdatePositionAsync(position);
        }

        public async Task CheckTradeExitsAsync()
        {
            await _tradeRepository.CheckTradeExitsAsync();
        }
    }
}
