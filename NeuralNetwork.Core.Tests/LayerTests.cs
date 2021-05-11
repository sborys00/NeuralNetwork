using NeuralNetwork.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NeuralNetwork.Core.Tests
{
    public class LayerTests
    {
        [Fact]
        public void CalculateOutput_ShouldWork()
        {
            NetworkConfiguration configuration = new NetworkConfiguration
            {
                NumberOfLayers = 3,
                NeuronAmounts = new int[] { 4, 3, 5 },
            };

            Network network = new(configuration);
            network.Layers[0].Neurons.ForEach(n => n.Weights = new List<double>(new double[] { .1, .2, .3 }));
            network.Layers[1].Neurons.ForEach(n => n.Weights = new List<double>(new double[] { .4, .5, .6, .7, .8 }));

            double[] expectedOutput = { 1.6, 3.2, 4.8 };
            network.Layers[1].CalculateOutputs(new double[] { 5, 6, 2, 3 }, Network.GetWeightsFromLayer(network.Layers[0]), x => x, out double[] actualOutput, out double[,] currentWeights);

            Assert.Equal(expectedOutput, actualOutput);
        }
    }
}
