using Temperance.Ephemeris.Models.Ludus.OptimizationResult;

namespace Temperance.Ephemeris.Models.Ludus
{
    public class OptimizationResultModel
    {
        public Guid JobId { get; set; }
        public int Id { get; set; }
        public string StrategyName { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public string Interval { get; set; } = string.Empty;
        public BestParametersModel? OptimizedParameters { get; set; }
        public OptimizationMetrics Metrics { get; set; }
        public TrainingInfoModel TrainingInfo { get; set; }
        public double? TotalReturns { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
