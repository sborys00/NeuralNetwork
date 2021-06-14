using Microsoft.Win32;
using NeuralNetwork.Core.DataAccess;
using NeuralNetwork.Core.Models;
using NeuralNetwork.UI.Event;
using NeuralNetwork.UI.Views;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeuralNetwork.UI.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly ISaveFileReader _sfr;

        public DelegateCommand<string> NavigateCommand { get; set; }
        public DelegateCommand<Window> CloseWindowCommand { get; set; }
        public DelegateCommand<Window> MaximizeWindowCommand { get; set; }
        public DelegateCommand<Window> MinimizeWindowCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand LoadCommand { get; set; }
        public MainWindowViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, ISaveFileReader sfr)
        {
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;
            _sfr = sfr;
            CloseWindowCommand = new DelegateCommand<Window>(CloseWindow);
            MaximizeWindowCommand = new DelegateCommand<Window>(MaximizeWindow);
            MinimizeWindowCommand = new DelegateCommand<Window>(MinimizeWindow);

            SaveCommand = new DelegateCommand(() => _eventAggregator.GetEvent<SaveButtonClickedEvent>().Publish());
            LoadCommand = new DelegateCommand(LoadNetwork);

            NavigateCommand = new DelegateCommand<string>(Navigate);
            _regionManager.RegisterViewWithRegion("ContentRegion", typeof(HomeView));
            _regionManager.RegisterViewWithRegion("ContentRegion", typeof(DataView));
            _regionManager.RegisterViewWithRegion("ContentRegion", typeof(DesignView));
            _regionManager.RegisterViewWithRegion("ContentRegion", typeof(TrainingView));
        }

        public void Navigate(string view)
        {
            _regionManager.RequestNavigate("ContentRegion", view);
        }

        public void CloseWindow(Window window)
        {
            window.Close();
        }

        public void MaximizeWindow(Window window)
        {
            if (window.WindowState == WindowState.Maximized)
            {
                window.WindowState = WindowState.Normal;
                window.SizeToContent = SizeToContent.WidthAndHeight;
            }
            else
            {
                window.SizeToContent = SizeToContent.Manual;
                window.WindowState = WindowState.Maximized;
            }
        }
        public void MinimizeWindow(Window window)
        {
            window.WindowState = WindowState.Minimized;
        }

        public async void LoadNetwork()
        {
            OpenFileDialog dialog = new()
            {
                DefaultExt = ".json",
                Filter = "Json files (*.json)|*.json|All files (*.*)|*.*"
            };
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                Save save = await _sfr.ReadSave(dialog.FileName);
                _eventAggregator.GetEvent<TrainingDatasetChangedEvent>().Publish(save.TrainingDataset);
                _eventAggregator.GetEvent<NeuralNetworkChangedEvent>().Publish(save.Network);
                _eventAggregator.GetEvent<TrainingConfigUpdateEvent>().Publish(save.TrainingConfig);
            }
        }
    }
}
