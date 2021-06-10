using Prism.Commands;
using Prism.Mvvm;

namespace NeuralNetwork.UI.ViewModels
{
    public class HomeViewModel : BindableBase
    {
        public HomeViewModel()
        {
            OpenGithubRepoCommand = new DelegateCommand<string>(OpenGithubRepo);
        }
        public DelegateCommand<string> OpenGithubRepoCommand { get; set; }

        public void OpenGithubRepo(string url)
        {
            System.Diagnostics.Process.Start("explorer.exe", url);
        }
    }
}
