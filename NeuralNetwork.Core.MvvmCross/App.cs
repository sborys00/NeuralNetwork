using MvvmCross;
using MvvmCross.ViewModels;
using NeuralNetwork.Core.MvvmCross.ViewModels;

namespace NeuralNetwork.Core.MvvmCross
{
    public class App : MvxApplication
    {
        public override void Initialize()
        {
           RegisterAppStart<MainViewModel>();
        }
    }
}
