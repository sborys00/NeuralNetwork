using NeuralNetwork.Core.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
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
            TrainingDataExample test = new(inputValues, expectedOutput);

            TestResult result = learningManager.RunOneExample(network, test);
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
                expected[i] = targets[i] - outputs[i];
            }
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void RunBackpropagation_ShouldImproveNetworkAccuracy()
        {
            int[] neuronAmounts = new int[] { 3, 4, 3 };
            NetworkBuilder networkBuilder = new();

            Network network = networkBuilder.AddLayers(neuronAmounts).Build();
            LearningManager learningManager = new();
            learningManager.LearningRate = 0.3;
            learningManager.ActivationFunction = new SigmoidActivationFunction();
            learningManager.TrainingSet = new() { 
                new TrainingDataExample(
                    new double[] { 1.0, 0.3, 0.4},
                    new double[] { 1.0, 0.0, 0.0}
                    ),
                new TrainingDataExample(
                    new double[] { 0.2, 0.9, 0.1 },
                    new double[] { 0.0, 1.0, 0.0 }
                    ),
                new TrainingDataExample(
                    new double[] { 0.0, 0.0, 0.7 },
                    new double[] { 0.0, 0.0, 1.0 }
                    ),
                new TrainingDataExample(
                    new double[] { 1.0, 0.0, 0.0 },
                    new double[] { 1.0, 0.0, 0.0 }
                    ),
                new TrainingDataExample(
                    new double[] { 0.0, 1.0, 0.0 },
                    new double[] { 0.0, 1.0, 0.0 }
                    ),
                new TrainingDataExample(
                    new double[] { 0.0, 0.0, 1.0 },
                    new double[] { 0.0, 0.0, 1.0 }
                    ),
                new TrainingDataExample(
                    new double[] { 0.0, 0.5, 0.0 },
                    new double[] { 0.0, 1.0, 0.0 }
                    ),
                new TrainingDataExample(
                    new double[] { 0.0, 0.0, 0.8 },
                    new double[] { 0.0, 0.0, 1.0 }
                    ),
            };
            string text = JsonSerializer.Serialize(learningManager.TrainingSet);
            learningManager.TrainingSet = JsonSerializer.Deserialize<List<TrainingDataExample>>(text);
            double avgError = 1;
            for (int i = 0; i < 5; i++)
            {
                double oldAvgError = avgError;
                for (int j = 0; j < 5; j++)
                {
                    avgError = learningManager.RunBackPropagation(network, learningManager.RunAllExamples(network, learningManager.TrainingSet).ToArray());
                }
                Assert.True(avgError < oldAvgError);
            }
        }
    }
}
