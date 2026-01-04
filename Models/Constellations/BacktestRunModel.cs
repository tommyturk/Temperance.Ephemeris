using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Temperance.Ephemeris.Models.Backtesting
{
    [Table("BacktestRuns", Schema = "Constellations")]
    public class BacktestRunModel
    {
        [Key]
        public Guid RunId { get; set; }
        public string StrategyName { get; set; } = string.Empty;

        // Raw JSON strings for Database persistence
        public string SymbolsJson { get; set; } = "[]";
        public string IntervalsJson { get; set; } = "[]";

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal InitialCapital { get; set; }
        public string Status { get; set; } = "Pending"; // e.g., Running, Completed, Failed

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        // Performance Metrics
        public decimal TotalProfitLoss { get; set; }
        public decimal TotalReturn { get; set; }
        public decimal MaxDrawdown { get; set; }
        public decimal WinRate { get; set; }
        public decimal SharpeRatio { get; set; }
        public int TotalTrades { get; set; }

        public string? ErrorMessage { get; set; }

        // Link to Ludus Optimization logic
        public int? OptimizationResultId { get; set; }
        public int? SessionId { get; set; }

        // .NET 9 Helper Properties for easy access in code
        [NotMapped]
        public List<string> Symbols
        {
            get => string.IsNullOrEmpty(SymbolsJson) ? new() : JsonSerializer.Deserialize<List<string>>(SymbolsJson) ?? new();
            set => SymbolsJson = JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public List<string> Intervals
        {
            get => string.IsNullOrEmpty(IntervalsJson) ? new() : JsonSerializer.Deserialize<List<string>>(IntervalsJson) ?? new();
            set => IntervalsJson = JsonSerializer.Serialize(value);
        }
    }
}