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
    }
}
