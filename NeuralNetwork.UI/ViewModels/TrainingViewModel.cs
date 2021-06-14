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
            _eventAggregator.GetEvent<TrainingConfigUpdateEvent>().Subscribe(UpdateTrainingProperties);

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

        public double ClassificationThreshold { get => classificationThreshold; set => SetProperty(ref classificationThreshold, value); }
        public double TargetError { get => targetError; set => SetProperty(ref targetError, value); }
        public int TargetEpoch { get => targetEpoch; set => SetProperty(ref targetEpoch, value); }
        public int NumberOfSteps { get => numberOfSteps; set => SetProperty(ref numberOfSteps, value); }
        public PlotModel TotalErrorPlot { get; private set; }
        public LineSeries TrainingErrorSeries { get; set; }
        public LineSeries TestErrorSeries { get; set; }

        public List<ActivationFuntion> ActivationFunctions { get; set; } = new();

        public PlotModel ClassificationCorrectnessLinePlot { get; set; }

        private int _speed;
        public int Speed
        {
            get { return _speed; }
            set
            {
                if (value <= 0)
                    _speed = 1;
                else
                    SetProperty(ref _speed, value);
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
                RaisePropertyChanged(nameof(LearningRate));

            }
        }

        public ActivationFuntion SelectedActivationFunction
        {
            get { return _learningManager.ActivationFunction; }
            set
            {
                _learningManager.ActivationFunction = value;
                RaisePropertyChanged(nameof(SelectedActivationFunction));

            }
        }

        private TrainingDataset trainingDataset;

        public TrainingDataset TrainingDataset
        {
            get { return trainingDataset; }
            set
            {
                SetProperty(ref trainingDataset, value);
            }
        }

        private LineSeries _classificationCorrectnessTestLineSeries;
        private LineSeries _classificationCorrectnessTrainingLineSeries;

        private Timer timer;
        private int numberOfSteps = 10;
        private int targetEpoch = 1000;
        private double targetError = 0.01;
        private double classificationThreshold = 0.5;

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
            foreach (var result in trainingResult)
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
            _classificationCorrectnessTestLineSeries.Points.Clear();
            _classificationCorrectnessTrainingLineSeries.Points.Clear();
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
        }
        private void UpdateTrainingProperties(TrainingConfig cfg)
        {
            LearningRate = cfg.LearningRate;
            TargetEpoch = cfg.TargetEpoch;
            TargetError = cfg.TargetError;
            ClassificationThreshold = cfg.ClassificationThreshold;
            NumberOfSteps = cfg.NumberOfSteps;
            Speed = cfg.Speed;
            Type t = Type.GetType(cfg.ActivationFunctionName);
            SelectedActivationFunction = (ActivationFuntion)Activator.CreateInstance(t);
        }
        private void UpdateClassificationCorrectnessLinePlot(TrainingResult trainingResult)
        {
            double testPercentage = CalculatePercentageOfCorrectClassification(trainingResult.testingResults);
            double trainingPercentage = CalculatePercentageOfCorrectClassification(trainingResult.trainingResults);
            _classificationCorrectnessTestLineSeries.Points.Add(new DataPoint(_classificationCorrectnessTestLineSeries.Points.Count + 1, testPercentage));
            _classificationCorrectnessTrainingLineSeries.Points.Add(new DataPoint(_classificationCorrectnessTrainingLineSeries.Points.Count + 1, trainingPercentage));
        }

        private double CalculatePercentageOfCorrectClassification(TestResult[] results)
        {
            int correctCount = 0;
            foreach (var result in results)
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
            return 100d * correctCount / results.Length;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            timer.Enabled = false;
            TrainForManyEpochs(Speed);
            if (timer != null)
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
                TextColor = OxyColor.FromArgb(160, 255, 255, 255)
            };
            TotalErrorPlot.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = 0,
            });
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
                Maximum = 100,
                Minimum = 0,            
            });

            _classificationCorrectnessTestLineSeries = new()
            {
                Title = "Test",
                Color = OxyColor.FromAColor(opacity, OxyColors.DeepSkyBlue)
            };
            _classificationCorrectnessTrainingLineSeries = new()
            {
                Title = "Training",
                Color = OxyColor.FromAColor(opacity, OxyColors.Red)
            };
            ClassificationCorrectnessLinePlot.Series.Add(_classificationCorrectnessTestLineSeries);
            ClassificationCorrectnessLinePlot.Series.Add(_classificationCorrectnessTrainingLineSeries);
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

            TrainingConfig cfg = new()
            {
                LearningRate = LearningRate,
                TargetEpoch = TargetEpoch,
                TargetError = TargetError,
                ClassificationThreshold = ClassificationThreshold,
                NumberOfSteps = NumberOfSteps,
                ActivationFunctionName = SelectedActivationFunction.GetType().AssemblyQualifiedName,
                Speed = Speed
            };

            if (saveFileDialog.FileName != "")
            {
                Save save = new()
                {
                    Network = _network,
                    TrainingDataset = TrainingDataset,
                    TrainingConfig = cfg

                };
                _sfr.WriteSave(saveFileDialog.FileName, save);
            }
        }
    }
}
