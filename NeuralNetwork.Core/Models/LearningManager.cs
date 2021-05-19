using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.Core.Models
{
    public class LearningManager : ILearningManager
    {
        public int Epoch { get; }

        /// <summary>
        /// Should be between 0.0 to 1.0.
        /// </summary>
        public double LearningRate { get; set; }
        public ActivationFuntion ActivationFunction { get; set; }
        public List<TrainingDataUnit> TrainingSet { get; set; }
        public List<TrainingDataUnit> TestSet { get; set; }

        public Network Network { get; set; }

        /// <summary>
        /// Runs all tests in TestSet property
        /// </summary>
        /// <param name="network">Instance of Network to be tested</param>
        /// <returns></returns>
        public List<TestResult> RunAllTests(Network network)
        {
            List<TestResult> results = new();
            foreach(var test in TestSet)
            {
                results.Add(RunOneTest(network, test));
            }
            return results;
        }

        /// <summary>
        /// Runs one test and returns the result as TestResult object.
        /// </summary>
        /// <param name="network">Network instance to be tested</param>
        /// <param name="test">Unit of training data</param>
        /// <returns></returns>
        public TestResult RunOneTest(Network network, TrainingDataUnit test)
        {
            var networkOutput = network.CalculateOutput(test.inputValues, this.ActivationFunction.Function).Last();
            if (networkOutput.Count != test.expectedOutputs.Length)
                throw new Exception("Number of outputs does not match test data");

            int len = test.expectedOutputs.Length;

            double[] expected = new double[len];
            double[] actual = new double[len];

            for (int i = 0; i < len; i++)
            {
                expected[i] = test.expectedOutputs[i];
                actual[i] = networkOutput[i];
            }
            return new TestResult(expected, actual);
        }

        public void TrainForMultipleEpochs(int numberOfEpochs)
        {
            throw new NotImplementedException();
        }

        public void TrainForOneEpoch()
        {
            throw new NotImplementedException();
        }
    }
}
