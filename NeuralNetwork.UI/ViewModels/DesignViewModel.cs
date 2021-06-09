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

namespace NeuralNetwork.UI.ViewModels
{
    public class DesignViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;

        public DesignViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            RedrawNetworkCommand = new DelegateCommand(RedrawNetwork);

            _eventAggregator.GetEvent<TrainingDatasetChangedEvent>().Subscribe(UpdateDataset);
            _eventAggregator.GetEvent<RequestNeuralNetworkUpdate>().Subscribe(PublishNetworkUpdate);
            _eventAggregator.GetEvent<RequestDatasetUpdate>().Publish();

            if (_network == null)
            {
                AssignDefaultNetwork(_dataset);
            }

            if (Graph == null)
            {
                Graph = new BidirectionalGraph<object, IEdge<object>>();
            }

            DrawNetwork(Graph);
        }

        public DelegateCommand RedrawNetworkCommand { get; set; }

        public IGraph<object, IEdge<object>> Graph { get; set; }
        public List<Grid> ManageButtons { get; set; } = new();
        public string LayoutAlgorithmType { get; set; }
        private Network _network;
        private TrainingDataset _dataset;
        
        private readonly int neuronSize = 20;

        private void UpdateDataset(TrainingDataset dataset)
        {
            _dataset = dataset;
            NetworkBuilder nb = new();
            var element = _dataset.TrainingExamples.First();
            _network = nb.AddLayers(element.inputValues.Length, element.expectedOutputs.Length).Build();
            _eventAggregator.GetEvent<NeuralNetworkChangedEvent>().Publish(_network);
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
                firstLayerCount = dataset.TrainingExamples.First().inputValues.Length;
                lastLayerCount = dataset.TrainingExamples.First().expectedOutputs.Length;
            }
            _network = nb.AddLayers(firstLayerCount, 2, 2, 3, lastLayerCount).Build();
        }

        private void DrawNetwork(IGraph<object, IEdge<object>> graph)
        {
            var bidirectionalGraph = (BidirectionalGraph<object, IEdge<object>>)graph;
            for (int i = 0; i < _network.Layers.Count; i++)
            {
                if (i > 0 && i < _network.Layers.Count - 1)
                {
                    Grid grid = new();
                    DrawAddButton(bidirectionalGraph, i, grid);
                    DrawRemoveButton(bidirectionalGraph, i, grid);
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
            DrawNetwork(Graph);
        }

        private void AddNeuron(int layer)
        {
            _network.AddNeuronToLayer(layer);
            var graph = (BidirectionalGraph<object, IEdge<object>>)Graph;
            DrawNeuronOnGraph(graph, layer, _network.Layers[layer].Neurons.Count);
        }

        private void RemoveNeuron(int layer)
        {
            _network.RemoveNeuronFromLayer(layer);
            var graph = (BidirectionalGraph<object, IEdge<object>>)Graph;
            RemoveNeuronFromGraph(graph, layer);
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
                List<Ellipse> lastLayerDrawn = graph.Vertices.Where(v => (v is Ellipse) && ((Ellipse)v).Name
                    .Split('_')[1] == (layerIndex - 1)
                    .ToString())
                    .OfType<Ellipse>()
                    .ToList();
                foreach (var lastLayerNeuronDrawn in lastLayerDrawn)
                {
                    graph.AddEdge(new Edge<object>(lastLayerNeuronDrawn, neuronDrawn));
                }
            }

            if (layerIndex != _network.Layers.Count - 1)
            {
                List<Ellipse> nextLayerDrawn = graph.Vertices.Where(v => (v is Ellipse) && ((Ellipse)v).Name
                    .Split('_')[1] == (layerIndex + 1)
                    .ToString())
                    .OfType<Ellipse>()
                    .ToList();
                foreach (var nextLayerNeuronDrawn in nextLayerDrawn)
                {
                    graph.AddEdge(new Edge<object>(neuronDrawn, nextLayerNeuronDrawn));
                }
            }
        }

        private void RemoveNeuronFromGraph(BidirectionalGraph<object, IEdge<object>> graph, int layerIndex)
        {
            var verticleToRemove = graph.Vertices.First(v => (v is Ellipse) && ((Ellipse)v).Name
                    .Split('_')[1] == (layerIndex)
                    .ToString());
            graph.RemoveVertex(verticleToRemove);
        }

        private void DrawAddButton(BidirectionalGraph<object, IEdge<object>> graph, int layerIndex, Grid grid)
        {
            Button addButton = new();
            addButton.Name = $"addButton_{layerIndex}";
            addButton.Width = neuronSize;
            addButton.Height = neuronSize;
            addButton.Background = new SolidColorBrush { Color = Colors.Green };
            addButton.Content = "+";
            addButton.Click += delegate { AddNeuron(layerIndex); };

            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.SetColumn(addButton, 0);
            Grid.SetRow(addButton, 0);
            grid.Children.Add(addButton);
        }

        private void DrawRemoveButton(BidirectionalGraph<object, IEdge<object>> graph, int layerIndex, Grid grid)
        {
            Button removeButton = new();
            removeButton.Name = $"removeButton_{layerIndex}";
            removeButton.Width = neuronSize;
            removeButton.Height = neuronSize;
            removeButton.Background = new SolidColorBrush { Color = Colors.Red };
            removeButton.Content = "-";
            removeButton.Click += delegate { RemoveNeuron(layerIndex); };

            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.SetColumn(removeButton, 1);
            Grid.SetRow(removeButton, 0);
            grid.Children.Add(removeButton);
        }
    }
}
