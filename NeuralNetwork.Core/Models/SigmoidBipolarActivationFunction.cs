using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.Core.Models
{
    public class SigmoidBipolarActivationFunction : ActivationFuntion
    {
        public override Func<double, double> Function => ActivationFuntion.SigmoidBipolar;
        public override Func<double, double> Derivative => ActivationFuntion.SigmoidBipolarDerivative;
    }
}
