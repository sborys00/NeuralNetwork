using NeuralNetwork.Core.Models;
using NeuralNetwork.UI.Event;
using OxyPlot;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NeuralNetwork.UI.ViewModels
{
    class TrainingViewModel : BindableBase
    {
        private readonly ILearningManager _learningManager;
        private readonly INetwork _network;
        private readonly IEventAggregator _eventAggregator;

        public TrainingViewModel(ILearningManager learningManager, INetwork network, IEventAggregator eventAggregator)
        {
            _learningManager = learningManager;
            _network = network;
            _eventAggregator = eventAggregator;
            TrainForOneEpochCommand = new DelegateCommand(TrainForOneEpoch);
            TrainFor50EpochsCommand = new DelegateCommand(TrainFor50Epochs);
            InitializeWeightsCommand = new DelegateCommand(InitializeWeights);

            _eventAggregator.GetEvent<TrainingDatasetChangedEvent>().Subscribe(UpdateTrainingDataset);

            // for testing
            CreateDataForTesting(out _network);

            this.PlotModel = new PlotModel { Title = "Example 1" };
            this.PlotModel.Series.Add(TrainingErrorSeries);
            this.PlotModel.Series.Add(TestErrorSeries);
        }
        public DelegateCommand TrainForOneEpochCommand { get; set; }
        public DelegateCommand TrainFor50EpochsCommand { get; set; }
        public DelegateCommand InitializeWeightsCommand { get; set; }

        public PlotModel PlotModel { get; private set; }
        public LineSeries TrainingErrorSeries { get; set; } = new();
        public LineSeries TestErrorSeries { get; set; } = new();

        private TrainingDataset _trainingDataset;

        public TrainingDataset TrainingDataset
        {
            get { return _trainingDataset; }
            set 
            { 
                _trainingDataset = value; 
            }
        }


        public void TrainForOneEpoch()
        {
            TrainingResult trainingResult = _learningManager.TrainForOneEpoch(_network);
            TrainingErrorSeries.Points.Add(new DataPoint(TrainingErrorSeries.Points.Count + 1, trainingResult.TrainingExampleTotalError));
            TestErrorSeries.Points.Add(new DataPoint(TestErrorSeries.Points.Count + 1, trainingResult.TestExampleTotalError));
            PlotModel.InvalidatePlot(true);
        }

        public void TrainFor50Epochs()
        {
            TrainingResult[] trainingResult = _learningManager.TrainForMultipleEpochs(_network, 50);
            foreach(var result in trainingResult)
            {
                TrainingErrorSeries.Points.Add(new DataPoint(TrainingErrorSeries.Points.Count + 1, result.TrainingExampleTotalError));
                TestErrorSeries.Points.Add(new DataPoint(TestErrorSeries.Points.Count + 1, result.TestExampleTotalError));
            }
            PlotModel.InvalidatePlot(true);
        }

        public void InitializeWeights()
        {
            _network.InitializeWeights();
            TestErrorSeries.Points.Clear();
            TrainingErrorSeries.Points.Clear();
            PlotModel.InvalidatePlot(true);
        }

        private void UpdateTrainingDataset(TrainingDataset dataset)
        {
            TrainingDataset = dataset;
            _learningManager.TrainingSet = dataset.TrainingExamples.ToList();
            _learningManager.TestSet = dataset.TestExamples.ToList();
            InitializeWeights();
        }

        private void CreateDataForTesting(out INetwork network)
        {
            // made for testing, to be removed later on

            _learningManager.ActivationFunction = new ReLUActivationFunction();
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
