using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using NeuralNetwork.Core.Models;

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
                expected[i] = Math.Pow((outputs[i] - targets[i]), 2);
            }
            Assert.Equal(expected, actual);
        }
    }
}
