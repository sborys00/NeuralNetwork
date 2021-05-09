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

            Assert.Equal(configuration.NumberOfLayers, network.Layers.Count);
            Assert.Equal(configuration.NeuronAmounts[0], network.Layers[0].Neurons.Count);
            Assert.Equal(configuration.NeuronAmounts[1], network.Layers[1].Neurons.Count);
            Assert.Equal(configuration.NeuronAmounts[2], network.Layers[2].Neurons.Count);
            Assert.Equal(configuration.NeuronAmounts[1], network.Layers[0].Neurons[0].Weights.Count);
            Assert.Equal(configuration.NeuronAmounts[1], network.Layers[0].Neurons[1].Weights.Count);
            Assert.Equal(configuration.NeuronAmounts[2], network.Layers[1].Neurons[0].Weights.Count);
            Assert.Null(network.Layers[2].Neurons[0].Weights);
        }

        [Fact]
        public void InsertHiddenLayer_ShouldInsertCorrectly()
        {
            NetworkConfiguration configuration = new NetworkConfiguration
            {
                NumberOfLayers = 3,
                NeuronAmounts = new int[] { 3, 4, 5 },
            };

            Network network = new(configuration);

            network.InsertHiddenLayer(2, 7);

            // { 3, 4, 7, 5 }
            Assert.Equal(4, network.Layers.Count);
            Assert.Equal(7, network.Layers[2].Neurons.Count);
            Assert.Equal(4, network.Layers[1].Neurons.Count);
            Assert.Equal(5, network.Layers[3].Neurons.Count);
            Assert.Equal(7, network.Layers[1].Neurons[0].Weights.Count);
            Assert.Equal(7, network.Layers[1].Neurons[1].Weights.Count);
            Assert.Equal(5, network.Layers[2].Neurons[0].Weights.Count);
            Assert.Equal(5, network.Layers[2].Neurons[6].Weights.Count);
            Assert.Null(network.Layers[3].Neurons[0].Weights);
            Assert.Null(network.Layers[3].Neurons[4].Weights);
        }

        [Fact]
        public void RemoveHiddenLayer_ShouldRemoveCorrectly()
        {
            NetworkConfiguration configuration = new NetworkConfiguration
            {
                NumberOfLayers = 4,
                NeuronAmounts = new int[] { 3, 4, 7, 5 },
            };

            Network network = new(configuration);

            network.RemoveHiddenLayer(2);

            // { 3, 4, 5 }
            Assert.Equal(3, network.Layers.Count);
            Assert.Equal(4, network.Layers[1].Neurons.Count);
            Assert.Equal(5, network.Layers[2].Neurons.Count);
            Assert.Equal(5, network.Layers[1].Neurons[0].Weights.Count);
            Assert.Equal(5, network.Layers[1].Neurons[3].Weights.Count);
            Assert.Null(network.Layers[2].Neurons[0].Weights);
            Assert.Null(network.Layers[2].Neurons[4].Weights);
        }

        [Fact]
        public void ChangeLayerNeuronAmount_ShouldChangeCorrectlyForHidden()
        {
            NetworkConfiguration configuration = new NetworkConfiguration
            {
                NumberOfLayers = 4,
                NeuronAmounts = new int[] { 3, 4, 7, 5 },
            };

            Network network = new(configuration);

            network.ChangeLayerNeuronAmount(2, 3);

            // { 3, 4, 3, 5 }
            Assert.Equal(4, network.Layers.Count);
            Assert.Equal(3, network.Layers[2].Neurons.Count);
            Assert.Equal(4, network.Layers[1].Neurons.Count);
            Assert.Equal(5, network.Layers[3].Neurons.Count);
            Assert.Equal(5, network.Layers[2].Neurons[0].Weights.Count);
            Assert.Equal(5, network.Layers[2].Neurons[2].Weights.Count);
            Assert.Equal(3, network.Layers[1].Neurons[0].Weights.Count);
            Assert.Equal(3, network.Layers[1].Neurons[3].Weights.Count);
        }

        [Fact]
        public void ChangeLayerNeuronAmount_ShouldChangeCorrectlyForOutput()
        {
            NetworkConfiguration configuration = new NetworkConfiguration
            {
                NumberOfLayers = 4,
                NeuronAmounts = new int[] { 3, 4, 3, 5 },
            };

            Network network = new(configuration);

            network.ChangeLayerNeuronAmount(3, 2);

            // { 3, 4, 3, 2 }
            Assert.Equal(4, network.Layers.Count);
            Assert.Equal(2, network.Layers[3].Neurons.Count);
            Assert.Equal(3, network.Layers[2].Neurons.Count);
            Assert.Equal(2, network.Layers[2].Neurons[0].Weights.Count);
            Assert.Equal(2, network.Layers[2].Neurons[2].Weights.Count);
            Assert.Null(network.Layers[3].Neurons[0].Weights);
            Assert.Null(network.Layers[3].Neurons[1].Weights);
        }
    }
}
