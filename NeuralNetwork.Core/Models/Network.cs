using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.Core.Models
{
    public class Network
    {
        public List<Layer> Layers { get; set; } = new();
        public double LearningRate { get; set; } 

        public Network(NetworkConfiguration configuration)
        {
            // input and hidden layers
            for (int i = 0; i < configuration.NumberOfLayers - 1; i++)
            {
                Layers.Add(CreateNewLayer(configuration.NeuronAmounts[i], configuration.NeuronAmounts[i + 1]));
            }

            // output layer
            Layers.Add(CreateNewLayer(configuration.NeuronAmounts[configuration.NumberOfLayers - 1], null));
        }

        // disrupts learned weights - needs initializing afterwards
        public void InsertHiddenLayer(int index, int amountOfNeurons)
        {
            if (index < 1)
                throw new IndexOutOfRangeException("Cannot insert hidden layer at input layer index or below.");

            if (index > Layers.Count - 1)
                throw new IndexOutOfRangeException("Cannot insert hidden layer at output layer index or after.");

            Layer layer = CreateNewLayer(amountOfNeurons, Layers[index].Neurons.Count);
            AdaptWeights(Layers[index - 1], amountOfNeurons);
            Layers.Insert(index, layer);
        }

        public void RemoveHiddenLayer(int index)
        {
            if (index < 1)
                throw new IndexOutOfRangeException("Cannot remove input layer or any non-existing layer below.");

            if (index > Layers.Count - 2)
                throw new IndexOutOfRangeException("Cannot remove output layer or any non-existing layer after.");

            Layers.RemoveAt(index);

            if (Layers[index].Neurons.Count == Layers[index - 1].Neurons[0].Weights.Count)
                return;
            
            AdaptWeights(Layers[index - 1], Layers[index].Neurons.Count);
        }

        public void ChangeLayerNeuronAmount(int index, int amountOfNeurons)
        {
            if (index < 0 || index > Layers.Count - 1)
                throw new IndexOutOfRangeException();

            if (index == Layers.Count - 1)
                EditLayer(Layers[index], amountOfNeurons, null);
            else
                EditLayer(Layers[index], amountOfNeurons, Layers[index + 1].Neurons.Count);

            AdaptWeights(Layers[index-1], amountOfNeurons);
        }

        public void InitializeWeights()
        {
            for (int i = 0; i < Layers.Count - 1; i++)
            {
                InitializeWeightsForLayer(i);
            }
        }

        public void InitializeWeightsForLayer(int index)
        {
            if (index < 0)
                throw new IndexOutOfRangeException();

            if (index > Layers.Count - 2)
                throw new IndexOutOfRangeException();

            // Uses Normalized Xavier Weight Initialization
            CalculateWeightBoundsForLayer(index, out double upperBound, out double lowerBound);
            Layer layer = Layers[index];
            InitializeWeightsForLayer(layer, upperBound, lowerBound);
        }

        public static double[,] GetWeightsFromLayer(Layer layer)
        {
            int nextLayerNeuronAmount = layer.Neurons[0].Weights.Count;
            double[,] weights = new double[layer.Neurons.Count, nextLayerNeuronAmount];
            for (int i = 0; i < layer.Neurons.Count; i++)
            {
                for (int j = 0; j < nextLayerNeuronAmount; j++)
                {
                    weights[i, j] = layer.Neurons[i].Weights[j];
                }
            }
            return weights;
        }

        public List<List<double>> CalculateOutput(double[] input, Func<double, double> activationFunction)
        {
            if (input.Length != Layers[0].Neurons.Count)
                throw new Exception("Input must contain same amount of nodes as the first layer.");

            double[,] previousWeights = GetWeightsFromLayer(Layers[0]);
            List<List<double>> inputHistory = new();
            for (int i = 1; i < Layers.Count; i++)
            {
                inputHistory.Add(input.ToList());

                Layers[i].CalculateOutputs(input, previousWeights, activationFunction, out double[] output, out double[,] currentWeights);
                
                input = output;
                previousWeights = currentWeights;
            }
            inputHistory.Add(input.ToList());
            return inputHistory;
        }

        private void InitializeWeightsForLayer(Layer layer, double upperBound, double lowerBound)
        {
            Random random = new();
            for (int i = 0; i < layer.Neurons.Count; i++)
            {
                for (int j = 0; j < layer.Neurons[i].Weights.Count; j++)
                {
                    layer.Neurons[i].Weights[j] = GetRandomDoubleBetween(upperBound, lowerBound, random);
                }
            }
        }

        private double GetRandomDoubleBetween(double upperBound, double lowerBound, Random random)
        {
            return lowerBound + (random.NextDouble() * (upperBound - lowerBound));
        }

        private void CalculateWeightBoundsForLayer(int index, out double upperBound, out double lowerBound)
        {
            int lastLayerNodeAmount = 0;
            if (index != 0)
                lastLayerNodeAmount = Layers[index - 1].Neurons.Count;

            int nextLayerNodeAmount = Layers[index + 1].Neurons.Count;

            CalculateWeightBounds(lastLayerNodeAmount, nextLayerNodeAmount, out upperBound, out lowerBound);
        }

        private void CalculateWeightBounds(int lastLayerNodeAmount, int nextLayerNodeAmount, out double upperBound, out double lowerBound)
        {
            upperBound = Math.Sqrt(6.0) / Math.Sqrt(lastLayerNodeAmount + nextLayerNodeAmount);
            lowerBound = -upperBound;
        }

        private Layer CreateNewLayer(int amountOfNeurons, int? amountOfWeights)
        {
            Layer layer = new();
            layer.Neurons = new();

            if (amountOfWeights == null)
            {
                for (int i = 0; i < amountOfNeurons; i++)
                {
                    layer.Neurons.Add(new());
                    layer.Neurons[i].Weights = null;
                }
            }
            else
            {
                for (int i = 0; i < amountOfNeurons; i++)
                {
                    layer.Neurons.Add(new());
                    for (int j = 0; j < amountOfWeights; j++)
                    {
                        layer.Neurons[i].Weights.Add(0);
                    }
                }
            }
            return layer;
        }

        private void EditLayer(Layer layer, int amountOfNeurons, int? amountOfWeights)
        {
            layer.Neurons = new();
            for (int i = 0; i < amountOfNeurons; i++)
            {
                layer.Neurons.Add(new());
            }

            if (amountOfWeights == null)
            {
                for (int i = 0; i < amountOfNeurons; i++)
                {
                    layer.Neurons[i].Weights = null;
                }
            }
            else
            {
                for (int i = 0; i < amountOfNeurons; i++)
                {
                    for (int j = 0; j < amountOfWeights; j++)
                    {
                        layer.Neurons[i].Weights.Add(0);
                    }
                }
            }
        }

        private void AdaptWeights(Layer layer, int amountOfWeights)
        {
            for (int i = 0; i < layer.Neurons.Count; i++)
            {
                layer.Neurons[i].Weights = new();
                for (int j = 0; j < amountOfWeights; j++)
                {
                    layer.Neurons[i].Weights.Add(0);
                }
            }
        }


    }
}
