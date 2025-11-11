namespace Temperance.Ephemeris.Models.Indicators
{
    public class IndicatorBackFillStatus
    {
        public int Id { get; set; }

        public string? Indicator { get; set; }

        public string? Interval { get; set; }

        public string? Status { get; set; }

        public DateTime? StartTime { get; set; } = null;

        public DateTime? EndTime { get; set; } = null;
    }
}
