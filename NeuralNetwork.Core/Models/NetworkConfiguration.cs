using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.Core.Models
{
    public class NetworkConfiguration
    {
        public int NumberOfLayers { get; set; } // hidden + input and output
        public int[] NeuronAmounts { get; set; }
    }
}
