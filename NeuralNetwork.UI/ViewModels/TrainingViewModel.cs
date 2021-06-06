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

            TotalErrorPlot = new PlotModel { Title = "Total Error" };
            TotalErrorPlot.Series.Add(TrainingErrorSeries);
            TotalErrorPlot.Series.Add(TestErrorSeries);

            ClassificationCorrectnessLinePlot = new PlotModel { Title = "Classification Correctness" };
            ClassificationCorrectnessLinePlot.Series.Add(_classificationCorrectnessLineSeries);
        }
        public DelegateCommand TrainForOneEpochCommand { get; set; }
        public DelegateCommand TrainFor50EpochsCommand { get; set; }
        public DelegateCommand InitializeWeightsCommand { get; set; }

        public double ClassificationThreshold { get; set; } = 0.5;

        public PlotModel TotalErrorPlot { get; private set; }
        public LineSeries TrainingErrorSeries { get; set; } = new();
        public LineSeries TestErrorSeries { get; set; } = new();

        public PlotModel ClassificationCorrectnessLinePlot { get; set; }

        private readonly LineSeries _classificationCorrectnessLineSeries = new();

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
            UpdateClassificationCorrectnessLinePlot(trainingResult);
            TotalErrorPlot.InvalidatePlot(true);
            ClassificationCorrectnessLinePlot.InvalidatePlot(true);
        }

        public void TrainFor50Epochs()
        {
            TrainingResult[] trainingResult = _learningManager.TrainForMultipleEpochs(_network, 50);
            foreach(var result in trainingResult)
            {
                TrainingErrorSeries.Points.Add(new DataPoint(TrainingErrorSeries.Points.Count + 1, result.TrainingExampleTotalError));
                TestErrorSeries.Points.Add(new DataPoint(TestErrorSeries.Points.Count + 1, result.TestExampleTotalError));
                UpdateClassificationCorrectnessLinePlot(result);
            }
            TotalErrorPlot.InvalidatePlot(true);
            ClassificationCorrectnessLinePlot.InvalidatePlot(true);
        }

        public void InitializeWeights()
        {
            _network.InitializeWeights();
            TestErrorSeries.Points.Clear();
            TrainingErrorSeries.Points.Clear();
            _classificationCorrectnessLineSeries.Points.Clear();
            TotalErrorPlot.InvalidatePlot(true);
            ClassificationCorrectnessLinePlot.InvalidatePlot(true);
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

            _learningManager.ActivationFunction = new SigmoidActivationFunction();
            _learningManager.LearningRate = 0.1;

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

        private void UpdateClassificationCorrectnessLinePlot(TrainingResult trainingResult)
        {
            int correctCount = 0;
            foreach(var result in trainingResult.testingResults)
            {
                bool correct = true;
                for (int i = 0; i < result.actualValues.Length; i++)
                {
                    //if > 0, then both greater or lower than threshold at the same time
                    double sideOfThreshold = (result.actualValues[i] - ClassificationThreshold) * (result.expectedValues[i] - ClassificationThreshold);
                    if (sideOfThreshold < 0)
                        correct = false;
                }
                if (correct)
                    correctCount++;
            }
            double correctPercentage = 100d * correctCount / trainingResult.testingResults.Length;
            _classificationCorrectnessLineSeries.Points.Add(new DataPoint(_classificationCorrectnessLineSeries.Points.Count + 1, correctPercentage));
        }
    }
}
