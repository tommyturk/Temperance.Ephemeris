using Temperance.Ephemeris.Models.BackFill;
using Temperance.Ephemeris.Models.Prices;

namespace Temperance.Ephemeris.Repositories.Prices.Interfaces
{
    public interface IHistoricalPriceRepository
    {
        Task<List<PriceModel>> GetAllHistoricalPrices(string symbol, string interval, DateTime? startDate, DateTime? endDate);
        Task<bool> SecurityDataCheck(string symbol, string interval, DateTime startDate);
        Task<List<PriceModel>> GetMonthlyDataCoverage(string symbol, string interval, DateTime startDate, DateTime endDate);
        Task<bool> DoesDailyDataExistAsync(string symbol);
        Task<IEnumerable<SecurityDataCoverageModel>> GetMonthlyDataCoverageAsync(string symbol, string interval);
    }
}
