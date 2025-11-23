using Temperance.Ephemeris.Models.Prices;

namespace Temperance.Ephemeris.Services.Prices.Interfaces
{
    public interface IHistoricalPriceService
    {
        Task<List<PriceModel>> GetAllHistoricalPrices(string symbol, string interval, DateTime? startDate, DateTime? endDate);
        Task<bool> SecurityDataCheck(string symbol, string interval, DateTime startDate);
        Task<List<PriceModel>> GetMonthlyDataCoverage(string symbol, string interval, DateTime startDate, DateTime endDate);
    }
}
