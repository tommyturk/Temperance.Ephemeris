using Temperance.Ephemeris.Models.Prices;

namespace Temperance.Ephemeris.Services.Prices
{
    public interface IPriceService
    {
        Task<List<PriceModel>> GetAllHistoricalPrices(string symbol, string interval, DateTime? startDate, DateTime? endDate);
    }
}
