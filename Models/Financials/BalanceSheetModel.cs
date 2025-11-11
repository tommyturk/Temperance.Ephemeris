namespace Temperance.Ephemeris.Models.Financials
{
    public class BalanceSheetModel
    {
        public string Symbol { get; set; }
        public List<BalanceSheetAnnualModel> AnnualReports { get; set; }
        public List<BalanceSheetQuarterlyModel> QuarterlyReports { get; set; }
    }
}
