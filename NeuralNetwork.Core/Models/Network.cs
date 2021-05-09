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

        public Network(NetworkConfiguration configuration)
        {
            // input and hidden layers
            for (int i = 0; i < configuration.NumberOfLayers - 1; i++)
            {
                Layers.Add(new());
                for (int j = 0; j < configuration.NeuronAmounts[i]; j++)
                {
                    Layers[i].Neurons.Add(new());
                    for (int k = 0; k < configuration.NeuronAmounts[i + 1]; k++)
                    {
                        Layers[i].Neurons[j].Weights.Add(0);
                    }
                }
            }

            // output layer
            Layers.Add(new());
            for (int j = 0; j < configuration.NeuronAmounts[configuration.NumberOfLayers - 1]; j++)
            {
                Layers[configuration.NumberOfLayers - 1].Neurons.Add(new());
                Layers[configuration.NumberOfLayers - 1].Neurons[j].Weights = null;
            }
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

        private Layer CreateNewLayer(int amountOfNeurons, int amountOfWeights)
        {
            Layer layer = new();
            layer.Neurons = new();
            for (int i = 0; i < amountOfNeurons; i++)
            {
                layer.Neurons.Add(new());
                for (int j = 0; j < amountOfWeights; j++)
                {
                    layer.Neurons[i].Weights.Add(0);
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
