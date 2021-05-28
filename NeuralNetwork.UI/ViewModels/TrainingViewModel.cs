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
        /*
         * Chart lags when large series are present
         * Needs optimization
         * 
         */

        private readonly ILearningManager _learningManager;
        private readonly INetwork _network;
        public DelegateCommand TrainForOneEpochCommand { get; set; }
        public DelegateCommand TrainFor50EpochsCommand { get; set; }
        public DelegateCommand InitializeWeightsCommand { get; set; }

        public ChartValues<ObservablePoint> TrainErrorSeries { get; set; } = new();
        public ChartValues<ObservablePoint> TestErrorSeries { get; set; } = new();

        public TrainingViewModel(ILearningManager learningManager, INetwork network)
        {
            _learningManager = learningManager;
            _network = network;
            TrainForOneEpochCommand = new DelegateCommand(TrainForOneEpoch);
            TrainFor50EpochsCommand = new DelegateCommand(TrainFor50Epochs);
            InitializeWeightsCommand = new DelegateCommand(InitializeWeights);

            // for testing
            CreateDataForTesting(out _network);
        }

        public void TrainForOneEpoch()
        {
            TrainingResult trainingResult = _learningManager.TrainForOneEpoch(_network);
            int index = TrainErrorSeries.Count + 1;
            TrainErrorSeries.Add(new ObservablePoint(index, trainingResult.TrainingExampleTotalError));
            TestErrorSeries.Add(new ObservablePoint(index, trainingResult.TestExampleTotalError));
        }

        public void TrainFor50Epochs()
        {
            TrainingResult[] trainingResult = _learningManager.TrainForMultipleEpochs(_network, 50);
            int trainIndex = TrainErrorSeries.Count + 1;
            int testIndex = trainIndex;
            TrainErrorSeries.AddRange(trainingResult.Select(r => new ObservablePoint(trainIndex++, r.TrainingExampleTotalError)));
            TestErrorSeries.AddRange(trainingResult.Select(r => new ObservablePoint(testIndex++, r.TestExampleTotalError)));
        }

        public void InitializeWeights()
        {
            _network.InitializeWeights();
            TrainErrorSeries.Clear();
            TestErrorSeries.Clear();
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
