using Newtonsoft.Json;

namespace Temperance.Ephemeris.Models.Financials
{
    public class EarningsModel
    {
        public string Symbol { get; set; }

        [JsonProperty("annualEarnings")]
        public List<AnnualEarningsModel> Annual { get; set; }

        [JsonProperty("quarterlyEarnings")]
        public List<QuarterlyEarningsModel> Quarterly { get; set; }
    }
}
