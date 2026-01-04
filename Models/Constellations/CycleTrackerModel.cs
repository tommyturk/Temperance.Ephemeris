namespace Temperance.Ephemeris.Models.Constellations
{
    public class CycleTrackerModel
    {
        public Guid CycleTrackerId { get; set; }
        public Guid SessionId { get; set; }
        public DateTime CycleStartDate { get; set; }
        public DateTime OosStartDate { get; set; }
        public DateTime OosEndDate { get; set; }
        public Guid PortfolioBacktestRunId { get; set; }
        public Guid ShadowBacktestRunId { get; set; }
        public bool IsPortfolioBacktestComplete { get; set; }
        public bool IsShadowBacktestComplete { get; set; }
        public bool IsOptimizationDispatched { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
