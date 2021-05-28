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
        public DataViewModel(IFileReader fileReader)
        {
            _fileReader = fileReader;
            LoadFileCommand = new DelegateCommand(LoadFile);
            SaveTableDataCommand = new DelegateCommand(SaveTableData);
        }

        public DelegateCommand LoadFileCommand { get; set; }
        public DelegateCommand SaveTableDataCommand { get; set; }

        private DataTable dataTable;

        public DataTable DataTable
        {
            get { return dataTable; }
            set { SetProperty(ref dataTable, value); }
        }
        public async void LoadFile()
        {
            string fileName = SelectDataFile();
            TrainingDataset data = await _fileReader.ReadInputData(fileName, new int[] { 3 });
            DataTable = GenerateDataTable(data);
        }

        public void SaveTableData()
        {
            List<TrainingDataExample> trainingExamples = new();
            List<TrainingDataExample> testExamples = new();

            GetExamplesFromTable(dataTable, 3 , ref trainingExamples, ref testExamples);
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

        private void GetExamplesFromTable(DataTable table, int inputCount, ref List<TrainingDataExample> training, ref List<TrainingDataExample> test)
        {
            foreach(DataRow row in table.Rows)
            {
                double[] values = new double[row.ItemArray.Length-1];
                for (int i = 1; i < row.ItemArray.Length; i++)
                {
                    values[i - 1] = row.Field<double>(i);

                }
                TrainingDataExample example = new(values[..inputCount], values[inputCount..]);
                if (row.Field<bool>(0))
                    test.Add(example);
                else
                    training.Add(example);
            }
        }
    }
}
