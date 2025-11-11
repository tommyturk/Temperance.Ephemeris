using Temperance.Ephemeris.Models.Prices;
using Temperance.Ephemeris.Repositories.Prices;

namespace Temperance.Ephemeris.Services.Prices
{
    public class PriceService : IPriceService
    {
        private readonly IPricesRepository _pricesRepository;


        public PriceService(IPricesRepository pricesRepository)
        {
            _pricesRepository = pricesRepository;
        }

        public async Task<bool> SecurityDataCheck(string symbol, string interval, DateTime startDate)
        {
            return await _pricesRepository.SecurityDataCheck(symbol, interval, startDate);
        }

        public async Task<List<PriceModel>> GetAllHistoricalPrices(string symbol, string interval, DateTime? startDate, DateTime? endDate)
        {
            return await _pricesRepository.GetAllHistoricalPrices(symbol, interval, startDate, endDate);
        }
    }
}
