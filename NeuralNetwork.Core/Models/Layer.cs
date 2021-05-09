using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.Core.Models
{
    public class Layer
    {
        public List<Neuron> Neurons { get; set; } = new();
    }
}
