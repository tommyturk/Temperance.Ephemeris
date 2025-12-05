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
        Task<IEnumerable<SecurityDataCoverageModel>> GetMonthlyDataCoverageAsync(string symbol, string interval, DateTime? startDate, DateTime? endDate);
        Task<bool> UpdateHistoricalIntradayPrices(List<PriceModel> prices, string symbol, string timeInterval);
        Task<bool> UpdateHistoricalDailyPrices(List<PriceModel> prices, string symbol);
        Task<DateTime?> GetFirstDataPointDate(string symbol, string interval);


        //// This method assumes the historical data structure is correctly mapped to your 
        //// [Prices].[StockSymbol]_[Interval] SQL tables.
        //Task<decimal> GetLatestPriceAsync(string symbol, DateTime backtestTimestamp);

        //// This method can retrieve data for multiple symbols/intervals, useful for portfolios.
        //Task<List<PriceModel>> GetAllHistoricalPrices(List<string> symbols, List<string> intervals);

        //// Fetch prices for a specific time window.
        //Task<List<PriceModel>> GetHistoricalPricesForMonth(string symbol, string timeInterval, DateTime startDate, DateTime endDate);

        //// Used by Delphi to persist new data.
        //Task<bool> UpdateHistoricalPrices(List<PriceModel> prices, string symbol, string timeInterval);

        //// Fetches all historical prices for a single security/interval pair. (Redundant with GetHistoricalPrices, kept for compatibility)
        //Task<List<PriceModel>> GetSecurityHistoricalPrices(string symbol, string interval);

        //// Clean implementation of fetching data.
        //Task<List<PriceModel>> GetHistoricalPrices(string symbol, string interval);

        //// Used for backfill logic in Delphi.
        //Task<bool> CheckIfBackfillExists(string symbol, string interval);

        //// Used to determine where to start the next backfill from.
        //Task<DateTime?> GetMostRecentTimestamp(string symbol, string interval);

        //// Maintenance method.
        //Task<bool> DeleteHistoricalPrices(string symbol, string interval);

        //// Logic for backfill queueing/status management.
        //Task<bool> UpdateBackFillQueue(string symbol);
    }
}
