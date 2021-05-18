using System;

namespace NeuralNetwork.Core.Models
{
    public interface ILearningManager
    {
        Func<double, double> ActivationFunction { get; set; }
        int Epoch { get; }
        double LearningRate { get; set; }
    }
}