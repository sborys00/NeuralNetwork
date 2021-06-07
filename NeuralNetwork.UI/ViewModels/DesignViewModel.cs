using Prism.Mvvm;
using Prism.Events;
using NeuralNetwork.Core.Models;
using NeuralNetwork.UI.Event;
using System.Linq;

namespace NeuralNetwork.UI.ViewModels
{
    public class DesignViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;

        public DesignViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            _eventAggregator.GetEvent<TrainingDatasetChangedEvent>().Subscribe(UpdateDataset);
            _eventAggregator.GetEvent<RequestNeuralNetworkUpdate>().Subscribe(PublishNetworkUpdate);
            _eventAggregator.GetEvent<RequestDatasetUpdate>().Publish();
        }

        private Network _network;
        private TrainingDataset _dataset; 

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
    }
}
