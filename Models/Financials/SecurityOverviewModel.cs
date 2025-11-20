using Newtonsoft.Json;
using Temperance.Ephemeris.Utilities.Extensions;

namespace Temperance.Ephemeris.Models.Financials
{
    public class SecurityOverviewModel
    {
        public int Id { get; set; }
        public int SecurityID { get; set; }
        public string? Symbol { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? CIK { get; set; }
        public string? Exchange { get; set; }
        public string? Currency { get; set; }
        public string? Country { get; set; }
        public string? Sector { get; set; }
        public string? Industry { get; set; }
        public string? Address { get; set; }
        public string? OfficialSite { get; set; }
        public string? FiscalYearEnd { get; set; }
        public DateTime? LatestQuarter { get; set; }
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? MarketCapitalization { get; set; }
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? EBITDA { get; set; }
        [JsonProperty("PERatio")]
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? PERatio { get; set; }
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? PEGRatio { get; set; }
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? BookValue { get; set; }
        private string? _dividendPerShare;
        public string DividendPerShare { get; set; }
        public double ParsedDividendPerShare { get; set; }
        private string? _dividendYield;
        public string DividendYield { get; set; }
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double ParsedDividendYield { get; set; }
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? EPS { get; set; }
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? RevenuePerShareTTM { get; set; }
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? ProfitMargin { get; set; }
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? OperatingMarginTTM { get; set; }
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? ReturnOnAssetsTTM { get; set; }
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? ReturnOnEquityTTM { get; set; }
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? RevenueTTM { get; set; }
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? GrossProfitTTM { get; set; }
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? DilutedEPSTTM { get; set; }
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? QuarterlyEarningsGrowthYOY { get; set; }
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? QuarterlyRevenueGrowthYOY { get; set; }
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? AnalystTargetPrice { get; set; }
        public int AnalystRatingStrongBuy { get; set; }
        public int AnalystRatingBuy { get; set; }
        public int AnalystRatingHold { get; set; }
        public int AnalystRatingSell { get; set; }
        public int AnalystRatingStrongSell { get; set; }
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? TrailingPE { get; set; }
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? ForwardPE { get; set; }
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? PriceToSalesRatioTTM { get; set; }
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? PriceToBookRatio { get; set; }
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? EVToRevenue { get; set; }
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? EVToEBITDA { get; set; }
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? Beta { get; set; }
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? FiftyTwoWeekHigh { get; set; }
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? FiftyTwoWeekLow { get; set; }
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? FiftyDayMovingAverage { get; set; }
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? TwoHundredDayMovingAverage { get; set; }
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? SharesOutstanding { get; set; }
        private string? _dividendDate;
        public string? DividendDate
        {
            get => _dividendDate;
            set
            {
                _dividendDate = value;
                ParsedDividendDate = ParseDate(value);
            }
        }
        public DateTime? ParsedDividendDate { get; private set; }

        private string _exDividendDate;
        public string ExDividendDate
        {
            get => _exDividendDate;
            set
            {
                _exDividendDate = value;
                ParsedExDividendDate = ParseDate(value);
            }
        }
        public DateTime? ParsedExDividendDate { get; private set; }

        private string? _lastUpdated;
        public string? LastUpdated
        {
            get => _lastUpdated;
            set
            {
                _lastUpdated = value;
                ParsedLastUpdated = ParseDate(value);
            }
        }
        public DateTime? ParsedLastUpdated { get; private set; }

        private DateTime? ParseDate(string date)
        {
            if (DateTime.TryParse(date, out DateTime parsedDate))
            {
                return parsedDate;
            }
            return null;
        }
    }
}
