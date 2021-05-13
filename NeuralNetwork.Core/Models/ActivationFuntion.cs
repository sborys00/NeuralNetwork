using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.Core.Models
{
    public static class ActivationFuntion
    {
        public static double Sigmoid(double x) => 1 / (1 + Math.Exp(-x));
        public static double Tanh(double x) => (Math.Exp(x) - Math.Exp(-x)) / (Math.Exp(x) + Math.Exp(-x));
        public static double ReLU(double x) => Math.Max(0d, x);               
    }
}
