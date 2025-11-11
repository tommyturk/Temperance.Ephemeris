using Temperance.Ephemeris.Models.Financials;

namespace Temperance.Ephemeris.Repositories.Financials.Interfaces
{
    public interface IEarningsRepository
    {
        Task<bool> InsertSecuritiesEarningsData(int securityId, EarningsModel reports, string symbol);
        Task<bool> InsertAnnualEarnings(int securityId, List<AnnualEarningsModel> reports, string symbol);
        Task<bool> InsertQuarterlyEarnings(int securityId, List<QuarterlyEarningsModel> reports, string symbol);
        Task<EarningsModel> GetSecurityEarningsData(int securityId);
    }
}
