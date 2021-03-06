using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.Core.Models
{
    public class Layer
    {
        public List<Neuron> Neurons { get; set; } = new();

        // to refactor
        public void CalculateOutputs(double[] input, double[,] previousWeights, Func<double, double> activationFunction, out double[] output, out double[] rawOutput, out double[,] currentWeights)
        {
            int nextLayerNeuronCount = 0;
            if (Neurons[0].Weights == null)
            {
                currentWeights = null;
            }
            else
            {
                nextLayerNeuronCount = Neurons[0].Weights.Count;
                currentWeights = new double[Neurons.Count, nextLayerNeuronCount];
            }
            output = new double[Neurons.Count];
            rawOutput = new double[Neurons.Count];
            for (int i = 0; i < Neurons.Count; i++)
            {
                var currentNeuronEnteringWeights = Enumerable.Range(0, previousWeights.GetLength(0)).Select(x => previousWeights[x, i]).ToArray();
                output[i] = Neuron.CalculateOutput(input, currentNeuronEnteringWeights);
                rawOutput[i] = output[i];
                output[i] = activationFunction(output[i]);

                if (currentWeights != null)
                {
                    for (int j = 0; j < nextLayerNeuronCount; j++)
                    {
                        currentWeights[i, j] = Neurons[i].Weights[j];
                    }
                }
            }
        }
        public void UpdateWeights(double[][] newWeights)
        {
            for (int i = 0; i < newWeights.Length; i++)
            {
                Neurons[i].Weights = newWeights[i].ToList();
            }
        }
        public void AddDeltaWeights(double[][] deltas)
        {
            for (int i = 0; i < deltas.Length; i++)
            {
                for (int j = 0; j < deltas[i].Length; j++)
                {
                    Neurons[i].Weights[j] += deltas[i][j];
                }
            }
        }
    }
}
