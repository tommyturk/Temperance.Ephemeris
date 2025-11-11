using Dapper;
using Microsoft.Data.SqlClient;
using Temperance.Ephemeris.Models.Financials;
using Temperance.Ephemeris.Repositories.Financials.Interfaces;

namespace Temperance.Ephemeris.Repositories.Financials.Implementations
{
    public class BalanceSheetRepository : IBalanceSheetRepository
    {
        private readonly string _connectionString;
        public BalanceSheetRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<BalanceSheetModel> GetSecurityBalanceSheet(int securityId)
        {
            using var connection = new SqlConnection(_connectionString);

            var queryQuarterly = @"
                SELECT TOP(4) 
                    [SecurityID]
                    ,[Symbol]
                    ,[FiscalDateEnding]
                    ,[ReportedCurrency]
                    ,[TotalAssets]
                    ,[TotalCurrentAssets]
                    ,[CashAndCashEquivalentsAtCarryingValue]
                    ,[CashAndShortTermInvestments]
                    ,[Inventory]
                    ,[CurrentNetReceivables]
                    ,[TotalNonCurrentAssets]
                    ,[PropertyPlantEquipment]
                    ,[AccumulatedDepreciationAmortizationPPE]
                    ,[IntangibleAssets]
                    ,[IntangibleAssetsExcludingGoodwill]
                    ,[Goodwill]
                    ,[Investments]
                    ,[LongTermInvestments]
                    ,[ShortTermInvestments]
                    ,[OtherCurrentAssets]
                    ,[OtherNonCurrentAssets]
                    ,[TotalLiabilities]
                    ,[TotalCurrentLiabilities]
                    ,[CurrentAccountsPayable]
                    ,[DeferredRevenue]
                    ,[CurrentDebt]
                    ,[ShortTermDebt]
                    ,[TotalNonCurrentLiabilities]
                    ,[CapitalLeaseObligations]
                    ,[LongTermDebt]
                    ,[CurrentLongTermDebt]
                    ,[LongTermDebtNoncurrent]
                    ,[ShortLongTermDebtTotal]
                    ,[OtherCurrentLiabilities]
                    ,[OtherNonCurrentLiabilities]
                    ,[TotalShareholderEquity]
                    ,[TreasuryStock]
                    ,[RetainedEarnings]
                    ,[CommonStock]
                    ,[CommonStockSharesOutstanding]
                    ,[CreatedAt]
                FROM [TradingBotDb].[Financials].[BalanceSheetQuarterly]
                WHERE SecurityID = @SecurityId
                ORDER BY FiscalDateEnding DESC";

            var parameters = new { SecurityId = securityId };
            var quarterlyData = (await connection.QueryAsync<BalanceSheetQuarterlyModel>(queryQuarterly, parameters)).ToList();

            var queryAnnual = @"
                SELECT TOP(4) 
                    [SecurityID]
                    ,[Symbol]
                    ,[FiscalDateEnding]
                    ,[ReportedCurrency]
                    ,[TotalAssets]
                    ,[TotalCurrentAssets]
                    ,[CashAndCashEquivalentsAtCarryingValue]
                    ,[CashAndShortTermInvestments]
                    ,[Inventory]
                    ,[CurrentNetReceivables]
                    ,[TotalNonCurrentAssets]
                    ,[PropertyPlantEquipment]
                    ,[AccumulatedDepreciationAmortizationPPE]
                    ,[IntangibleAssets]
                    ,[IntangibleAssetsExcludingGoodwill]
                    ,[Goodwill]
                    ,[Investments]
                    ,[LongTermInvestments]
                    ,[ShortTermInvestments]
                    ,[OtherCurrentAssets]
                    ,[OtherNonCurrentAssets]
                    ,[TotalLiabilities]
                    ,[TotalCurrentLiabilities]
                    ,[CurrentAccountsPayable]
                    ,[DeferredRevenue]
                    ,[CurrentDebt]
                    ,[ShortTermDebt]
                    ,[TotalNonCurrentLiabilities]
                    ,[CapitalLeaseObligations]
                    ,[LongTermDebt]
                    ,[CurrentLongTermDebt]
                    ,[LongTermDebtNoncurrent]
                    ,[ShortLongTermDebtTotal]
                    ,[OtherCurrentLiabilities]
                    ,[OtherNonCurrentLiabilities]
                    ,[TotalShareholderEquity]
                    ,[TreasuryStock]
                    ,[RetainedEarnings]
                    ,[CommonStock]
                    ,[CommonStockSharesOutstanding]
                    ,[CreatedAt]
                FROM [TradingBotDb].[Financials].[BalanceSheetAnnual]
                WHERE SecurityID = @SecurityId
                ORDER BY FiscalDateEnding DESC";

            var annualData = (await connection.QueryAsync<BalanceSheetAnnualModel>(queryAnnual, parameters)).ToList();

            var balanceSheetData = new BalanceSheetModel
            {
                AnnualReports = annualData,
                QuarterlyReports = quarterlyData,
                Symbol = annualData.FirstOrDefault()?.Symbol ?? quarterlyData.FirstOrDefault()?.Symbol
            };

            return balanceSheetData;
        }


        public async Task<bool> InsertSecuritiesBalanceSheetData(int securityId, BalanceSheetModel balanceSheetData, string symbol)
        {
            var annualSuccess = false;
            var quarterlySuccess = false;

            if (balanceSheetData.AnnualReports != null)
                annualSuccess = await InsertAnnualBalanceSheetData(securityId, balanceSheetData.AnnualReports, symbol);
            if (annualSuccess)
                Console.WriteLine($"Save balance sheet success: {annualSuccess}");

            if (balanceSheetData.QuarterlyReports != null)
                quarterlySuccess = await InsertQuarterlyBalanceSheetData(securityId, balanceSheetData.QuarterlyReports, symbol);
            if(annualSuccess)
                Console.WriteLine($"Save balance sheet success: {quarterlySuccess}");
            return annualSuccess || quarterlySuccess;
        }

        private async Task<bool> CheckIfBalanceSheetExists<T>(int securityId, List<T> reports) where T : IBalanceSheetReport
        {
            if (reports == null || !reports.Any())
                return false;

            using var connection = new SqlConnection(_connectionString);

            var tableName = typeof(T) == typeof(BalanceSheetAnnualModel) ?
                "[TradingBotDb].[Financials].[BalanceSheetAnnual]" :
                "[TradingBotDb].[Financials].[BalanceSheetQuarterly]";

            var query = $@"
                SELECT COUNT(1) 
                FROM {tableName} 
                WHERE SecurityID = @SecurityID 
                AND FiscalDateEnding IN @FiscalDateEndings";

            var fiscalDates = reports.Select(r => r.FiscalDateEnding).ToList();

            var count = await connection.ExecuteScalarAsync<int>(query, new
            {
                SecurityID = securityId,
                FiscalDateEndings = fiscalDates
            });

            return count > 0;
        }


        private async Task<bool> InsertAnnualBalanceSheetData(int securityId, List<BalanceSheetAnnualModel> annualReports, string symbol)
        {
            var existsCheck = await CheckIfBalanceSheetExists(securityId, annualReports);
            if (existsCheck)
                return true;

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    var query = @"
                    INSERT INTO [TradingBotDb].[Financials].[BalanceSheetAnnual] 
                    (
                        SecurityID, Symbol, FiscalDateEnding, ReportedCurrency, TotalAssets, 
                        TotalCurrentAssets, CashAndCashEquivalentsAtCarryingValue, CashAndShortTermInvestments, 
                        Inventory, CurrentNetReceivables, TotalNonCurrentAssets, PropertyPlantEquipment, 
                        AccumulatedDepreciationAmortizationPPE, IntangibleAssets, IntangibleAssetsExcludingGoodwill, 
                        Goodwill, Investments, LongTermInvestments, ShortTermInvestments, OtherCurrentAssets, 
                        OtherNonCurrentAssets, TotalLiabilities, TotalCurrentLiabilities, CurrentAccountsPayable, 
                        DeferredRevenue, CurrentDebt, ShortTermDebt, TotalNonCurrentLiabilities, CapitalLeaseObligations, 
                        LongTermDebt, CurrentLongTermDebt, LongTermDebtNoncurrent, ShortLongTermDebtTotal, 
                        OtherCurrentLiabilities, OtherNonCurrentLiabilities, TotalShareholderEquity, TreasuryStock, 
                        RetainedEarnings, CommonStock, CommonStockSharesOutstanding, CreatedAt
                    ) 
                    VALUES 
                    (
                        @SecurityID, @Symbol, @FiscalDateEnding, @ReportedCurrency, @TotalAssets, 
                        @TotalCurrentAssets, @CashAndCashEquivalentsAtCarryingValue, @CashAndShortTermInvestments, 
                        @Inventory, @CurrentNetReceivables, @TotalNonCurrentAssets, @PropertyPlantEquipment, 
                        @AccumulatedDepreciationAmortizationPPE, @IntangibleAssets, @IntangibleAssetsExcludingGoodwill, 
                        @Goodwill, @Investments, @LongTermInvestments, @ShortTermInvestments, @OtherCurrentAssets, 
                        @OtherNonCurrentAssets, @TotalLiabilities, @TotalCurrentLiabilities, @CurrentAccountsPayable, 
                        @DeferredRevenue, @CurrentDebt, @ShortTermDebt, @TotalNonCurrentLiabilities, @CapitalLeaseObligations, 
                        @LongTermDebt, @CurrentLongTermDebt, @LongTermDebtNoncurrent, @ShortLongTermDebtTotal, 
                        @OtherCurrentLiabilities, @OtherNonCurrentLiabilities, @TotalShareholderEquity, @TreasuryStock, 
                        @RetainedEarnings, @CommonStock, @CommonStockSharesOutstanding, @CreatedAt
                    )";

                    var parameters = annualReports.Select(report => new
                    {
                        SecurityID = securityId,
                        Symbol = symbol,
                        report.FiscalDateEnding,
                        report.ReportedCurrency,
                        report.TotalAssets,
                        report.TotalCurrentAssets,
                        report.CashAndCashEquivalentsAtCarryingValue,
                        report.CashAndShortTermInvestments,
                        report.Inventory,
                        report.CurrentNetReceivables,
                        report.TotalNonCurrentAssets,
                        report.PropertyPlantEquipment,
                        report.AccumulatedDepreciationAmortizationPPE,
                        report.IntangibleAssets,
                        report.IntangibleAssetsExcludingGoodwill,
                        report.Goodwill,
                        report.Investments,
                        report.LongTermInvestments,
                        report.ShortTermInvestments,
                        report.OtherCurrentAssets,
                        report.OtherNonCurrentAssets,
                        report.TotalLiabilities,
                        report.TotalCurrentLiabilities,
                        report.CurrentAccountsPayable,
                        report.DeferredRevenue,
                        report.CurrentDebt,
                        report.ShortTermDebt,
                        report.TotalNonCurrentLiabilities,
                        report.CapitalLeaseObligations,
                        report.LongTermDebt,
                        report.CurrentLongTermDebt,
                        report.LongTermDebtNoncurrent,
                        report.ShortLongTermDebtTotal,
                        report.OtherCurrentLiabilities,
                        report.OtherNonCurrentLiabilities,
                        report.TotalShareholderEquity,
                        report.TreasuryStock,
                        report.RetainedEarnings,
                        report.CommonStock,
                        report.CommonStockSharesOutstanding,
                        CreatedAt = DateTime.UtcNow
                    });

                    return await connection.ExecuteAsync(query, parameters) > 0;
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"SQL Error: {ex.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return false;
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        private async Task<bool> InsertQuarterlyBalanceSheetData(int securityId, List<BalanceSheetQuarterlyModel> quarterlyReports, string symbol)
        {
            var existsCheck = await CheckIfBalanceSheetExists(securityId, quarterlyReports);
            if (existsCheck)
                return true;

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    var query = @"
                INSERT INTO [TradingBotDb].[Financials].[BalanceSheetQuarterly] 
                (
                    SecurityID, Symbol, FiscalDateEnding, ReportedCurrency, TotalAssets, 
                    TotalCurrentAssets, CashAndCashEquivalentsAtCarryingValue, CashAndShortTermInvestments, 
                    Inventory, CurrentNetReceivables, TotalNonCurrentAssets, PropertyPlantEquipment, 
                    AccumulatedDepreciationAmortizationPPE, IntangibleAssets, IntangibleAssetsExcludingGoodwill, 
                    Goodwill, Investments, LongTermInvestments, ShortTermInvestments, OtherCurrentAssets, 
                    OtherNonCurrentAssets, TotalLiabilities, TotalCurrentLiabilities, CurrentAccountsPayable, 
                    DeferredRevenue, CurrentDebt, ShortTermDebt, TotalNonCurrentLiabilities, CapitalLeaseObligations, 
                    LongTermDebt, CurrentLongTermDebt, LongTermDebtNoncurrent, ShortLongTermDebtTotal, 
                    OtherCurrentLiabilities, OtherNonCurrentLiabilities, TotalShareholderEquity, TreasuryStock, 
                    RetainedEarnings, CommonStock, CommonStockSharesOutstanding, CreatedAt
                ) 
                VALUES 
                (
                    @SecurityID, @Symbol, @FiscalDateEnding, @ReportedCurrency, @TotalAssets, 
                    @TotalCurrentAssets, @CashAndCashEquivalentsAtCarryingValue, @CashAndShortTermInvestments, 
                    @Inventory, @CurrentNetReceivables, @TotalNonCurrentAssets, @PropertyPlantEquipment, 
                    @AccumulatedDepreciationAmortizationPPE, @IntangibleAssets, @IntangibleAssetsExcludingGoodwill, 
                    @Goodwill, @Investments, @LongTermInvestments, @ShortTermInvestments, @OtherCurrentAssets, 
                    @OtherNonCurrentAssets, @TotalLiabilities, @TotalCurrentLiabilities, @CurrentAccountsPayable, 
                    @DeferredRevenue, @CurrentDebt, @ShortTermDebt, @TotalNonCurrentLiabilities, @CapitalLeaseObligations, 
                    @LongTermDebt, @CurrentLongTermDebt, @LongTermDebtNoncurrent, @ShortLongTermDebtTotal, 
                    @OtherCurrentLiabilities, @OtherNonCurrentLiabilities, @TotalShareholderEquity, @TreasuryStock, 
                    @RetainedEarnings, @CommonStock, @CommonStockSharesOutstanding, @CreatedAt
                )";

                    var parameters = quarterlyReports.Select(report => new
                    {
                        SecurityID = securityId,
                        Symbol = symbol,
                        report.FiscalDateEnding,
                        report.ReportedCurrency,
                        report.TotalAssets,
                        report.TotalCurrentAssets,
                        report.CashAndCashEquivalentsAtCarryingValue,
                        report.CashAndShortTermInvestments,
                        report.Inventory,
                        report.CurrentNetReceivables,
                        report.TotalNonCurrentAssets,
                        report.PropertyPlantEquipment,
                        report.AccumulatedDepreciationAmortizationPPE,
                        report.IntangibleAssets,
                        report.IntangibleAssetsExcludingGoodwill,
                        report.Goodwill,
                        report.Investments,
                        report.LongTermInvestments,
                        report.ShortTermInvestments,
                        report.OtherCurrentAssets,
                        report.OtherNonCurrentAssets,
                        report.TotalLiabilities,
                        report.TotalCurrentLiabilities,
                        report.CurrentAccountsPayable,
                        report.DeferredRevenue,
                        report.CurrentDebt,
                        report.ShortTermDebt,
                        report.TotalNonCurrentLiabilities,
                        report.CapitalLeaseObligations,
                        report.LongTermDebt,
                        report.CurrentLongTermDebt,
                        report.LongTermDebtNoncurrent,
                        report.ShortLongTermDebtTotal,
                        report.OtherCurrentLiabilities,
                        report.OtherNonCurrentLiabilities,
                        report.TotalShareholderEquity,
                        report.TreasuryStock,
                        report.RetainedEarnings,
                        report.CommonStock,
                        report.CommonStockSharesOutstanding,
                        CreatedAt = DateTime.UtcNow
                    });

                    return await connection.ExecuteAsync(query, parameters) > 0;
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"SQL Error: {ex.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return false;
                }
                finally
                {
                    connection.Close();
                }
            }
        }
    }
}
