using System;
using System.Collections.Generic;
using NeuralNetwork.Core.Models;

namespace NeuralNetwork.Core.Models
{
    public interface ILearningManager
    {
        ActivationFuntion ActivationFunction { get; set; }
        int Epoch { get; }
        double LearningRate { get; set; }
        Network Network { get; set; }
        List<TrainingDataUnit> TrainingSet { get; set; }
        List<TrainingDataUnit> TestSet { get; set; }

        void TrainForOneEpoch(Network network);
        void TrainForMultipleEpochs(Network network, int numberOfEpochs);

        TestResult RunOneTest(Network network, TrainingDataUnit test);
        List<TestResult> RunAllTests(Network network);

    }
}
