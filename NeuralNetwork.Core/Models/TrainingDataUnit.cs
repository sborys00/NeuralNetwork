using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.Core.Models
{
    /// <summary>
    /// Stores input values and expected output of a network for training purpose. 
    /// </summary>
    public class TrainingDataUnit
    {
        public readonly double[] inputValues;
        public readonly double expectedOutput;

        public TrainingDataUnit(double[] inputValues, double expectedOutput)
        {
            this.inputValues = inputValues;
            this.expectedOutput = expectedOutput;
        }
    }
}
