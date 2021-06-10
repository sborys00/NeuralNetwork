﻿using Prism.Mvvm;
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
        private List<List<object>> drawnNeurons = new List<List<object>>();
        
        private readonly int neuronSize = 20;
        private readonly string neuronPropertyIdentificationName = "Name";

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
                    grid.Name = $"manageButtons_{i}";
                    grid.RowDefinitions.Add(new RowDefinition());
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                    DrawAddButton(bidirectionalGraph, i, grid);
                    DrawRemoveButton(bidirectionalGraph, i, grid);
                    DrawRemoveLayerButton(bidirectionalGraph, i, grid);
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

        private void RemoveLayer(int layer)
        {
            _network.RemoveHiddenLayer(layer);
            var graph = (BidirectionalGraph<object, IEdge<object>>)Graph;
            RemoveLayerFromGraph(graph, layer);
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
                //var lastLayerDrawn = GetNeuronsFromLayer<Ellipse>(graph, layerIndex - 1);
                var lastLayerDrawn = GetLayerDrawn(layerIndex - 1);
                foreach (var lastLayerNeuronDrawn in lastLayerDrawn)
                {
                    graph.AddEdge(new Edge<object>(lastLayerNeuronDrawn, neuronDrawn));
                }
            }

            if (layerIndex != _network.Layers.Count - 1 && drawnNeurons.Count > layerIndex + 1)
            {
                //var nextLayerDrawn = GetNeuronsFromLayer<Ellipse>(graph, layerIndex + 1);
                var nextLayerDrawn = GetLayerDrawn(layerIndex + 1);
                foreach (var nextLayerNeuronDrawn in nextLayerDrawn)
                {
                    graph.AddEdge(new Edge<object>(neuronDrawn, nextLayerNeuronDrawn));
                }
            }

            if (drawnNeurons.Count <= layerIndex)
                drawnNeurons.Add(new List<object>());

            drawnNeurons.ElementAt(layerIndex).Add(neuronDrawn);
        }

        private void RemoveNeuronFromGraph(BidirectionalGraph<object, IEdge<object>> graph, int layerIndex)
        {
            //var verticleToRemove = GetNeuronFromLayer<Ellipse>(graph, layerIndex);
            var verticleToRemove = GetLastNeuronFromLayer(layerIndex);
            graph.RemoveVertex(verticleToRemove);
            ((List<object>)GetLayerDrawn(layerIndex)).Remove(verticleToRemove);
        }

        private void RemoveLayerFromGraph(BidirectionalGraph<object, IEdge<object>> graph, int layerIndex)
        {
            //var lastLayerDrawn = GetNeuronsFromLayer<Ellipse>(graph, layerIndex - 1);
            //var nextLayerDrawn = GetNeuronsFromLayer<Ellipse>(graph, layerIndex + 1);
            //var verticesToRemove = GetNeuronsFromLayer<Ellipse>(graph, layerIndex);
            var lastLayerDrawn = GetLayerDrawn(layerIndex - 1);
            var nextLayerDrawn = GetLayerDrawn(layerIndex + 1);
            var verticesToRemove = GetLayerDrawn(layerIndex);
            foreach (var vertex in verticesToRemove)
            {
                graph.RemoveVertex(vertex);
            }

            foreach (var vertexLast in lastLayerDrawn)
            {
                foreach (var vertexNext in nextLayerDrawn)
                {
                    graph.AddEdge(new Edge<object>(vertexLast, vertexNext));
                }
            }

            drawnNeurons.RemoveAt(layerIndex);
        }

        private void DrawAddButton(BidirectionalGraph<object, IEdge<object>> graph, int layerIndex, Grid grid)
        {
            Button addButton = new();
            addButton.Name = $"addButton_{layerIndex}";
            addButton.Width = neuronSize;
            addButton.Height = neuronSize;
            addButton.Background = new SolidColorBrush { Color = Colors.Green };
            addButton.Content = "+1";
            addButton.Click += delegate { AddNeuron(layerIndex); };

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
            removeButton.Content = "-1";
            removeButton.Click += delegate { RemoveNeuron(layerIndex); };

            Grid.SetColumn(removeButton, 1);
            Grid.SetRow(removeButton, 0);
            grid.Children.Add(removeButton);
        }

        private void DrawRemoveLayerButton(BidirectionalGraph<object, IEdge<object>> graph, int layerIndex, Grid grid)
        {
            Button removeLayerButton = new();
            removeLayerButton.Name = $"removeLayerButton_{layerIndex}";
            removeLayerButton.Width = neuronSize;
            removeLayerButton.Height = neuronSize;
            removeLayerButton.Background = new SolidColorBrush { Color = Colors.Red };
            removeLayerButton.Content = "-n";
            removeLayerButton.Click += delegate { RemoveLayer(layerIndex); };

            Grid.SetColumn(removeLayerButton, 2);
            Grid.SetRow(removeLayerButton, 0);
            grid.Children.Add(removeLayerButton);
        }

        private List<T> GetNeuronsFromLayer<T>(BidirectionalGraph<object, IEdge<object>> graph, int layerIndex)
        {
            var propertyInfo = typeof(T).GetProperty(neuronPropertyIdentificationName);
            return graph.Vertices.Where(v => (v is T) && propertyInfo
            .GetValue((T)v, null).ToString()
            .Split('_')[1] == (layerIndex).ToString())
            .OfType<T>()
            .ToList();
        }

        private IEnumerable<object> GetLayerDrawn(int layerIndex)
        {
            return drawnNeurons.ElementAt(layerIndex);
        }

        private T GetNeuronFromLayer<T>(BidirectionalGraph<object, IEdge<object>> graph, int layerIndex)
        {
            var propertyInfo = typeof(T).GetProperty(neuronPropertyIdentificationName);
            return (T)graph.Vertices.Last(v => (v is T) && propertyInfo
            .GetValue((T)v, null).ToString()
            .Split('_')[1] == layerIndex.ToString());
        }

        private object GetLastNeuronFromLayer(int layerIndex)
        {
            return GetLayerDrawn(layerIndex).Last();
        }
    }
}
