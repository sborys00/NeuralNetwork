using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.Core.Models
{
    /// <summary>
    /// Used to create an Network object. It follows Fluent Builder design pattern. 
    /// </summary>
    public class NetworkBuilder
    {
        private readonly Network _network = new Network();
        /// <summary>
        /// Adds new layers to a neural network, each of them having amount of neurons specified in params.
        /// </summary>
        /// <param name="neuronsInLayer">Comma separted integers or an array of ints.</param>
        /// <returns></returns>
        public NetworkBuilder AddLayers(params int[] neuronsInLayer)
        {
            // input and hidden layers
            for (int i = 0; i < neuronsInLayer.Length - 1; i++)
            {
                _network.Layers.Add(NetworkBuilder.CreateNewLayer(neuronsInLayer[i], neuronsInLayer[i + 1]));
            }

            // output layer
            _network.Layers.Add(NetworkBuilder.CreateNewLayer(neuronsInLayer[^1], null));

            return this;
        }

        public NetworkBuilder SetLearningRate(double lr)
        {
            _network.LearningRate = lr;
            return this;
        }

        public NetworkBuilder SetActivationFunction(Func<double, double> af)
        {
            _network.ActivationFunction = af;
            return this;
        }

        public Network Build()
        {
            return _network;
        }
        
        /// <summary>
        /// Creates a layer with specified amount of neurons and amount of weights for every neuron.
        /// </summary>
        /// <param name="amountOfNeurons"></param>
        /// <param name="amountOfWeights"></param>
        /// <returns>Created layer</returns>
        public static Layer CreateNewLayer(int amountOfNeurons, int? amountOfWeights)
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

        /// <summary>
        /// Changes given layer's structure to specified amount of neurons and amount of weights for every neuron.
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="amountOfNeurons"></param>
        /// <param name="amountOfWeights"></param>
        public static void EditLayer(Layer layer, int amountOfNeurons, int? amountOfWeights)
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

        /// <summary>
        /// Changes given layer's amount of weights for each neuron.
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="amountOfWeights"></param>
        public static void AdaptWeights(Layer layer, int amountOfWeights)
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
