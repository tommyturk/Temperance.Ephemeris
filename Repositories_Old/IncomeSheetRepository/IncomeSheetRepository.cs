using Dapper;
using Microsoft.Data.SqlClient;
using Temperance.Delphi.Models.Securities.IncomeSheet;

namespace Temperance.Ephemeris.Repositories_Old.IncomeSheetRepository
{
    public class IncomeSheetRepository : IIncomeSheetRepository
    {
        private readonly string _connectionString;

        public IncomeSheetRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<bool> UpdateSecuritiesIncomeStatement(string symbol, IncomeSheetResponse incomeSheet)
        {
            if (incomeSheet.AnnualReports == null)
                return false;
            var annualSuccess = false;
            var quarterlySuccess = false;

            if(incomeSheet.QuarterlyReports != null)
                quarterlySuccess = await InsertQuarterlyIncomeSheetData(symbol, incomeSheet.QuarterlyReports);
            if(incomeSheet.AnnualReports != null)
                annualSuccess = await InsertAnnualIncomeSheetData(symbol, incomeSheet.AnnualReports);

            return annualSuccess || quarterlySuccess;
        }

        private async Task<bool> InsertQuarterlyIncomeSheetData(string symbol, List<IncomeSheetModel> quarterlyReports)
        {
            await using var connection = new SqlConnection(_connectionString);

            var sql = @"
            MERGE [Financials].[IncomeStatementQuarterly] AS target
            USING (
                SELECT
                    @Symbol AS Symbol, @FiscalDateEnding AS FiscalDateEnding,
                    @ReportedCurrency AS ReportedCurrency, @GrossProfit AS GrossProfit, @TotalRevenue AS TotalRevenue,
                    @CostOfRevenue AS CostOfRevenue, @CostofGoodsAndServicesSold AS CostofGoodsAndServicesSold,
                    @OperatingIncome AS OperatingIncome, @SellingGeneralAndAdministrative AS SellingGeneralAndAdministrative,
                    @ResearchAndDevelopment AS ResearchAndDevelopment, @OperatingExpenses AS OperatingExpenses,
                    @InvestmentIncomeNet AS InvestmentIncomeNet, @NetInterestIncome AS NetInterestIncome,
                    @InterestIncome AS InterestIncome, @InterestExpense AS InterestExpense, @NonInterestIncome AS NonInterestIncome,
                    @OtherNonOperatingIncome AS OtherNonOperatingIncome, @Depreciation AS Depreciation,
                    @DepreciationAndAmortization AS DepreciationAndAmortization, @IncomeBeforeTax AS IncomeBeforeTax,
                    @IncomeTaxExpense AS IncomeTaxExpense, @InterestAndDebtExpense AS InterestAndDebtExpense,
                    @NetIncomeFromContinuingOperations AS NetIncomeFromContinuingOperations,
                    @ComprehensiveIncomeNetOfTax AS ComprehensiveIncomeNetOfTax, @Ebit AS Ebit, @Ebitda AS Ebitda, @NetIncome AS NetIncome
            ) AS source
            ON (target.Symbol = source.Symbol AND target.FiscalDateEnding = source.FiscalDateEnding)
            WHEN NOT MATCHED THEN
                INSERT (
                    Symbol, FiscalDateEnding, ReportedCurrency, GrossProfit, TotalRevenue, CostOfRevenue,
                    CostofGoodsAndServicesSold, OperatingIncome, SellingGeneralAndAdministrative, ResearchAndDevelopment,
                    OperatingExpenses, InvestmentIncomeNet, NetInterestIncome, InterestIncome, InterestExpense,
                    NonInterestIncome, OtherNonOperatingIncome, Depreciation, DepreciationAndAmortization,
                    IncomeBeforeTax, IncomeTaxExpense, InterestAndDebtExpense, NetIncomeFromContinuingOperations,
                    ComprehensiveIncomeNetOfTax, Ebit, Ebitda, NetIncome
                )
                VALUES (
                    source.Symbol, source.FiscalDateEnding, source.ReportedCurrency, source.GrossProfit,
                    source.TotalRevenue, source.CostOfRevenue, source.CostofGoodsAndServicesSold, source.OperatingIncome,
                    source.SellingGeneralAndAdministrative, source.ResearchAndDevelopment, source.OperatingExpenses,
                    source.InvestmentIncomeNet, source.NetInterestIncome, source.InterestIncome, source.InterestExpense,
                    source.NonInterestIncome, source.OtherNonOperatingIncome, source.Depreciation, source.DepreciationAndAmortization,
                    source.IncomeBeforeTax, source.IncomeTaxExpense, source.InterestAndDebtExpense,
                    source.NetIncomeFromContinuingOperations, source.ComprehensiveIncomeNetOfTax, source.Ebit, source.Ebitda, source.NetIncome
                );";

            var affectedRows = 0;
            foreach (var report in quarterlyReports)
            {
                var parameters = new
                {
                    Symbol = symbol,
                    FiscalDateEnding = DateTime.Parse(report.FiscalDateEnding),
                    report.ReportedCurrency,
                    report.GrossProfit,
                    report.TotalRevenue,
                    report.CostOfRevenue,
                    report.CostofGoodsAndServicesSold,
                    report.OperatingIncome,
                    report.SellingGeneralAndAdministrative,
                    report.ResearchAndDevelopment,
                    report.OperatingExpenses,
                    report.InvestmentIncomeNet,
                    report.NetInterestIncome,
                    report.InterestIncome,
                    report.InterestExpense,
                    report.NonInterestIncome,
                    report.OtherNonOperatingIncome,
                    report.Depreciation,
                    report.DepreciationAndAmortization,
                    report.IncomeBeforeTax,
                    report.IncomeTaxExpense,
                    report.InterestAndDebtExpense,
                    report.NetIncomeFromContinuingOperations,
                    report.ComprehensiveIncomeNetOfTax,
                    report.Ebit,
                    report.Ebitda,
                    report.NetIncome
                };
                affectedRows += await connection.ExecuteAsync(sql, parameters);
            }

            return affectedRows > 0;
        }

        private async Task<bool> InsertAnnualIncomeSheetData(string symbol, List<IncomeSheetModel> annualReports)
        {
            await using var connection = new SqlConnection(_connectionString);

            var sql = @"
            MERGE [Financials].[IncomeStatementAnnual] AS target
            USING (
                SELECT
                    @Symbol AS Symbol, @FiscalDateEnding AS FiscalDateEnding,
                    @ReportedCurrency AS ReportedCurrency, @GrossProfit AS GrossProfit, @TotalRevenue AS TotalRevenue,
                    @CostOfRevenue AS CostOfRevenue, @CostofGoodsAndServicesSold AS CostofGoodsAndServicesSold,
                    @OperatingIncome AS OperatingIncome, @SellingGeneralAndAdministrative AS SellingGeneralAndAdministrative,
                    @ResearchAndDevelopment AS ResearchAndDevelopment, @OperatingExpenses AS OperatingExpenses,
                    @InvestmentIncomeNet AS InvestmentIncomeNet, @NetInterestIncome AS NetInterestIncome,
                    @InterestIncome AS InterestIncome, @InterestExpense AS InterestExpense, @NonInterestIncome AS NonInterestIncome,
                    @OtherNonOperatingIncome AS OtherNonOperatingIncome, @Depreciation AS Depreciation,
                    @DepreciationAndAmortization AS DepreciationAndAmortization, @IncomeBeforeTax AS IncomeBeforeTax,
                    @IncomeTaxExpense AS IncomeTaxExpense, @InterestAndDebtExpense AS InterestAndDebtExpense,
                    @NetIncomeFromContinuingOperations AS NetIncomeFromContinuingOperations,
                    @ComprehensiveIncomeNetOfTax AS ComprehensiveIncomeNetOfTax, @Ebit AS Ebit, @Ebitda AS Ebitda, @NetIncome AS NetIncome
            ) AS source
            ON (target.Symbol = source.Symbol AND target.FiscalDateEnding = source.FiscalDateEnding)
            WHEN NOT MATCHED THEN
                INSERT (
                    Symbol, FiscalDateEnding, ReportedCurrency, GrossProfit, TotalRevenue, CostOfRevenue,
                    CostofGoodsAndServicesSold, OperatingIncome, SellingGeneralAndAdministrative, ResearchAndDevelopment,
                    OperatingExpenses, InvestmentIncomeNet, NetInterestIncome, InterestIncome, InterestExpense,
                    NonInterestIncome, OtherNonOperatingIncome, Depreciation, DepreciationAndAmortization,
                    IncomeBeforeTax, IncomeTaxExpense, InterestAndDebtExpense, NetIncomeFromContinuingOperations,
                    ComprehensiveIncomeNetOfTax, Ebit, Ebitda, NetIncome
                )
                VALUES (
                    source.Symbol, source.FiscalDateEnding, source.ReportedCurrency, source.GrossProfit,
                    source.TotalRevenue, source.CostOfRevenue, source.CostofGoodsAndServicesSold, source.OperatingIncome,
                    source.SellingGeneralAndAdministrative, source.ResearchAndDevelopment, source.OperatingExpenses,
                    source.InvestmentIncomeNet, source.NetInterestIncome, source.InterestIncome, source.InterestExpense,
                    source.NonInterestIncome, source.OtherNonOperatingIncome, source.Depreciation, source.DepreciationAndAmortization,
                    source.IncomeBeforeTax, source.IncomeTaxExpense, source.InterestAndDebtExpense,
                    source.NetIncomeFromContinuingOperations, source.ComprehensiveIncomeNetOfTax, source.Ebit, source.Ebitda, source.NetIncome
                );";

            var affectedRows = 0;
            foreach (var report in annualReports)
            {
                var parameters = new
                {
                    Symbol = symbol,
                    FiscalDateEnding = DateTime.Parse(report.FiscalDateEnding),
                    report.ReportedCurrency,
                    report.GrossProfit,
                    report.TotalRevenue,
                    report.CostOfRevenue,
                    report.CostofGoodsAndServicesSold,
                    report.OperatingIncome,
                    report.SellingGeneralAndAdministrative,
                    report.ResearchAndDevelopment,
                    report.OperatingExpenses,
                    report.InvestmentIncomeNet,
                    report.NetInterestIncome,
                    report.InterestIncome,
                    report.InterestExpense,
                    report.NonInterestIncome,
                    report.OtherNonOperatingIncome,
                    report.Depreciation,
                    report.DepreciationAndAmortization,
                    report.IncomeBeforeTax,
                    report.IncomeTaxExpense,
                    report.InterestAndDebtExpense,
                    report.NetIncomeFromContinuingOperations,
                    report.ComprehensiveIncomeNetOfTax,
                    report.Ebit,
                    report.Ebitda,
                    report.NetIncome
                };
                affectedRows += await connection.ExecuteAsync(sql, parameters);
            }

            return affectedRows > 0;
        }
    }
}
