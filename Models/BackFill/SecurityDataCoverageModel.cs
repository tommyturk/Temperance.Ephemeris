namespace Temperance.Ephemeris.Models.BackFill
{
    public class SecurityDataCoverageModel
    {
        public string? Symbol { get; set; }
        public string? Interval { get; set; }
        public string? Type { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public bool Success { get; set; }
    }
}
