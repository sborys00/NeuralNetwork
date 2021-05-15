using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.Core.ExtensionMethods
{
    public static class RandomExtensions
    {
        public static double GetRandomDoubleBetween(this Random random, double upperBound, double lowerBound)
        {
            return lowerBound + (random.NextDouble() * (upperBound - lowerBound));
        }
    }
}
