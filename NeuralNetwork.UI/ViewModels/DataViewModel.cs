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
using System.Data;

namespace NeuralNetwork.UI.ViewModels
{
    public class DataViewModel : BindableBase
    {
        private readonly IFileReader _fileReader;

        public DelegateCommand LoadFileCommand { get; set; }

        private DataTable dataTable;

        public DataTable DataTable
        {
            get { return dataTable; }
            set { SetProperty(ref dataTable, value); }
        }


        public DataViewModel(IFileReader fileReader)
        {
            _fileReader = fileReader;
            LoadFileCommand = new DelegateCommand(LoadFile);
        }
        public async void LoadFile()
        {
            string fileName = SelectDataFile();
            TrainingDataset data = await _fileReader.ReadInputData(fileName, new int[] { 3 });
            DataTable = GenerateDataTable(data);
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

        private DataTable GenerateDataTable(TrainingDataset trainingDataset)
        {
            DataTable dataTable = new();
            List<TrainingDataExample> dataset = trainingDataset.Dataset.ToList();
            int inputCount = dataset[0].inputValues.Length;
            int outputCount = dataset[0].expectedOutputs.Length;

            dataTable.Columns.Add(new DataColumn() { ColumnName = $"Test", DataType = typeof(bool) });
            for (int i = 0; i < inputCount + outputCount; i++)
            {
                if(i < inputCount)
                    dataTable.Columns.Add(new DataColumn() { ColumnName = $"in {i}", DataType = typeof(double) });
                else
                    dataTable.Columns.Add(new DataColumn() { ColumnName = $"out {i - inputCount}", DataType = typeof(double) });
            }

            for (int i = 0; i < dataset.Count; i++)
            {
                DataRow row = dataTable.NewRow();
                row[0] = false;
                int j;
                for (j = 0; j < inputCount + outputCount;  j++)
                {
                    if(j < inputCount)
                        row[j + 1] = dataset[i].inputValues[j];
                    else
                        row[j + 1] = dataset[i].expectedOutputs[j - inputCount];
                }
                dataTable.Rows.Add(row);
            }
            return dataTable;
        }
    }
}
