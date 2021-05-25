using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using NeuralNetwork.UI;
using NeuralNetwork.UI.Views;
using Prism.Ioc;
using Prism.Unity;

namespace NeuralNetwork.UI
{
    public partial class App : PrismApplication
    {
        // RegisterTypes function is here

        protected override Window CreateShell()
        {
            var w = Container.Resolve<Views.MainWindow>();
            return w;
        }
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<HomeView>();
            containerRegistry.RegisterForNavigation<DesignView>();
            containerRegistry.RegisterForNavigation<TrainingView>();
            containerRegistry.RegisterForNavigation<Views.DataView>();
        }
    }
}