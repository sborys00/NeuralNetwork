﻿using System;
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
            var lastLayer = networkOutputs.outputs.Last();
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
            return new TestResult(expected, actual, networkOutputs.outputs, networkOutputs.inputs);
        }

        public void RunBackPropagation(Network network)
        {
            foreach (var test in TrainingSet)
            {
                TestResult result = RunOneTest(network, test);
                double[] errors = CalculateErrorForOutputLayer(result.actualValues, result.expectedValues);
                double[] errorAndDerivativeProducts = CalculateErrorAndDerivativeProducts(errors, result.networkInputs[^1].ToArray());
                Layer outputLayer = network.Layers.Last();
                Layer prevLayer = network.Layers[^2];
                double[][] weightDeltas = new double[prevLayer.Neurons.Count][];
                //weight delta from i-th neuron to j-th neuron from next layer
                for (int i = 0; i < prevLayer.Neurons.Count; i++)
                {
                    Neuron neuron = prevLayer.Neurons[i];
                    weightDeltas[i] = new double[neuron.Weights.Count];
                    for (int j = 0; j < neuron.Weights.Count; j++)
                    {
                        weightDeltas[i][j] = 2 * LearningRate * errorAndDerivativeProducts[j];
                        weightDeltas[i][j] *= result.networkOutputs[^2][i];
                    }
                }
            }
        }

        public double[] CalculateErrorForOutputLayer(double[] output, double[] target)
        {
            if (output.Length != target.Length)
                throw new Exception("Output array does not match size of the target array");

            double[] errors = new double[output.Length];
            for (int i = 0; i < output.Length; i++)
            {
                double diff = output[i] - target[i];
                errors[i] = diff;// * diff;
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

        private double[] CalculateErrorAndDerivativeProducts(double[] errors, double[] inputs)
        {
            double[] errorsAndDerivative = (double[])errors.Clone();
            for (int i = 0; i < errorsAndDerivative.Length; i++)
            {
                errorsAndDerivative[i] *= ActivationFunction.Derivative(inputs[i]);
            }
            return errorsAndDerivative;
        }
    }
}
