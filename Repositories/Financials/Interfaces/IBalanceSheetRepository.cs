using Temperance.Ephemeris.Models.Financials;

namespace Temperance.Ephemeris.Repositories.Financials.Interfaces
{
    public interface IBalanceSheetRepository
    {
        Task<BalanceSheetModel> GetSecurityBalanceSheet(int securityId);

        Task<bool> InsertSecuritiesBalanceSheetData(int securityId, BalanceSheetModel balanceSheetData, string symbol);
    }
}
