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

        public TestResult(double[] expectedValues, double[] actualValues)
        {
            this.expectedValues = expectedValues;
            this.actualValues = actualValues;
        }
    }
}
