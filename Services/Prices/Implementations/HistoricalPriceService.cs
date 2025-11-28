using Microsoft.Data.SqlClient;
using Temperance.Ephemeris.Models.Prices;
using Temperance.Ephemeris.Repositories.Prices.Interfaces;
using Temperance.Ephemeris.Services.Prices.Interfaces;

namespace Temperance.Ephemeris.Services.Prices.Implementations
{
    public class HistoricalPriceService : IHistoricalPriceService
    {
        private readonly IHistoricalPriceRepository _historicalPriceRepository;

        public HistoricalPriceService(IHistoricalPriceRepository pricesRepository)
        {
            _historicalPriceRepository = pricesRepository;
        }

        public async Task<bool> SecurityDataCheck(string symbol, string interval, DateTime startDate)
        {
            return await _historicalPriceRepository.SecurityDataCheck(symbol, interval, startDate);
        }

        public async Task<List<PriceModel>> GetAllHistoricalPrices(string symbol, string interval, DateTime? startDate, DateTime? endDate)
        {
            return await _historicalPriceRepository.GetAllHistoricalPrices(symbol, interval, startDate, endDate);
        }

        public async Task<List<PriceModel>> GetMonthlyDataCoverage(string symbol, string interval, DateTime startDate, DateTime endDate)
        {
            return await _historicalPriceRepository.GetMonthlyDataCoverage(symbol, interval, startDate, endDate);
        }

        public async Task<DateTime?> GetFirstDataPointDate(string symbol, string interval)
        {
            return await _historicalPriceRepository.GetFirstDataPointDate(symbol, interval);
        }
    }
}
