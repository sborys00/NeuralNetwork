using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.Core.Models
{
    public class TanhActivationFunction : ActivationFuntion
    {
        public override Func<double, double> Function => ActivationFuntion.Tanh;
        public override Func<double, double> Derivative => ActivationFuntion.TanhDerivative;
    }
}
