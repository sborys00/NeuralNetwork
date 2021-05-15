using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.Core.Models
{
    public abstract class ActivationFuntion
    {
        public abstract Func<double, double> Function { get;} 
        public abstract Func<double, double> Derivative { get;} 
        public static double Sigmoid(double x) => 1 / (1 + Math.Exp(-x));
        public static double SigmoidBipolar(double x) => (1 - Math.Exp(x)) / (1 + Math.Exp(-x));
        public static double Tanh(double x) => (Math.Exp(x) - Math.Exp(-x)) / (Math.Exp(x) + Math.Exp(-x));
        public static double ReLU(double x) => Math.Max(0d, x);

        public static double SigmoidDerivative(double x) 
        {
            double y = Sigmoid(x);
            return y * (1 - y);
        }
        public static double SigmoidBipolarDerivative(double x)
        {
            double y = SigmoidBipolar(x);
            return 1 - (y * y);
        }
        public static double TanhDerivative(double x) 
        {
            double y = Tanh(x);
            return 1 - y * y;
        }

        public static double ReLUDerivative(double x) => x <= 0 ? 0 : 1;

    }
}
