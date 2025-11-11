using Temperance.Delphi.Models.Securities.IncomeSheet;

namespace Temperance.Ephemeris.Repositories_Old.IncomeSheetRepository
{
    public interface IIncomeSheetRepository
    {
        Task<bool> UpdateSecuritiesIncomeStatement(string symbol, IncomeSheetResponse incomeSheet);
    }
}
