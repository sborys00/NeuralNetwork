using NeuralNetwork.Core.Models;
using NeuralNetwork.UI.Event;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace NeuralNetwork.UI.ViewModels
{
    class TrainingViewModel : BindableBase
    {
        private readonly ILearningManager _learningManager;
        private readonly IEventAggregator _eventAggregator;
        private INetwork _network;


        public TrainingViewModel(ILearningManager learningManager, INetwork network, IEventAggregator eventAggregator)
        {
            _learningManager = learningManager;
            _network = network;
            _eventAggregator = eventAggregator;
            TrainForOneEpochCommand = new DelegateCommand(TrainForOneEpoch);
            TrainFor50EpochsCommand = new DelegateCommand(TrainFor50Epochs);
            InitializeWeightsCommand = new DelegateCommand(InitializeWeights);
            StartAutoTrainCommand = new DelegateCommand(StartAutoTrain);
            StopAutoTrainCommand = new DelegateCommand(StopAutoTrain);

            ConfigurePlots();
            Delay = 200;

            _eventAggregator.GetEvent<TrainingDatasetChangedEvent>().Subscribe(UpdateTrainingDataset);
            _eventAggregator.GetEvent<NeuralNetworkChangedEvent>().Subscribe(UpdateNetwork);

            _learningManager.ActivationFunction = new SigmoidActivationFunction();
            _learningManager.LearningRate = 0.1;

            _eventAggregator.GetEvent<RequestNeuralNetworkUpdate>().Publish();
            _eventAggregator.GetEvent<RequestDatasetUpdate>().Publish();
            
            // for testing
            CreateDataForTesting(out _network);
        }
        public DelegateCommand TrainForOneEpochCommand { get; set; }
        public DelegateCommand TrainFor50EpochsCommand { get; set; }
        public DelegateCommand InitializeWeightsCommand { get; set; }
        public DelegateCommand StartAutoTrainCommand { get; set; }
        public DelegateCommand StopAutoTrainCommand { get; set; }

        public double ClassificationThreshold { get; set; } = 0.5;

        private double _delay;

        public double Delay
        {
            get { return _delay; }
            set
            {
                if (value <= 0)
                    _delay = 0.00000001;
                else
                    _delay = value;
            }
        }

        public double LearningRate
        {
            get { return _learningManager.LearningRate; }
            set 
            {
                if (value < 0)
                    return;
                _learningManager.LearningRate = value;
            }
        }


        public PlotModel TotalErrorPlot { get; private set; }
        public LineSeries TrainingErrorSeries { get; set; }
        public LineSeries TestErrorSeries { get; set; }

        public PlotModel ClassificationCorrectnessLinePlot { get; set; }


        private LineSeries _classificationCorrectnessLineSeries;

        private TrainingDataset trainingDataset;
        private Timer timer;

        public TrainingDataset TrainingDataset
        {
            get { return trainingDataset; }
            set 
            { 
                trainingDataset = value; 
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
            _network?.InitializeWeights();
            TestErrorSeries.Points.Clear();
            TrainingErrorSeries.Points.Clear();
            _classificationCorrectnessLineSeries.Points.Clear();
            TotalErrorPlot.InvalidatePlot(true);
            ClassificationCorrectnessLinePlot.InvalidatePlot(true);
        }

        public void StartAutoTrain()
        {
            if (timer != null)
                return;

            timer = new System.Timers.Timer(Delay);
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        public void StopAutoTrain()
        {
            if (timer == null)
                return;

            timer.Enabled = false;
            timer.Dispose();
            timer = null;
        }

        private void UpdateTrainingDataset(TrainingDataset dataset)
        {
            TrainingDataset = dataset;
            _learningManager.TrainingSet = dataset.TrainingExamples.ToList();
            _learningManager.TestSet = dataset.TestExamples.ToList();
            InitializeWeights();
        }

        private void UpdateNetwork(Network network)
        {
            _network = network;
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

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            TrainForOneEpoch();
        }

        private void ConfigurePlots()
        {
            byte opacity = 140;

            TotalErrorPlot = new PlotModel 
            {
                Title = "Total Error",
                PlotAreaBorderColor = OxyColors.Transparent,
                TextColor = OxyColor.FromArgb(160,255,255,255)
            };
            TrainingErrorSeries = new()
            {
                Title = "Training",
                Color = OxyColor.FromAColor(opacity, OxyColors.Red)
            };
            TestErrorSeries = new()
            {
                Title = "Test",
                Color = OxyColor.FromAColor(opacity, OxyColors.DeepSkyBlue)

            };
            TotalErrorPlot.Series.Add(TrainingErrorSeries);
            TotalErrorPlot.Series.Add(TestErrorSeries);

            ClassificationCorrectnessLinePlot = new PlotModel
            {
                Title = "Classification Correctness",
                PlotAreaBorderColor = OxyColors.Transparent,
                TextColor = OxyColors.LightGray
            };
            ClassificationCorrectnessLinePlot.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                AbsoluteMaximum = 100,
                AbsoluteMinimum = 0,
            });

            _classificationCorrectnessLineSeries = new()
            {
                Color = OxyColor.FromAColor(opacity, OxyColors.DeepSkyBlue)
            };
            ClassificationCorrectnessLinePlot.Series.Add(_classificationCorrectnessLineSeries);
        }
    }
}
