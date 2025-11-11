using Newtonsoft.Json;
using System.ComponentModel;

namespace Temperance.Ephemeris.Models.Financials
{
    public class AnnualEarningsModel
    {
        public int Id { get; set; }

        public int SecurityId { get; set; }

        public string Symbol { get; set; }

        public DateTime FiscalDateEnding { get; set; }

        [JsonConverter(typeof(DecimalConverter))]
        public decimal ReportedEPS { get; set; }
    }
}
