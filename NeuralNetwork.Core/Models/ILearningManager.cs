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

        TrainingResult TrainForOneEpoch(INetwork network);
        TrainingResult[] TrainForMultipleEpochs(INetwork network, int numberOfEpochs);

        TestResult RunOneExample(INetwork network, TrainingDataExample test);
        List<TestResult> RunAllExamples(INetwork network, List<TrainingDataExample> examples);

    }
}
