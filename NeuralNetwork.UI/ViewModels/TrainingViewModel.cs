using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using NeuralNetwork.Core.Models;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NeuralNetwork.UI.ViewModels
{
    class TrainingViewModel : BindableBase
    {
        private readonly ILearningManager _learningManager;
        private readonly INetwork _network;

        private readonly LineSeries trainErrorSeries = new() { Title = "Training error", PointGeometry = null };
        private readonly LineSeries testErrorSeries = new() { Title = "Test error", PointGeometry = null };
        private readonly ChartValues<double> trainErrorValues = new();
        private readonly ChartValues<double> testErrorValues = new();
        public DelegateCommand TrainForOneEpochCommand { get; set; }
        public DelegateCommand TrainFor50EpochsCommand { get; set; }
        public DelegateCommand InitializeWeightsCommand { get; set; }

        public SeriesCollection SeriesCollection { get; set; } = new SeriesCollection();

        public TrainingViewModel(ILearningManager learningManager, INetwork network)
        {
            _learningManager = learningManager;
            _network = network;
            TrainForOneEpochCommand = new DelegateCommand(TrainForOneEpoch);
            TrainFor50EpochsCommand = new DelegateCommand(TrainFor50Epochs);
            InitializeWeightsCommand = new DelegateCommand(InitializeWeights);

            SeriesCollection.Add(trainErrorSeries);
            SeriesCollection.Add(testErrorSeries);
            trainErrorSeries.Values = trainErrorValues;
            testErrorSeries.Values = testErrorValues;
            // for testing
            CreateDataForTesting(out _network);
        }

        public void TrainForOneEpoch()
        {
            TrainingResult trainingResult = _learningManager.TrainForOneEpoch(_network);
            trainErrorValues.Add(trainingResult.TrainingExampleTotalError);
            testErrorValues.Add(trainingResult.TestExampleTotalError);
        }

        public void TrainFor50Epochs()
        {
            TrainingResult[] trainingResult = _learningManager.TrainForMultipleEpochs(_network, 50);
            trainErrorValues.AddRange(trainingResult.Select(r =>  r.TrainingExampleTotalError));
            testErrorValues.AddRange(trainingResult.Select(r => r.TestExampleTotalError));
        }

        public void InitializeWeights()
        {
            _network.InitializeWeights();
            trainErrorValues.Clear();
            testErrorValues.Clear();
        }

        private void CreateDataForTesting(out INetwork network)
        {
            // made for testing, to be removed later on

            _learningManager.ActivationFunction = new SigmoidActivationFunction();
            _learningManager.LearningRate = 0.3;

            NetworkBuilder nb = new();
            network = nb.AddLayers(3, 4, 3).Build();

            _learningManager.TrainingSet = new()
            {
                new TrainingDataExample(new double[] { 1.0, 0.3, 0.4 }, new double[] { 1.0, 0.0, 0.0 }),
                new TrainingDataExample(new double[] { 0.2, 0.9, 0.1 }, new double[] { 0.0, 1.0, 0.0 }),
                new TrainingDataExample(new double[] { 0.0, 0.0, 0.7 }, new double[] { 0.0, 0.0, 1.0 }),
                new TrainingDataExample(new double[] { 1.0, 0.0, 0.0 }, new double[] { 1.0, 0.0, 0.0 }),
                new TrainingDataExample(new double[] { 0.0, 1.0, 0.0 }, new double[] { 0.0, 1.0, 0.0 }),
            };
            _learningManager.TestSet = new List<TrainingDataExample>
            {
                new TrainingDataExample(new double[] { 0.0, 0.0, 1.0 }, new double[] { 0.0, 0.0, 1.0 }),
                new TrainingDataExample(new double[] { 0.0, 0.5, 0.0 }, new double[] { 0.0, 1.0, 0.0 }),
                new TrainingDataExample(new double[] { 0.0, 0.0, 0.8 }, new double[] { 0.0, 0.0, 1.0 }),
            };
        }
    }
}
