using Temperance.Ephemeris.Models.Indicators;

namespace Temperance.Ephemeris.Repositories.Financials.Interfaces
{
    public interface IIndicatorRepository
    {
        Task<List<IndicatorBackFillStatus>> CheckMarketIndicatorBackfillQueue();
        Task InsertIndicatorQueueItem(IndicatorBackFillStatus indicator);
        Task UpdateMarketIndicatorBackFillStatus(int indicatorId, string status);
        Task<bool> FederalFundsRateBackFill(FederalFundInterestRateModel interestModel);
        Task<bool> InflationBackfill(InflationModel inflationData);
        Task<bool> RealGdpBackfill(RealGdpModel realGdpData);
        Task<bool> RealGdpPerCapitaBackFill(RealGdpModel realGdpPerCapitaData);
        Task<bool> TreasuryYieldsBackFill(TreasuryYieldsModel treasuryYieldsData);
        Task<bool> UnemploymentRateBackfill(UnemploymentRatesModel unemploymentRateData);
    }
}
