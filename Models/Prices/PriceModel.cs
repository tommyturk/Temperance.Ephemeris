namespace Temperance.Ephemeris.Models.Prices
{
    public class PriceModel
    {
        public int SecurityID { get; set; }
        public string Symbol { get; set; }
        public DateTime Timestamp { get; set; }
        public string TimeInterval { get; set; }
        public decimal OpenPrice { get; set; }
        public decimal HighPrice { get; set; }
        public decimal LowPrice { get; set; }
        public decimal ClosePrice { get; set; }
        public decimal AdjustedClosePrice { get; set; }
        public long Volume { get; set; }
        public decimal DividendAmount { get; set; }
        public decimal SplitCoefficient { get; set; }
    }
}
