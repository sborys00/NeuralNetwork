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
        List<TrainingDataExample> TrainingSet { get; set; }
        List<TrainingDataExample> TestSet { get; set; }

        TrainingResult TrainForOneEpoch(Network network);
        TrainingResult[] TrainForMultipleEpochs(Network network, int numberOfEpochs);

        TestResult RunOneExample(Network network, TrainingDataExample test);
        List<TestResult> RunAllExamples(Network network, List<TrainingDataExample> examples);

    }
}
