using Dapper;
using Microsoft.Data.SqlClient;
using Temperance.Delphi.Models.MeanReversion;
using Temperance.Delphi.Models.Trading;
using Temperance.Ephemeris.Repositories.Trade.Interfaces;
using Temperance.Ephemeris.Repositories_Old.HistoricalPrices;
namespace Temperance.Ephemeris.Repositories.Trade.Implementations
{
    public class TradeRepository : ITradeRepository
    {
        private readonly string _connectionString;
        private readonly IHistoricalPriceRepository _historicalPricesRepository;

        public TradeRepository(string connectionString, IHistoricalPriceRepository historicalPriceRepository)
        {
            _connectionString = connectionString;
            _historicalPricesRepository = historicalPriceRepository;
        }

        public async Task<int> SaveTradeAsync(Models.Trading.Trade trade)
        {
            using var connection = new SqlConnection(_connectionString);
            string query = @"
            INSERT INTO [Trading].[Trades] 
            (SecurityID, Symbol, Strategy, TradeType, SignalPrice, SignalTimestamp, Status) 
            VALUES (@SecurityID, @Symbol, @Strategy, @TradeType, @SignalPrice, @SignalTimestamp, @Status);
            SELECT SCOPE_IDENTITY();";
            return await connection.ExecuteScalarAsync<int>(query, trade);
        }

        public async Task<int> ExecuteOrderAsync(Order order)
        {
            using var connection = new SqlConnection(_connectionString);
            string query = @"
            INSERT INTO [Trading].[Orders] 
            (TradeID, ExecutionPrice, ExecutionTimestamp, OrderStatus, Quantity) 
            VALUES (@TradeID, @ExecutionPrice, @ExecutionTimestamp, @OrderStatus, @Quantity);
            SELECT SCOPE_IDENTITY();";
            return await connection.ExecuteScalarAsync<int>(query, order);
        }

        public async Task<int> UpdatePositionAsync(Position position)
        {
            using var connection = new SqlConnection(_connectionString);
            string query = @"
            MERGE INTO [Trading].[Positions] AS target
            USING (SELECT @SecurityID AS SecurityID, @Symbol AS Symbol) AS source
            ON target.SecurityID = source.SecurityID
            WHEN MATCHED THEN
                UPDATE SET 
                    Quantity = target.Quantity + @Quantity, 
                    AveragePrice = (@AveragePrice * @Quantity + target.AveragePrice * target.Quantity) / (target.Quantity + @Quantity)
            WHEN NOT MATCHED THEN
                INSERT (SecurityID, Symbol, Quantity, AveragePrice, UnrealizedPL, Status) 
                VALUES (@SecurityID, @Symbol, @Quantity, @AveragePrice, NULL, 'Open');
            ";
            return await connection.ExecuteAsync(query, position);
        }

        public async Task<int> LogStrategyAsync(StrategyLog log)
        {
            using var connection = new SqlConnection(_connectionString);
            string query = @"
            INSERT INTO [Trading].[StrategyLogs] 
            (TradeID, MovingAverage, StandardDeviation, UpperThreshold, LowerThreshold, Reason, CreatedAt) 
            VALUES (@TradeID, @MovingAverage, @StandardDeviation, @UpperThreshold, @LowerThreshold, @Reason, @CreatedAt);";
            return await connection.ExecuteAsync(query, log);
        }

        public async Task CheckTradeExitsAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            string query = "SELECT * FROM [Trading].[Trades] WHERE Status = 'Open'";
            var openTrades = await connection.QueryAsync<Models.Trading.Trade>(query);

            foreach (var trade in openTrades)
            {
                decimal latestPrice = await _historicalPricesRepository.GetLatestPriceAsync(trade.Symbol, trade.SignalTimestamp);

                bool shouldSell = (trade.TradeType == TradeType.Buy && (latestPrice >= trade.TakeProfitPrice || latestPrice <= trade.StopLossPrice)) ||
                                  (trade.TradeType == TradeType.Sell && (latestPrice <= trade.TakeProfitPrice || latestPrice >= trade.StopLossPrice));

                if (!shouldSell)
                    return;
                
                decimal profitLoss = (trade.TradeType == TradeType.Buy)
                    ? latestPrice - trade.SignalPrice
                    : trade.SignalPrice - latestPrice;

                var order = new Order
                {
                    TradeID = trade.TradeID,
                    ExecutionPrice = latestPrice,
                    ExecutionTimestamp = DateTime.UtcNow,
                    OrderStatus = "Executed",
                    Quantity = 1
                };

                await ExecuteOrderAsync(order);

                await connection.ExecuteAsync(@"
                        UPDATE [Trading].[Trades] 
                        SET Status = 'Closed', ExitPrice = @ExitPrice, ExitTimestamp = @ExitTimestamp, ProfitLoss = @ProfitLoss 
                        WHERE TradeID = @TradeID",
                    new { ExitPrice = latestPrice, ExitTimestamp = DateTime.UtcNow, ProfitLoss = profitLoss, trade.TradeID });
            }
        }
    }
}
