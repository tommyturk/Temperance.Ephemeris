namespace Temperance.Ephemeris.Models.Financials
{
    public class IncomeSheetResponse
    {
        public List<IncomeSheetModel> AnnualReports { get; set; }
        public List<IncomeSheetModel> QuarterlyReports { get; set; }
    }
}
