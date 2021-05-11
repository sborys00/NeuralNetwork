using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.Core.Models
{
    public class Neuron
    {
        public List<double> Weights { get; set; } = new();

        public static double CalculateOutput(double[] input, double[] enteringWeights)
        {
            if (input.Length != enteringWeights.Length)
                throw new Exception("Amount of input values is different from amount of weights coming in.");

            double output = 0;
            for (int i = 0; i < input.Length; i++)
            {
                output += input[i] * enteringWeights[i];
            }

            return output;
        }
    }
}
