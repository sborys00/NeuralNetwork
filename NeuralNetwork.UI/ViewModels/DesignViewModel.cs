using Prism.Mvvm;
using Prism.Events;
using NeuralNetwork.Core.Models;
using NeuralNetwork.UI.Event;
using System.Linq;
using Prism.Commands;
using GraphShape;
using QuikGraph;
using GraphShape.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System;
using System.Windows;

namespace NeuralNetwork.UI.ViewModels
{
    public class DesignViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;

        public DesignViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            RedrawNetworkCommand = new DelegateCommand(RedrawNetwork);
            AddLayerCommand = new DelegateCommand(AddLayer);
            InitializeWeightsCommand = new DelegateCommand(InitializeWeights);

            _eventAggregator.GetEvent<TrainingDatasetChangedEvent>().Subscribe(UpdateDataset);
            _eventAggregator.GetEvent<RequestNeuralNetworkUpdate>().Subscribe(PublishNetworkUpdate);
            _eventAggregator.GetEvent<NeuralNetworkChangedEvent>().Subscribe((network) => 
            {
                _network = network;
                RedrawNetwork();
            });
            _eventAggregator.GetEvent<RequestDatasetUpdate>().Publish();

            if (_network == null)
            {
                AssignDefaultNetwork(_dataset);
            }

            DrawAndPublish();
        }

        public Network Network
        {
            get => _network;
            set
            {
                _network = value;
                PublishNetworkUpdate();
            }
        }

        public DelegateCommand RedrawNetworkCommand { get; set; }
        public DelegateCommand AddLayerCommand { get; set; }
        public DelegateCommand InitializeWeightsCommand { get; set; }

        public IGraph<object, IEdge<object>> Graph { get; set; }
        public ObservableCollection<Grid> ManageButtons { get; set; } = new();
        public string LayoutAlgorithmType { get; set; }
        private Network _network;
        private TrainingDataset _dataset;
        private List<List<object>> drawnNeurons = new List<List<object>>();
        
        private readonly int neuronSize = 50;

        private void UpdateDataset(TrainingDataset dataset)
        {
            _dataset = dataset;
            NetworkBuilder nb = new();
            var element = _dataset.TrainingExamples.First();
            _network = nb.AddLayers(element.InputValues.Length, element.ExpectedOutputs.Length).Build();
            DrawAndPublish();
        }

        private void PublishNetworkUpdate()
        {
            _eventAggregator.GetEvent<NeuralNetworkChangedEvent>().Publish(_network);
        }

        private void AssignDefaultNetwork(TrainingDataset dataset)
        {
            NetworkBuilder nb = new();
            
            int firstLayerCount = 2;
            int lastLayerCount = 2;
            if (dataset != null)
            {
                firstLayerCount = dataset.TrainingExamples.First().InputValues.Length;
                lastLayerCount = dataset.TrainingExamples.First().ExpectedOutputs.Length;
            }
            _network = nb.AddLayers(firstLayerCount, 2, 2, 3, lastLayerCount).Build();
        }

        private void InitializeWeights()
        {
            _network.InitializeWeights();
            DrawAndPublish();
        }

        private void DrawAndPublish()
        {
            if (drawnNeurons.Count > 0)
            {
                RedrawNetwork();
            }
            else
            {
                Graph = new BidirectionalGraph<object, IEdge<object>>();
                DrawNetwork(Graph);
            }
            PublishNetworkUpdate();
        }

        private void DrawNetwork(IGraph<object, IEdge<object>> graph)
        {
            var bidirectionalGraph = (BidirectionalGraph<object, IEdge<object>>)graph;
            for (int i = 0; i < _network.Layers.Count; i++)
            {
                if (i > 0 && i < _network.Layers.Count - 1)
                {
                    Grid grid = new();
                    grid.Name = $"manageButtons_{i}";
                    grid.RowDefinitions.Add(new RowDefinition());
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                    DrawAddButton(bidirectionalGraph, i, grid);
                    DrawRemoveButton(bidirectionalGraph, i, grid);
                    DrawRemoveLayerButton(bidirectionalGraph, i, grid);
                    GridOnHoverChangeColor(grid, i, new SolidColorBrush { Color = Colors.Aqua }, new SolidColorBrush { Color = Colors.White });
                    ManageButtons.Add(grid);
                }

                for (int j = 0; j < _network.Layers[i].Neurons.Count; j++)
                {
                    DrawNeuronOnGraph(bidirectionalGraph, i, j);
                }
            }

            LayoutAlgorithmType = "Sugiyama";
        }

        private void RedrawNetwork()
        {
            var graph = (BidirectionalGraph<object, IEdge<object>>)Graph;
            graph.Clear();
            drawnNeurons.Clear();
            ManageButtons.Clear();
            DrawNetwork(Graph);
        }

        private void AddNeuron(int layer)
        {
            _network.AddNeuronToLayer(layer);
            DrawAndPublish();
        }

        private void AddLayer()
        {
            _network.InsertHiddenLayer(_network.Layers.Count - 1, 1);
            DrawAndPublish();
        }

        private void RemoveNeuron(int layer)
        {
            if (_network.Layers[layer].Neurons.Count <= 1)
            {
                RemoveLayer(layer);
            }
            else
            {
                _network.RemoveNeuronFromLayer(layer);
                DrawAndPublish();
            }
        }

        private void RemoveLayer(int layer)
        {
            _network.RemoveHiddenLayer(layer);
            DrawAndPublish();
        }

        private void DrawNeuronOnGraph(BidirectionalGraph<object, IEdge<object>> graph, int layerIndex, int neuronIndex)
        {
            SolidColorBrush colorBrush = new();
            colorBrush.Color = Colors.White;

            Ellipse neuronDrawn = new();
            neuronDrawn.Name = $"neuron_{layerIndex}_{neuronIndex}";
            neuronDrawn.Width = neuronSize;
            neuronDrawn.Height = neuronSize;
            neuronDrawn.Fill = colorBrush;
            graph.AddVertex(neuronDrawn);


            if (layerIndex != 0)
            {
                List<SolidColorBrush> weightBrushes = NeuronInWeightsToBrush(layerIndex, neuronIndex);
                var lastLayerDrawn = GetLayerDrawn(layerIndex - 1);

                //foreach (var lastLayerNeuronDrawn in lastLayerDrawn)
                //{
                //    graph.AddEdge(new ColoredEdge<object>(lastLayerNeuronDrawn, neuronDrawn, new SolidColorBrush { Color = Colors.Green }));
                //}
                foreach (var lastLayerNeuronDrawn in lastLayerDrawn.Zip(weightBrushes, (n, b) => new { Neuron = n, Brush = b }))
                {
                    graph.AddEdge(new ColoredEdge<object>(lastLayerNeuronDrawn.Neuron, neuronDrawn, lastLayerNeuronDrawn.Brush));
                }
            }

            //if (layerIndex != _network.Layers.Count - 1 && drawnNeurons.Count > layerIndex + 1)
            //{
            //    var nextLayerDrawn = GetLayerDrawn(layerIndex + 1);
            //    foreach (var nextLayerNeuronDrawn in nextLayerDrawn)
            //    {
            //        graph.AddEdge(new ColoredEdge<object>(neuronDrawn, nextLayerNeuronDrawn, new SolidColorBrush { Color = Colors.Green }));
            //    }
            //}

            if (drawnNeurons.Count <= layerIndex)
                drawnNeurons.Add(new List<object>());

            drawnNeurons.ElementAt(layerIndex).Add(neuronDrawn);
        }

        private List<SolidColorBrush> NeuronInWeightsToBrush(int layerIndex, int neuronIndex)
        {
            _network.CalculateWeightBoundsForLayer(layerIndex - 1, out double upperBound, out double lowerBound);
            List<SolidColorBrush> brushes = new(_network.Layers[layerIndex - 1].Neurons.Count);
            int i = 0;
            foreach (var neuron in _network.Layers[layerIndex - 1].Neurons)
            {
                if (neuron.Weights[neuronIndex] < lowerBound || neuron.Weights[neuronIndex] > upperBound)
                    throw new Exception($"Weight {neuronIndex} of neuron {i} at layer {layerIndex - 1} has weight that exceeds bounds");

                brushes.Add(WeightToBrush(neuron.Weights[neuronIndex], upperBound, lowerBound));
                i++;
            }
            return brushes;
        }

        private List<SolidColorBrush> NeuronWeightsToBrush(int layerIndex, int neuronIndex)
        {
            _network.CalculateWeightBoundsForLayer(layerIndex, out double upperBound, out double lowerBound);
            List<SolidColorBrush> brushes = new(_network.Layers[layerIndex].Neurons.Count);
            foreach (var weight in _network.Layers[layerIndex].Neurons[neuronIndex].Weights)
            {
                brushes.Add(WeightToBrush(weight, upperBound, lowerBound));
            }
            return brushes;
        }

        private SolidColorBrush WeightToBrush(double weight, double upperBound, double lowerBound)
        {
            var x = (weight + Math.Abs(lowerBound)) * 511 / (upperBound - lowerBound);
            if (x > 255)
                return new SolidColorBrush { Color = Color.FromRgb(0, Convert.ToByte(x - 255), 0) };
            else
                return new SolidColorBrush { Color = Color.FromRgb(Convert.ToByte(x), 0, 0) };
        }

        private void DrawAddButton(BidirectionalGraph<object, IEdge<object>> graph, int layerIndex, Grid grid)
        {
            DrawButton(graph, layerIndex, grid, 0, "addNeuronButton", "+1", new SolidColorBrush { Color = Colors.Green }, AddNeuron);
        }

        private void DrawRemoveButton(BidirectionalGraph<object, IEdge<object>> graph, int layerIndex, Grid grid)
        {
            DrawButton(graph, layerIndex, grid, 1, "removeNeuronButton", "-1", new SolidColorBrush { Color = Colors.Red }, RemoveNeuron);
        }

        private void DrawRemoveLayerButton(BidirectionalGraph<object, IEdge<object>> graph, int layerIndex, Grid grid)
        {
            DrawButton(graph, layerIndex, grid, 2, "removeLayerButton", "-n", new SolidColorBrush { Color = Colors.Red }, RemoveLayer);
        }

        private void DrawButton(BidirectionalGraph<object, IEdge<object>> graph, int layerIndex, Grid grid, int gridPosition, string name, string content, SolidColorBrush brush, Action<int> click)
        {
            Button button = new();
            button.Name = name + $"_{layerIndex}";
            button.Width = neuronSize * 2;
            button.Height = neuronSize;
            button.Background = brush;
            button.Content = content;
            button.Click += delegate { click(layerIndex); };
            
            Grid.SetColumn(button, gridPosition);
            Grid.SetRow(button, 0);
            grid.Children.Add(button);
        }

        private void GridOnHoverChangeColor(Grid grid, int layerIndex, SolidColorBrush brushEnter, SolidColorBrush brushLeave)
        {
            grid.MouseEnter += delegate { ChangeLayerColor(layerIndex, brushEnter); };
            grid.MouseLeave += delegate { ChangeLayerColor(layerIndex, brushLeave); };
        }

        private void ChangeLayerColor(int layerIndex, SolidColorBrush brush)
        {
            drawnNeurons.ElementAt(layerIndex).ForEach(n => ((Shape)n).Fill = brush);
        }

        private IEnumerable<object> GetLayerDrawn(int layerIndex)
        {
            return drawnNeurons.ElementAt(layerIndex);
        }
    }
}
