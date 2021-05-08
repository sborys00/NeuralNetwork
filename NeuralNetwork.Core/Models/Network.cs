using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.Core.Models
{
    public class Network
    {
        public Layer[] Layers { get; set; }

        public Network(NetworkConfiguration configuration)
        {
            Layers = new Layer[configuration.NumberOfLayers];

            // input and hidden layers
            for (int i = 0; i < configuration.NumberOfLayers - 1; i++)
            {
                Layers[i] = new Layer 
                {
                    Neurons = new Neuron[configuration.NeuronAmounts[i]],
                };
                for (int j = 0; j < configuration.NeuronAmounts[i]; j++)
                {
                    Layers[i].Neurons[j] = new Neuron
                    {
                        Value = 0,
                        Weights = new float[configuration.NeuronAmounts[i + 1]],
                    };
                }
            }

            // output layer
            Layers[configuration.NumberOfLayers - 1] = new Layer
            {
                Neurons = new Neuron[configuration.NeuronAmounts[configuration.NumberOfLayers - 1]],
            };
            for (int j = 0; j < configuration.NeuronAmounts[configuration.NumberOfLayers - 1]; j++)
            {
                Layers[configuration.NumberOfLayers - 1].Neurons[j] = new Neuron
                {
                    Value = 0,
                    Weights = null,
                };
            }
        }
    }
}
