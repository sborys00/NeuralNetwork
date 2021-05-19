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
            int[] neuronAmounts = new int[] { 4, 3, 5 };
            NetworkBuilder networkBuilder = new();
            Network network = networkBuilder.AddLayers(neuronAmounts).Build();

            network.Layers[0].Neurons.ForEach(n => n.Weights = new List<double>(new double[] { .1, .2, .3 }));
            network.Layers[1].Neurons.ForEach(n => n.Weights = new List<double>(new double[] { .4, .5, .6, .7, .8 }));

            double[] expectedOutput = { 1.6, 3.2, 4.8 };
            network.Layers[1].CalculateOutputs(new double[] { 5, 6, 2, 3 }, Network.GetWeightsFromLayer(network.Layers[0]), x => x, out double[] actualOutput, out double[,] currentWeights);

            Assert.Equal(expectedOutput, actualOutput);
        }

        [Fact]
        public void UpdateWeights_SouldWork()
        {
            Layer layer = new();
            layer.Neurons.Add(new Neuron());
            layer.Neurons.Add(new Neuron());
            layer.Neurons.Add(new Neuron());
            layer.Neurons.Add(new Neuron());

            List<List<double>> newWeights = new() {
                new List<double>() { 0.2, 0.3, 0.4 },
                new List<double>() { 0.5, 1.0, 0.1},
                new List<double>() { 0.6, 0.2, 0.3},
            };
            layer.UpdateWeights(newWeights);

            Assert.Equal(0.2, layer.Neurons[0].Weights[0]);
            Assert.Equal(0.3, layer.Neurons[0].Weights[1]);
            Assert.Equal(0.4, layer.Neurons[0].Weights[2]);

            Assert.Equal(0.5, layer.Neurons[1].Weights[0]);
            Assert.Equal(1.0, layer.Neurons[1].Weights[1]);
            Assert.Equal(0.1, layer.Neurons[1].Weights[2]);

            Assert.Equal(0.6, layer.Neurons[2].Weights[0]);
            Assert.Equal(0.2, layer.Neurons[2].Weights[1]);
            Assert.Equal(0.3, layer.Neurons[2].Weights[2]);
        }
    }
}
