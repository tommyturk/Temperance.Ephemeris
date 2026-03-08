using Temperance.Ephemeris.Models.Trading;

namespace Temperance.Ephemeris.Models.Traiding
{
    public class TradeSignal
    {
        public int TradeID { get; set; }
        public int SecurityID { get; set; }
        public string Symbol { get; set; }
        public TradeType Type { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal ProfitOrLoss { get; set; }
        public string Strategy { get; set; }
        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            return $"{Type} signal at {Timestamp}: ${Price}";
        }
    }
}
