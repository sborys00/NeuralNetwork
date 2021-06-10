using NeuralNetwork.UI.Views;
using Prism.Commands;
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
        public DelegateCommand<string> NavigateCommand { get; set; }
        public DelegateCommand<Window> CloseWindowCommand { get; set; }
        public DelegateCommand<Window> MaximizeWindowCommand { get; set; }
        public DelegateCommand<Window> MinimizeWindowCommand { get; set; }
        public MainWindowViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            CloseWindowCommand = new DelegateCommand<Window>(CloseWindow);
            MaximizeWindowCommand = new DelegateCommand<Window>(MaximizeWindow);
            MinimizeWindowCommand = new DelegateCommand<Window>(MinimizeWindow);

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
    }
}
