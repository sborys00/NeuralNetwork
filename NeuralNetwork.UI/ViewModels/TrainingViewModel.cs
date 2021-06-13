using Microsoft.Win32;
using NeuralNetwork.Core.DataAccess;
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
        private readonly ISaveFileReader _sfr;
        private Network _network;


        public TrainingViewModel(ILearningManager learningManager, IEventAggregator eventAggregator, ISaveFileReader sfr)
        {
            _learningManager = learningManager;
            _eventAggregator = eventAggregator;
            _sfr = sfr;
            TrainForOneEpochCommand = new DelegateCommand(TrainForOneEpoch);
            QuickStepsCommand = new DelegateCommand<int?>(TrainForManyEpochs);
            InitializeWeightsCommand = new DelegateCommand(InitializeWeights);
            StartAutoTrainCommand = new DelegateCommand(StartAutoTrain);
            StopAutoTrainCommand = new DelegateCommand(StopAutoTrain);

            ConfigurePlots();
            Speed = 1;

            ActivationFunctions = new()
            {
                new SigmoidActivationFunction(),
                new SigmoidBipolarActivationFunction(),
                new TanhActivationFunction(),
            };

            _eventAggregator.GetEvent<TrainingDatasetChangedEvent>().Subscribe(UpdateTrainingDataset);
            _eventAggregator.GetEvent<NeuralNetworkChangedEvent>().Subscribe(UpdateNetwork);
            _eventAggregator.GetEvent<SaveButtonClickedEvent>().Subscribe(SaveNetwork);

            _learningManager.ActivationFunction = new SigmoidActivationFunction();
            _learningManager.LearningRate = 0.1;

            _eventAggregator.GetEvent<RequestNeuralNetworkUpdate>().Publish();
            _eventAggregator.GetEvent<RequestDatasetUpdate>().Publish();
            
        }
        public DelegateCommand TrainForOneEpochCommand { get; set; }
        public DelegateCommand<int?> QuickStepsCommand { get; set; }
        public DelegateCommand InitializeWeightsCommand { get; set; }
        public DelegateCommand StartAutoTrainCommand { get; set; }
        public DelegateCommand StopAutoTrainCommand { get; set; }

        public double ClassificationThreshold { get; set; } = 0.5;
        public double TargetError { get; set; } = 0.01;
        public int TargetEpoch { get; set; } = 1000;
        public int NumberOfSteps { get; set; } = 10;

        private int _speed;

        public int Speed
        {
            get { return _speed; }
            set
            {
                if (value <= 0)
                    _speed = 1;
                else
                    _speed = value;
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

        public ActivationFuntion SelectedActivationFunction
        {
            get { return _learningManager.ActivationFunction; }
            set 
            {
                _learningManager.ActivationFunction = value; 
            }
        }


        public PlotModel TotalErrorPlot { get; private set; }
        public LineSeries TrainingErrorSeries { get; set; }
        public LineSeries TestErrorSeries { get; set; }

        public List<ActivationFuntion> ActivationFunctions { get; set; } = new();

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

        public void TrainForManyEpochs(int? count)
        {
            int epochs = count == null ? NumberOfSteps : Convert.ToInt32(count);

            TrainingResult[] trainingResult = _learningManager.TrainForMultipleEpochs(_network, epochs);
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
            _learningManager?.ResetEpochCounter();
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

            timer = new System.Timers.Timer(100);
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
            timer.Enabled = false;
            TrainForManyEpochs(Speed);
            if(timer != null)
                timer.Enabled = true;

            if (TrainingErrorSeries.Points.Last().Y <= TargetError || _learningManager.Epoch >= TargetEpoch)
                StopAutoTrain();

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
                TextColor = OxyColor.FromArgb(160, 255, 255, 255)
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

        private void SaveNetwork()
        {
            SaveFileDialog saveFileDialog = new()
            {
                DefaultExt = ".json",
                Filter = "Json files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Save network to file"
            };
            saveFileDialog.ShowDialog();

            if (saveFileDialog.FileName != "")
            {
                Save save = new()
                {
                    Network = _network,
                    TrainingDataset = TrainingDataset
                };
                _sfr.WriteSave(saveFileDialog.FileName, save);
            }
        }
    }
}
