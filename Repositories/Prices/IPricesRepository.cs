using Temperance.Ephemeris.Models.Prices;

namespace Temperance.Ephemeris.Repositories.Prices
{
    public interface IPricesRepository
    {
        Task<List<PriceModel>> GetAllHistoricalPrices(string symbols, string interval, DateTime? startDate, DateTime? endDate);
        Task<bool> SecurityDataCheck(string symbol, string interval, DateTime startDate);
    }
}