namespace Temperance.Ephemeris.Models.Trading
{
    public class Position
    {
        public int PositionID { get; set; }
        public int SecurityID { get; set; }
        public string Symbol { get; set; }
        public int Quantity { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal? UnrealizedPL { get; set; }
        public string Status { get; set; } = "Open";
    }
}
