﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeuralNetwork.Core.ExtensionMethods;

namespace NeuralNetwork.Core.Models
{
    public class Network
    {
        public List<Layer> Layers { get; set; } = new();
        public double LearningRate { get; set; } 
        public Func<double, double> ActivationFunction { get; set; }

        public Network(NetworkConfiguration configuration)
        {
            // input and hidden layers
            for (int i = 0; i < configuration.NumberOfLayers - 1; i++)
            {
                Layers.Add(NetworkBuilder.CreateNewLayer(configuration.NeuronAmounts[i], configuration.NeuronAmounts[i + 1]));
            }

            // output layer
            Layers.Add(NetworkBuilder.CreateNewLayer(configuration.NeuronAmounts[configuration.NumberOfLayers - 1], null));
        }

        public Network()
        {
        }

        // disrupts learned weights - needs initializing afterwards
        public void InsertHiddenLayer(int index, int amountOfNeurons)
        {
            if (index < 1)
                throw new IndexOutOfRangeException("Cannot insert hidden layer at input layer index or below.");

            if (index > Layers.Count - 1)
                throw new IndexOutOfRangeException("Cannot insert hidden layer at output layer index or after.");

            Layer layer = NetworkBuilder.CreateNewLayer(amountOfNeurons, Layers[index].Neurons.Count);
            NetworkBuilder.AdaptWeights(Layers[index - 1], amountOfNeurons);
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
            
            NetworkBuilder.AdaptWeights(Layers[index - 1], Layers[index].Neurons.Count);
        }


        public void ChangeLayerNeuronAmount(int index, int amountOfNeurons)
        {
            if (index < 0 || index > Layers.Count - 1)
                throw new IndexOutOfRangeException();

            if (index == Layers.Count - 1)
                NetworkBuilder.EditLayer(Layers[index], amountOfNeurons, null);
            else
                NetworkBuilder.EditLayer(Layers[index], amountOfNeurons, Layers[index + 1].Neurons.Count);

            NetworkBuilder.AdaptWeights(Layers[index-1], amountOfNeurons);
        }

        /// <summary>
        /// Sets initial weight values in all layers using Normalized Xavier Weight Initialization
        /// </summary>
        public void InitializeWeights()
        {
            for (int i = 0; i < Layers.Count - 1; i++)
            {
                InitializeWeightsForLayer(i);
            }
        }

        /// <summary>
        /// Create a matrix of weights of all neurons in the given Layer object.
        /// </summary>
        /// <param name="layer">Layer of which the weights will be in the martix</param>
        /// <returns>A matrix of double values representing weights of connections from preceding layer to the given one</returns>
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
        
        public double[] CalculateErrorForOutputLayer(double[] output, double[] target)
        {
            if (output.Length != target.Length)
                throw new Exception("Output array does not match size of the target array");

            double[] errors = new double[output.Length];
            for (int i = 0; i < output.Length; i++)
            {
                double diff = output[i] - target[i];
                errors[i] = diff * diff;
            }
            return errors;
        }
        private void InitializeWeightsForLayer(int index)
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

        private void InitializeWeightsForLayer(Layer layer, double upperBound, double lowerBound)
        {
            Random random = new();
            for (int i = 0; i < layer.Neurons.Count; i++)
            {
                for (int j = 0; j < layer.Neurons[i].Weights.Count; j++)
                {
                    layer.Neurons[i].Weights[j] = random.GetRandomDoubleBetween(upperBound, lowerBound);
                }
            }
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

    }
}
