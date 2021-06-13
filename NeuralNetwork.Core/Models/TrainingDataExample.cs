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
    public class TrainingDataExample
    {
        public double[] InputValues { get;}
        public double[] ExpectedOutputs { get;}

        public TrainingDataExample(double[] InputValues, double[] ExpectedOutputs)
        {
            this.InputValues = InputValues;
            this.ExpectedOutputs = ExpectedOutputs;
        }
    }
}
