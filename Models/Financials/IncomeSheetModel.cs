using Newtonsoft.Json;

namespace Temperance.Ephemeris.Models.Financials
{
    public class IncomeSheetModel
    {
        [JsonProperty("fiscalDateEnding")]
        public string FiscalDateEnding { get; set; }

        [JsonProperty("reportedCurrency")]
        public string ReportedCurrency { get; set; }

        [JsonProperty("grossProfit")]
        public long? GrossProfit { get; set; }

        [JsonProperty("totalRevenue")]
        public long? TotalRevenue { get; set; }

        [JsonProperty("costOfRevenue")]
        public long? CostOfRevenue { get; set; }

        [JsonProperty("costofGoodsAndServicesSold")]
        public long? CostofGoodsAndServicesSold { get; set; }

        [JsonProperty("operatingIncome")]
        public long? OperatingIncome { get; set; }

        [JsonProperty("sellingGeneralAndAdministrative")]
        public long? SellingGeneralAndAdministrative { get; set; }

        [JsonProperty("researchAndDevelopment")]
        public long? ResearchAndDevelopment { get; set; }

        [JsonProperty("operatingExpenses")]
        public long? OperatingExpenses { get; set; }

        [JsonProperty("investmentIncomeNet")]
        public long? InvestmentIncomeNet { get; set; }

        [JsonProperty("netInterestIncome")]
        public long? NetInterestIncome { get; set; }

        [JsonProperty("interestIncome")]
        public long? InterestIncome { get; set; }

        [JsonProperty("interestExpense")]
        public long? InterestExpense { get; set; }

        [JsonProperty("nonInterestIncome")]
        public long? NonInterestIncome { get; set; }

        [JsonProperty("otherNonOperatingIncome")]
        public long? OtherNonOperatingIncome { get; set; }

        [JsonProperty("depreciation")]
        public long? Depreciation { get; set; }

        [JsonProperty("depreciationAndAmortization")]
        public long? DepreciationAndAmortization { get; set; }

        [JsonProperty("incomeBeforeTax")]
        public long? IncomeBeforeTax { get; set; }

        [JsonProperty("incomeTaxExpense")]
        public long? IncomeTaxExpense { get; set; }

        [JsonProperty("interestAndDebtExpense")]
        public long? InterestAndDebtExpense { get; set; }

        [JsonProperty("netIncomeFromContinuingOperations")]
        public long? NetIncomeFromContinuingOperations { get; set; }

        [JsonProperty("comprehensiveIncomeNetOfTax")]
        public long? ComprehensiveIncomeNetOfTax { get; set; }

        [JsonProperty("ebit")]
        public long? Ebit { get; set; }

        [JsonProperty("ebitda")]
        public long? Ebitda { get; set; }

        [JsonProperty("netIncome")]
        public long? NetIncome { get; set; }
    }
}
