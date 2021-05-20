using NeuralNetwork.Core.Models;
using System;
using Xunit;

namespace NeuralNetwork.Core.Tests
{
    public class LearningManagerTests
    {
        [Fact]
        public void RunOneTest_ShouldReturnCorrectValues()
        {
            LearningManager learningManager = new();
            learningManager.ActivationFunction = new SigmoidActivationFunction();

            NetworkBuilder nb = new();
            Network network = nb.AddLayers(2, 2, 2).Build();

            double[] inputValues = new double[] {2.0, 1.5};
            double[] expectedOutput = new double[] {0.3, 0.5};
            TrainingDataUnit test = new(inputValues, expectedOutput);

            TestResult result = learningManager.RunOneTest(network, test);
            Assert.Equal(expectedOutput[0], result.expectedValues[0]);
            Assert.Equal(expectedOutput[1], result.expectedValues[1]);
        }

        [Fact]
        public void CalculateErrorForOutputLayer_ShouldReturnCorrectValues()
        {
            LearningManager lm = new();
            NetworkBuilder networkBuilder = new();

            Network network = networkBuilder.AddLayers(2, 2).Build();
            double[] outputs = { 0.6, 0.4, 0.2, 0, 0.1 };
            double[] targets = { 0.4, 0.2, 0.2, 0.6, 1 };
            double[] actual = lm.CalculateErrorForOutputLayer(outputs, targets);
            double[] expected = new double[outputs.Length];
            for (int i = 0; i < expected.Length; i++)
            {
                expected[i] = outputs[i] - targets[i];
            }
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void RunBackpropagation_ShouldCalculateCorrectValues()
        {
            int[] neuronAmounts = new int[] { 3, 4, 3 };
            NetworkBuilder networkBuilder = new();

            Network network = networkBuilder.AddLayers(neuronAmounts).Build();
            LearningManager learningManager = new();
            learningManager.LearningRate = 0.1;
            learningManager.ActivationFunction = new SigmoidActivationFunction();
            learningManager.TrainingSet = new() { new TrainingDataUnit(
                new double[] { 0.1, 0.3, 0.5},
                new double[] { 0.4, 0.2, 0.6}
            )};
            learningManager.RunBackPropagation(network);
        }
    }
}
