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
            int[] NeuronAmounts = new int[] { 3, 4, 5 };

            NetworkBuilder networkBuilder = new();

            Network network = networkBuilder.AddLayers(NeuronAmounts).Build();

            Assert.Equal(NeuronAmounts.Length, network.Layers.Count);
            Assert.Equal(NeuronAmounts[0], network.Layers[0].Neurons.Count);
            Assert.Equal(NeuronAmounts[1], network.Layers[1].Neurons.Count);
            Assert.Equal(NeuronAmounts[2], network.Layers[2].Neurons.Count);
            Assert.Equal(NeuronAmounts[1], network.Layers[0].Neurons[0].Weights.Count);
            Assert.Equal(NeuronAmounts[1], network.Layers[0].Neurons[1].Weights.Count);
            Assert.Equal(NeuronAmounts[2], network.Layers[1].Neurons[0].Weights.Count);
            Assert.Null(network.Layers[2].Neurons[0].Weights);
        }

        [Fact]
        public void InsertHiddenLayer_ShouldInsertCorrectly()
        {
            int[] neuronAmounts = new int[] { 3, 4, 5 };

            NetworkBuilder networkBuilder = new();

            Network network = networkBuilder.AddLayers(neuronAmounts).Build();

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

            int[] neuronAmounts = new int[] { 3, 4, 7, 5 };
            NetworkBuilder networkBuilder = new();
            Network network = networkBuilder.AddLayers(neuronAmounts).Build();

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
            int[] neuronAmounts = new int[] { 3, 4, 7, 5 };
            NetworkBuilder networkBuilder = new();

            Network network = networkBuilder.AddLayers(neuronAmounts).Build();

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
            int[] neuronAmounts = new int[] { 3, 4, 3, 5 };
            NetworkBuilder networkBuilder = new();

            Network network = networkBuilder.AddLayers(neuronAmounts).Build();

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

        [Fact]
        public void InitializeWeightsForLayer_AllShouldBeInBounds()
        {
            int[] neuronAmounts = new int[] { 3, 4, 7, 5 };
            NetworkBuilder networkBuilder = new();

            Network network = networkBuilder.AddLayers(neuronAmounts).Build();

            double upperBound = Math.Sqrt(6.0) / Math.Sqrt(3 + 7);
            double lowerBound = -upperBound;

            network.InitializeWeights();

            for (int i = 0; i < 7; i++)
            {
                Assert.InRange(network.Layers[1].Neurons[0].Weights[i], lowerBound, upperBound);
                Assert.InRange(network.Layers[1].Neurons[1].Weights[i], lowerBound, upperBound);
                Assert.InRange(network.Layers[1].Neurons[2].Weights[i], lowerBound, upperBound);
                Assert.InRange(network.Layers[1].Neurons[3].Weights[i], lowerBound, upperBound);
            }
        }

        [Fact]
        public void InitializeWeights_AllShouldBeInBound()
        {
            int[] neuronAmounts = new int[] { 3, 4, 7, 5 };
            NetworkBuilder networkBuilder = new();

            Network network = networkBuilder.AddLayers(neuronAmounts).Build();

            network.InitializeWeights();

            double[] upperBounds = 
            {
                Math.Sqrt(6.0) / Math.Sqrt(0 + 4),
                Math.Sqrt(6.0) / Math.Sqrt(3 + 7),
                Math.Sqrt(6.0) / Math.Sqrt(4 + 5),
            };

            double[] lowerBounds =
            {
                -upperBounds[0],
                -upperBounds[1],
                -upperBounds[2],
            };

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Assert.InRange(network.Layers[0].Neurons[i].Weights[j], lowerBounds[0], upperBounds[0]);
                }
            }

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    Assert.InRange(network.Layers[1].Neurons[i].Weights[j], lowerBounds[1], upperBounds[1]);
                }
            }

            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Assert.InRange(network.Layers[2].Neurons[i].Weights[j], lowerBounds[2], upperBounds[2]);
                }
            }
        }

        [Fact]
        public void GetWeightsFromLayer_ShouldWork()
        {
            int[] neuronAmounts = new int[] { 3, 4, 7 };
            NetworkBuilder networkBuilder = new();

            Network network = networkBuilder.AddLayers(neuronAmounts).Build();

            network.Layers[0].Neurons.ForEach(n => n.Weights = new List<double>(new double[] { .1, .2, .3, .4 }));
            network.Layers[1].Neurons.ForEach(n => n.Weights = new List<double>(new double[] { .4, .5, .6, .7, .8, .9, .8 }));

            double[,] weights0 = Network.GetWeightsFromLayer(network.Layers[0]);
            double[,] weights1 = Network.GetWeightsFromLayer(network.Layers[1]);

            Assert.Equal(.2, weights0[0, 1]);
            Assert.Equal(.3, weights0[0, 2]);
            Assert.Equal(.3, weights0[1, 2]);
            Assert.Equal(.5, weights1[1, 1]);
            Assert.Equal(.6, weights1[2, 2]);
            Assert.Equal(.7, weights1[3, 3]);
        }

        [Fact]
        public void CalculateOutput_ShouldWork()
        {
            int[] neuronAmounts = new int[] { 3, 4, 2 };
            NetworkBuilder networkBuilder = new();

            Network network = networkBuilder.AddLayers(neuronAmounts).Build();

            network.Layers[0].Neurons.ForEach(n => n.Weights = new List<double>(new double[] { .1, .2, .3, .4 }));
            network.Layers[1].Neurons.ForEach(n => n.Weights = new List<double>(new double[] { .4, .5 }));

            /*
             *     2.4 3
             * .6 1.2 1.8 2.4
             *    1   2   3
             */

            List<List<double>> output = network.CalculateOutput(new double[] { 1, 2, 3 }, x => x).outputs;

            Assert.Equal(new double[] { 2.4, 3 }, output.Last().Select(x => Math.Round(x, 2)));
        }

        [Fact]
        public void CalculateOutput_ShouldThrowExceptionWhenWrongInput()
        {
            int[] neuronAmounts = new int[] { 3, 4, 7 };
            NetworkBuilder networkBuilder = new();

            Network network = networkBuilder.AddLayers(neuronAmounts).Build();
            network.Layers[0].Neurons.ForEach(n => n.Weights = new List<double>(new double[] { .1, .2, .3 }));
            network.Layers[1].Neurons.ForEach(n => n.Weights = new List<double>(new double[] { .4, .5, .6, .7 }));

            Assert.Throws<Exception>(delegate { network.CalculateOutput(new double[] { 1, 2, 3, 4 }, x => x); });
        }
    }
}
