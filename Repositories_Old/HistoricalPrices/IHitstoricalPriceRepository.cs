using Temperance.Delphi.Models.Contracts;
using Temperance.Delphi.Models.HistoricalData;

namespace Temperance.Ephemeris.Repositories_Old.HistoricalPrices
{
    public interface IHistoricalPriceRepository
    {
        Task<decimal> GetLatestPriceAsync(string symbol, DateTime backtestTimestamp);
        Task<List<PriceModel>> GetAllHistoricalPrices(List<string> symbols, List<string> intervals);
        Task<List<PriceModel>> GetAllHistoricalPrices(string symbol, string interval, DateTime? startDate, DateTime? endDate);
        Task<List<PriceModel>> GetHistoricalPricesForMonth(string symbol, string interval, DateTime startDate, DateTime endDate);
        Task<bool> UpdateHistoricalIntradayPrices(List<PriceModel> prices, string symbol, string timeInterval);
        Task<bool> UpdateHistoricalDailyPrices(List<PriceModel> prices, string symbol);
        Task<List<PriceModel>> GetSecurityHistoricalPrices(string symbol, string interval);
        Task<List<PriceModel>> GetHistoricalPrices(string symbol, string interval);
        Task<bool> CheckIfBackfillExists(string symbol, string interval);
        Task<DateTime?> GetMostRecentTimestamp(string symbol, string interval);
        Task<bool> DeleteHistoricalPrices(string symbol, string interval);
        Task<List<BackfillStatus>> CheckBackFillQueue();
        Task<bool> UpdateBackFillStatus(BackfillStatus backFillStatus);
        Task<bool> CompleteBackFillStatus(BackfillStatus backFillStatus, string symbol, string interval);
        Task<bool> UpdateAllBackFillStatus(BackfillState status);
        Task<IEnumerable<SecurityDataCoverage>> GetMonthlyDataCoverageAsync(string symbol, string interval);
        Task UpdateSecurityDataCoverage(string symbol, string interval, string type, int year, int month, bool success);

        Task<bool> DoesDailyDataExistAsync(string symbol);
    }
}
