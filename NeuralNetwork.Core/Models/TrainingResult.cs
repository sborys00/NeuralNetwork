using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.Core.Models
{
    public struct TrainingResult
    {
        public double TrainingExampleTotalError;
        public double TestExampleTotalError;
        public TestResult[] trainingResults;
        public TestResult[] testingResults;
    }
}
