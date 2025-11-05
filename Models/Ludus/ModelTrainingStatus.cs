using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temperance.Ephemeris.Models.Ludus
{
    public class ModelTrainingStatus
    {
        public int Id { get; set; }
        public string StrategyName { get; set; }
        public string Symbol { get; set; }
        public string Interval { get; set; }
        public bool InitialTrainingSent { get; set; }
        public DateTime? LastTrainingDate { get; set; }
    }
}
