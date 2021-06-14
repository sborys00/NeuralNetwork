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
using NeuralNetwork.UI.CustomGraphModels;

namespace NeuralNetwork.UI.ViewModels
{
    public class DesignViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;

        private readonly int neuronSize = 50;

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
                if(network != Network)
                    Network = network;
            });
            _eventAggregator.GetEvent<RequestDatasetUpdate>().Publish();

            if (_network == null)
            {
                AssignDefaultNetwork(Dataset);
            }
        }

        public DelegateCommand RedrawNetworkCommand { get; set; }
        public DelegateCommand AddLayerCommand { get; set; }
        public DelegateCommand InitializeWeightsCommand { get; set; }

        public IGraph<object, IEdge<object>> Graph { get; set; }
        public ObservableCollection<Grid> ManageButtons { get; set; } = new();

        private Network _network;
        public Network Network
        {
            get => _network;
            set
            {
                if(_network != value)
                {
                    _network = value;
                    RedrawNetwork();
                    PublishNetworkUpdate();
                }
            }
        }
        private TrainingDataset _dataset;

        public TrainingDataset Dataset
        {
            get => _dataset;
            set
            {
                _dataset = value;
                NetworkBuilder nb = new();
                var element = Dataset.TrainingExamples.First();
                Network = nb.AddLayers(element.InputValues.Length, element.ExpectedOutputs.Length).Build();
            }
        }

        public string LayoutAlgorithmType { get; set; }
        private List<List<object>> drawnNeurons = new List<List<object>>();
        

        private void UpdateDataset(TrainingDataset dataset)
        {
            Dataset = dataset;
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
            Network = nb.AddLayers(firstLayerCount, 2, 2, 3, lastLayerCount).Build();
        }

        private void InitializeWeights()
        {
            Network.InitializeWeights();
            RedrawNetwork();
        }

        private void DrawNetwork(IGraph<object, IEdge<object>> graph)
        {
            var bidirectionalGraph = (BidirectionalGraph<object, IEdge<object>>)graph;
            for (int i = 0; i < Network.Layers.Count; i++)
            {
                if (i > 0 && i < Network.Layers.Count - 1)
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

                for (int j = 0; j < Network.Layers[i].Neurons.Count; j++)
                {
                    DrawNeuronOnGraph(bidirectionalGraph, i, j);
                }
            }

            LayoutAlgorithmType = "Sugiyama";
        }

        private void RedrawNetwork()
        {
            Graph = new BidirectionalGraph<object, IEdge<object>>();
            drawnNeurons.Clear();
            ManageButtons.Clear();
            DrawNetwork(Graph);
            RaisePropertyChanged(nameof(Graph));
        }

        private void AddNeuron(int layer)
        {
            Network.AddNeuronToLayer(layer);
            RedrawNetwork();
        }

        private void AddLayer()
        {
            Network.InsertHiddenLayer(Network.Layers.Count - 1, 1);
            RedrawNetwork();
        }

        private void RemoveNeuron(int layer)
        {
            if (Network.Layers[layer].Neurons.Count <= 1)
            {
                RemoveLayer(layer);
            }
            else
            {
                Network.RemoveNeuronFromLayer(layer);
                RedrawNetwork();
            }
        }

        private void RemoveLayer(int layer)
        {
            Network.RemoveHiddenLayer(layer);
            RedrawNetwork();
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

                foreach (var lastLayerNeuronDrawn in lastLayerDrawn.Zip(weightBrushes, (n, b) => new { Neuron = n, Brush = b }))
                {
                    graph.AddEdge(new ColoredEdge<object>(lastLayerNeuronDrawn.Neuron, neuronDrawn, lastLayerNeuronDrawn.Brush));
                }
            }

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

        private SolidColorBrush WeightToBrush(double weight, double upperBound, double lowerBound)
        {
            var x = (weight + Math.Abs(lowerBound)) * 510 / (upperBound - lowerBound);
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
