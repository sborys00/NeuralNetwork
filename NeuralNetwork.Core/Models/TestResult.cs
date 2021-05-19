using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.Core.Models
{
    public struct TestResult
    {
        public readonly double[] expectedValues;
        public readonly double[] actualValues;
        public List<List<double>> networkOutputs;

        public TestResult(double[] expectedValues, double[] actualValues, List<List<double>> networkOutputs)
        {
            this.expectedValues = expectedValues;
            this.actualValues = actualValues;
            this.networkOutputs = networkOutputs;
        }
    }
}
