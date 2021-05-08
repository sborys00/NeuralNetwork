using NeuralNetwork.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NeuralNetwork.Core.Tests
{
    public class NetworkTests
    {
        [Fact]
        public void Constructor_ArraysShouldCorrectlyAllocate()
        {
            NetworkConfiguration configuration = new NetworkConfiguration
            {
                NumberOfLayers = 3,
                NeuronAmounts = new int[] { 3, 4, 5 },
            };

            Network network = new(configuration);

            Assert.Equal(configuration.NumberOfLayers, network.Layers.Length);
            Assert.Equal(configuration.NeuronAmounts[0], network.Layers[0].Neurons.Length);
            Assert.Equal(configuration.NeuronAmounts[1], network.Layers[1].Neurons.Length);
            Assert.Equal(configuration.NeuronAmounts[2], network.Layers[2].Neurons.Length);
            Assert.Equal(configuration.NeuronAmounts[1], network.Layers[0].Neurons[0].Weights.Length);
            Assert.Equal(configuration.NeuronAmounts[1], network.Layers[0].Neurons[1].Weights.Length);
            Assert.Equal(configuration.NeuronAmounts[2], network.Layers[1].Neurons[0].Weights.Length);
            Assert.Null(network.Layers[2].Neurons[0].Weights);
        }
    }
}
