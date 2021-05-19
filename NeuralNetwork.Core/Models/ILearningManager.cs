using System;
using System.Collections.Generic;

namespace NeuralNetwork.Core.Models
{
    public interface ILearningManager
    {
        Func<double, double> ActivationFunction { get; set; }
        int Epoch { get; }
        double LearningRate { get; set; }
        Network Network { get; set; }
        List<IEnumerable<double>> TrainingSet { get; set; }
        List<IEnumerable<double>> TestSet { get; set; }

        void TrainForOneEpoch();
        void TrainForMultipleEpochs(int numberOfEpochs);

        TestResult RunOneTest(Network network, IEnumerable<double> test);
        List<TestResult> RunAllTests(Network network);

    }
}
