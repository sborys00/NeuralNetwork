using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.Core.Models
{
    public class SigmoidActivationFunction : ActivationFuntion
    {
        public override string Name { get; } = "Sigmoid";
        public override Func<double, double> Function => ActivationFuntion.Sigmoid;
        public override Func<double, double> Derivative => ActivationFuntion.Sigmoid;
    }
}
