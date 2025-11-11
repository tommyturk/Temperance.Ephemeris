namespace Temperance.Ephemeris.Models.BackFill
{
    public class SecurityQueueModel
    {
        public string Symbol { get; set; }
        public string Type { get; set; }
        public string Interval { get; set; }
        public SecurityQueueStatus Status { get; set; }
        public DateTime? StartTime { get; set; } = null;
        public DateTime? EndTime { get; set; } = null;
        public string ErrorMessage { get; set; }
    }
}
