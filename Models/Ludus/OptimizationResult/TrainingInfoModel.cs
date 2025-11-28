namespace Temperance.Ephemeris.Models.Ludus.OptimizationResult
{
    public class TrainingInfoModel
    {
        public int NumTrainingSamples { get; set; }
        public int NumValSamples { get; set; }
        public int GPUsUsed { get; set; }
        public int BatchSize { get; set; }
        public double FinalLoss { get; set; }
    }
}
