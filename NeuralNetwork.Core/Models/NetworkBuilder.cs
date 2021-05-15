using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.Core.Models
{
    public class NetworkBuilder
    {
        /// <summary>
        /// Creates a new layer with specified amount of neurons and amount of weights for every neuron.
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
