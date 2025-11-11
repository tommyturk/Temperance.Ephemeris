namespace Temperance.Ephemeris.Models.Indicators
{
    public class IndicatorBaseModel
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public string Unit { get; set; }

        public string? Interval { get; set; }

        public List<IndicatorValueSet> Data { get; set; }
    }
}
