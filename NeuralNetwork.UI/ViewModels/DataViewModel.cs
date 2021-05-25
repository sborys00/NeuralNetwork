using Microsoft.Win32;
using NeuralNetwork.Core.DataAccess;
using NeuralNetwork.Core.Models;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace NeuralNetwork.UI.ViewModels
{
    public class DataViewModel : BindableBase
    {
        private readonly IFileReader _fileReader;

        public DelegateCommand LoadFileCommand { get; set; }


        private ObservableCollection<TrainingDataExample> trainingExamples;

        private ObservableCollection<TrainingDataExample> testExamples;

        private ObservableCollection<string> traningExamplesString;

        public DataViewModel(IFileReader fileReader)
        {
            _fileReader = fileReader;
            LoadFileCommand = new DelegateCommand(LoadFile);

            TraningExamples = new();
            TestExamples = new();
        }
        public ObservableCollection<TrainingDataExample> TraningExamples
        {
            get { return trainingExamples; }
            set
            {
                trainingExamples = value;
            }
        }

        public ObservableCollection<TrainingDataExample> TestExamples
        {
            get { return testExamples; }
            set 
            { 
                testExamples = value;
            }
        }
        public async void LoadFile()
        {
            string fileName = SelectDataFile();
            TrainingDataset data = await _fileReader.ReadInputData(fileName, new int[] { 1 });
            TraningExamples.AddRange(data.Dataset);
        }

        public string SelectDataFile()
        {
            OpenFileDialog dialog = new()
            {
                DefaultExt = ".csv",
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
            };
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                return dialog.FileName;
            }
            throw new FileNotFoundException("File could not be loaded");
        }
    }
}
