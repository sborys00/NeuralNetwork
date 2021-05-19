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
            foreach (var test in TestSet)
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
            var networkOutputs = network.CalculateOutput(test.inputValues, this.ActivationFunction.Function);
            var lastLayer = networkOutputs.Last();
            if (lastLayer.Count != test.expectedOutputs.Length)
                throw new Exception("Number of outputs does not match test data");

            int len = test.expectedOutputs.Length;

            double[] expected = new double[len];
            double[] actual = new double[len];

            for (int i = 0; i < len; i++)
            {
                expected[i] = test.expectedOutputs[i];
                actual[i] = lastLayer[i];
            }
            return new TestResult(expected, actual, networkOutputs);
        }

        public void RunBackPropagation(Network network)
        {
            foreach (var test in TrainingSet)
            {
                TestResult result = RunOneTest(network, test);
                double[] errors = CalculateErrorForOutputLayer(result.actualValues, result.expectedValues);
                double[] errorAndDerivativeProducts = CalculateErrorAndDerivativeProducts(errors, result.actualValues);
                Layer outputLayer = network.Layers.Last();
                for (int i = 0; i < outputLayer.Neurons.Count; i++)
                {
                    Neuron neuron = outputLayer.Neurons[i];
                    for (int j = 0; j < neuron.Weights.Count; j++)
                    {
                        double deltaWeight = 2 * LearningRate * errorAndDerivativeProducts[i];
                        //deltaWeight *= result.networkOutputs[^2][i];
                    }
                }
            }
            //TODO

        }

        public double[] CalculateErrorForOutputLayer(double[] output, double[] target)
        {
            if (output.Length != target.Length)
                throw new Exception("Output array does not match size of the target array");

            double[] errors = new double[output.Length];
            for (int i = 0; i < output.Length; i++)
            {
                double diff = output[i] - target[i];
                errors[i] = diff * diff;
            }
            return errors;
        }

        public void TrainForMultipleEpochs(int numberOfEpochs)
        {
            throw new NotImplementedException();
        }

        public void TrainForOneEpoch()
        {
            throw new NotImplementedException();
        }

        private double[] CalculateErrorAndDerivativeProducts(double[] errors, double[] outputs)
        {
            for (int i = 0; i < errors.Length; i++)
            {
                errors[i] *= ActivationFunction.Derivative(outputs[i]);
            }
            return errors;
        }
    }
}
