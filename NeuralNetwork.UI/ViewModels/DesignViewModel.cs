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

namespace NeuralNetwork.UI.ViewModels
{
    public class DesignViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;

        public DesignViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            AddNeuronCommand = new DelegateCommand(delegate { AddNeuron(1); });

            _eventAggregator.GetEvent<TrainingDatasetChangedEvent>().Subscribe(UpdateDataset);
            _eventAggregator.GetEvent<RequestNeuralNetworkUpdate>().Subscribe(PublishNetworkUpdate);
            _eventAggregator.GetEvent<RequestDatasetUpdate>().Publish();

            if (_network == null)
            {
                AssignDefaultNetwork();
            }

            DrawNetwork();
        }

        public DelegateCommand AddNeuronCommand { get; set; }

        public IGraph<object, IEdge<object>> Graph { get; set; }
        public string LayoutAlgorithmType { get; set; }
        public string FlowDirection { get; set; }
        private Network _network;
        private TrainingDataset _dataset;
        
        private int neuronSize = 20;

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

        private void AssignDefaultNetwork()
        {
            NetworkBuilder nb = new();
            _network = nb.AddLayers(2, 2, 2).Build();
        }

        private void DrawNetwork()
        {
            var graph = new BidirectionalGraph<object, IEdge<object>>();

            for (int i = 0; i < _network.Layers.Count; i++)
            {
                for (int j = 0; j < _network.Layers[i].Neurons.Count; j++)
                {
                    DrawNeuron(graph, i, j);
                }
            }

            LayoutAlgorithmType = "Sugiyama";
            FlowDirection = "LeftToRight";

            Graph = graph;
        }

        private void AddNeuron(int layer)
        {
            _network.AddNeuronToLayer(layer);
            var graph = (BidirectionalGraph<object, IEdge<object>>)Graph;
            DrawNeuron(graph, layer, _network.Layers[layer].Neurons.Count);
        }

        private void DrawNeuron(BidirectionalGraph<object, IEdge<object>> graph, int layerIndex, int neuronIndex)
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
                List<Ellipse> lastLayerDrawn = graph.Vertices.Where(v => ((Ellipse)v).Name.Split('_')[1] == (layerIndex - 1).ToString()).OfType<Ellipse>().ToList();
                foreach (var lastLayerNeuronDrawn in lastLayerDrawn)
                {
                    graph.AddEdge(new Edge<object>(lastLayerNeuronDrawn, neuronDrawn));
                }
            }

            if (layerIndex != _network.Layers.Count - 1)
            {
                List<Ellipse> nextLayerDrawn = graph.Vertices.Where(v => ((Ellipse)v).Name.Split('_')[1] == (layerIndex + 1).ToString()).OfType<Ellipse>().ToList();
                foreach (var nextLayerNeuronDrawn in nextLayerDrawn)
                {
                    graph.AddEdge(new Edge<object>(neuronDrawn, nextLayerNeuronDrawn));
                }
            }
        }
    }
}
