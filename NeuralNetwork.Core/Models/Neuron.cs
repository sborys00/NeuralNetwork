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
        public double Value { get; set; }
    }
}
