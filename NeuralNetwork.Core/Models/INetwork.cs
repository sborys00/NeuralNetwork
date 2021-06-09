using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.Core.Models
{
    public interface INetwork
    {
        public List<Layer> Layers { get; set; }
        public void InsertHiddenLayer(int index, int amountOfNeurons);
        public void RemoveHiddenLayer(int index);
        public void ChangeLayerNeuronAmount(int index, int amountOfNeurons);
        public void AddNeuronToLayer(int index);
        public void RemoveNeuronFromLayer(int index);
        public void InitializeWeights();
        public (List<List<double>> outputs, List<List<double>> inputs) CalculateOutput(double[] input, Func<double, double> activationFunction);
    }
}
