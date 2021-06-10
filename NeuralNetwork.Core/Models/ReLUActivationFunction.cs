using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.Core.Models
{
    public class ReLUActivationFunction : ActivationFuntion
    {
        public override string Name { get; } = "ReLU";

        public override Func<double, double> Function => ActivationFuntion.ReLU;
        public override Func<double, double> Derivative => ActivationFuntion.ReLUDerivative;
    }
}
