using NeuralNetwork.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NeuralNetwork.Core.Tests
{
    public class NeuronTests
    {
        [Fact]
        public void CalculateOutput_ShouldWork()
        {
            int[] neuronAmounts = new int[] { 4, 3, 5 };
            NetworkBuilder networkBuilder = new();
            Network network = networkBuilder.AddLayers(neuronAmounts).Build();

            network.Layers[0].Neurons.ForEach(n => n.Weights = new List<double>(new double[] { .1, .2, .3, .4 }));
            network.Layers[1].Neurons.ForEach(n => n.Weights = new List<double>(new double[] { .4, .5, .6, .7, .8 }));

            double expectedOutput = 12.5;
            double actualOutput = Neuron.CalculateOutput(new double[] { 5, 6, 2, 3 }, new double[] { .9, .8, .7, .6 });

            Assert.Equal(expectedOutput, actualOutput);
        }
    }
}
