using NeuralNetwork.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.Core.Models
{
    public class TrainingConfig
    {
        public double ClassificationThreshold { get; set; }
        public double TargetError { get; set; }
        public double LearningRate { get; set; }
        public int TargetEpoch { get; set; }
        public int NumberOfSteps { get; set; }
        public string ActivationFunctionName { get; set; }
        public int Speed { get; set; }
    }
}
