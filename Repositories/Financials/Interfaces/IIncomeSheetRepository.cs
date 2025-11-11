using Temperance.Ephemeris.Models.Financials;

namespace Temperance.Ephemeris.Repositories.Financials.Interfaces
{
    public interface IIncomeSheetRepository
    {
        Task<bool> UpdateSecuritiesIncomeStatement(string symbol, IncomeSheetResponse incomeSheet);
    }
}
