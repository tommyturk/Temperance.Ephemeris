namespace Temperance.Ephemeris.Models.Trading
{
    public class Trade
    {
        public int TradeID { get; set; }
        public int SecurityID { get; set; }
        public string Symbol { get; set; }
        public string Strategy { get; set; } = "Mean Reversion";
        public TradeType TradeType { get; set; }
        public decimal SignalPrice { get; set; }
        public decimal? TakeProfitPrice { get; set; }
        public decimal? StopLossPrice { get; set; }
        public DateTime SignalTimestamp { get; set; }
        public string Status { get; set; } = "Pending";
        public decimal? ExitPrice { get; set; } 
        public DateTime? ExitTimestamp { get; set; } 
        public decimal? ProfitLoss { get; set; }
    }
}
