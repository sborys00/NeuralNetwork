using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.Core.Models
{
    public class LearningManager : ILearningManager
    {
        public int Epoch { get; set; }

        /// <summary>
        /// Should be between 0.0 to 1.0.
        /// </summary>
        public double LearningRate { get; set; }
        public ActivationFuntion ActivationFunction { get; set; }
        public List<TrainingDataExample> TrainingSet { get; set; }
        public List<TrainingDataExample> TestSet { get; set; }

        public Network Network { get; set; }

        /// <summary>
        /// Runs all tests in TestSet property
        /// </summary>
        /// <param name="network">Instance of Network to be tested</param>
        /// <returns></returns>
        public List<TestResult> RunAllExamples(Network network, List<TrainingDataExample> examples)
        {
            List<TestResult> results = new();
            foreach (var test in examples)
            {
                results.Add(RunOneExample(network, test));
            }
            return results;
        }

        /// <summary>
        /// Runs one test and returns the result as TestResult object.
        /// </summary>
        /// <param name="network">Network instance to be tested</param>
        /// <param name="example">Unit of training data</param>
        /// <returns></returns>
        public TestResult RunOneExample(Network network, TrainingDataExample example)
        {
            var (outputs, inputs) = network.CalculateOutput(example.inputValues, this.ActivationFunction.Function);
            var lastLayer = outputs.Last();
            if (lastLayer.Count != example.expectedOutputs.Length)
                throw new Exception("Number of outputs does not match example data");

            int len = example.expectedOutputs.Length;

            double[] expected = new double[len];
            double[] actual = new double[len];

            for (int i = 0; i < len; i++)
            {
                expected[i] = example.expectedOutputs[i];
                actual[i] = lastLayer[i];
            }
            return new TestResult(expected, actual, outputs, inputs);
        }

        public double RunBackPropagation(Network network, TestResult[] trainingDataResults)
        {
            double[][][] deltaSum = new double[network.Layers.Count - 1][][];
            for (int i = 0; i < network.Layers.Count - 1; i++)
            {
                List<Neuron> neurons = network.Layers[i].Neurons;
                deltaSum[i] = new double[neurons.Count][];
                for (int j = 0; j < neurons.Count; j++)
                {
                    deltaSum[i][j] = new double[neurons[j].Weights.Count];
                }
            }

            List<double> avgErrors = new();
            foreach (TestResult result in trainingDataResults)
            {
                double[] errors = CalculateErrorForOutputLayer(result.actualValues, result.expectedValues);
                avgErrors.Add(errors.Average(e => e * e));
                double[][][] deltas = new double[network.Layers.Count - 1][][];
                for (int i = network.Layers.Count-2; i >= 0; i--)
                {
                    deltas[i] = CalculateDeltasForLayer(network.Layers[i], errors, result.networkInputs[i].ToArray(), result.networkOutputs[i].ToArray());
                    errors = CalculateErrorForHiddenLayer(errors, network.Layers[i]);
                }
                deltaSum = AddDeltasToSumArray(deltaSum, deltas);               
            }
            deltaSum = CalculateAvgDeltas(deltaSum, TrainingSet.Count);
            for (int i = 0; i < network.Layers.Count - 1; i++)
            {
                network.Layers[i].AddDeltaWeights(deltaSum[i]);
            }
            Epoch++;
            return avgErrors.Average();
        }

        /// <summary>
        /// Calculates values which will be added to existing weights to improve network accuracy. It uses gradient descent method. 
        /// </summary>
        /// <param name="layer">Layer with weights to be adjusted</param>
        /// <param name="errors">Errors of n+1 layer</param>
        /// <param name="inputs">Input to neurons of the n+1 layer</param>
        /// <param name="outputs">Output of neurons of n layer</param>
        /// <returns>2-D array of double type values</returns>
        public double[][] CalculateDeltasForLayer(Layer layer, double[] errors, double[] inputs, double[] outputs)
        {
            double[] errorAndDerivativeProducts = CalculateErrorAndDerivativeProducts(errors, inputs);
            double[][] weightDeltas = new double[layer.Neurons.Count][];
            //weight delta from i-th neuron to j-th neuron from next layer
            for (int i = 0; i < layer.Neurons.Count; i++)
            {
                Neuron neuron = layer.Neurons[i];
                weightDeltas[i] = new double[neuron.Weights.Count];
                for (int j = 0; j < neuron.Weights.Count; j++)
                {
                    weightDeltas[i][j] = 2 * LearningRate * errorAndDerivativeProducts[j];
                    weightDeltas[i][j] *= outputs[i];
                }
            }
            return weightDeltas;
        }
        
        public TrainingResult[] TrainForMultipleEpochs(Network network, int numberOfEpochs)
        {
            TrainingResult[] results = new TrainingResult[numberOfEpochs];
            for (int i = 0; i < numberOfEpochs; i++)
            {
                results[i] = TrainForOneEpoch(network);
            }
            return results;
        }
        
        public TrainingResult TrainForOneEpoch(Network network)
        {
            TestResult[] trainingResults = RunAllExamples(network, this.TrainingSet).ToArray();
            TestResult[] testResults = RunAllExamples(network, this.TestSet).ToArray();
            double trainingError = RunBackPropagation(network, trainingResults.ToArray());
            double[] testErrors = new double[testResults.Length];
            for (int i = 0; i < testResults.Length; i++)
            {
                testErrors[i] = CalculateErrorForOutputLayer(testResults[i].actualValues, testResults[i].expectedValues).Average(e => e * e);
            }
            double testError = testErrors.Average();
            return new TrainingResult { 
                TrainingExampleTotalError = trainingError,
                TestExampleTotalError = testError,
                trainingResults = trainingResults,
                testingResults = testResults
            };
        }

        /// <summary>
        /// Calculates error values for the last layer of the network
        /// </summary>
        /// <param name="neuronOutputs">Output values of the final layer</param>
        /// <param name="expectedValues">Expected values specified in the training example</param>
        /// <returns>New array with errors of output layer</returns>
        public double[] CalculateErrorForOutputLayer(double[] neuronOutputs, double[] expectedValues)
        {
            if (neuronOutputs.Length != expectedValues.Length)
                throw new Exception("Output array does not match size of the target array");

            double[] errors = new double[neuronOutputs.Length];
            for (int i = 0; i < neuronOutputs.Length; i++)
            {
                double diff = expectedValues[i] - neuronOutputs[i];
                errors[i] = diff;
            }
            return errors;
        }

        /// <summary>
        /// Calculates error values for a hidden layer of the network based on errors from previous layer (counting from the end)
        /// </summary>
        /// <param name="prevLayerErrors">Errors of the n+1 layer</param>
        /// <param name="layer">N-th layer in the network</param>
        /// <returns>Array of errors of passed layer</returns>
        public double[] CalculateErrorForHiddenLayer(double[] prevLayerErrors, Layer layer)
        {
            double[] errors = new double[layer.Neurons.Count];
            for (int i = 0; i < layer.Neurons.Count; i++)
            {
                Neuron neuron = layer.Neurons[i];
                for (int j = 0; j < neuron.Weights.Count; j++)
                {
                    errors[i] += prevLayerErrors[j] * neuron.Weights[j];
                }
            }
            return errors;
        }

        /// <summary>
        /// Calculates the result of multiplicating error with derivative of activation function
        /// </summary>
        /// <param name="errors">Array of errors from one layer</param>
        /// <param name="neuronInputs">Array of inputs to neurons from corresponding layer</param>
        /// <returns>New array with multiplied values</returns>
        private double[] CalculateErrorAndDerivativeProducts(double[] errors, double[] neuronInputs)
        {
            double[] errorAndDerivativeProducts = (double[])errors.Clone();
            for (int i = 0; i < errorAndDerivativeProducts.Length; i++)
            {
                errorAndDerivativeProducts[i] *= ActivationFunction.Derivative(neuronInputs[i]);
            }
            return errorAndDerivativeProducts;
        }

        /// <summary>
        /// Used to add delta values of entire network from one array to another. It modifies the array passed as first parameter.
        /// </summary>
        /// <param name="sumArray">Array which will be altered</param>
        /// <param name="partialArray">Array with delta values to be added</param>
        /// <returns>Reference to the same array passed as first parameter</returns>
        private double[][][] AddDeltasToSumArray(double[][][] sumArray, double[][][] partialArray)
        {
            for (int i = 0; i < sumArray.Length; i++)
            {
                for (int j = 0; j < sumArray[i].Length; j++)
                {
                    for (int k = 0; k < sumArray[i][j].Length; k++)
                    {
                        sumArray[i][j][k] += partialArray[i][j][k];
                    }
                }
            }
            return sumArray;
        }

        /// <summary>
        /// Divides all values in array by the passed value. Used to tranform array of summed up deltas for entire epoch to array of avg deltas.
        /// It modifies the passed array.
        /// </summary>
        /// <param name="deltaSum">Array of summed up delta values which will be altered</param>
        /// <param name="divisor">Value used to divide the delta sum, should be equal to number of partial deltas in the sum.</param>
        /// <returns></returns>
        private double[][][] CalculateAvgDeltas(double[][][] deltaSum, int divisor)
        {
            for (int i = 0; i < deltaSum.Length; i++)
            {
                for (int j = 0; j < deltaSum[i].Length; j++)
                {
                    for (int k = 0; k < deltaSum[i][j].Length; k++)
                    {
                        deltaSum[i][j][k] /= divisor;
                    }
                }
            }
            return deltaSum;
        }
    }
}
