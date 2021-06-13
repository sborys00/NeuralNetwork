using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.Core.Models
{
    public class TrainingDataset
    {
        public IEnumerable<string> VariableNames { get; set; }
        public IEnumerable<TrainingDataExample> TrainingExamples { get; set; }
        public IEnumerable<TrainingDataExample> TestExamples { get; set; }
    }
}
