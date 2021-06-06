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
using Prism.Services.Dialogs;

namespace NeuralNetwork.UI.ViewModels
{
    public class DataViewModel : BindableBase
    {
        private readonly IFileReader _fileReader;
        private readonly IDialogService _dialogService;

        public DataViewModel(IFileReader fileReader, IDialogService dialogService)
        {
            _fileReader = fileReader;
            _dialogService = dialogService;
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

            string message = message = $"Enter {data.VariableNames.Count()} variable names separeted with comma(,)";
            _dialogService.ShowDialog("DataPickerDialogView", new DialogParameters($"message={message}"),
                r =>
                {
                    string dialogInput = r.Parameters.GetValue<string>("input");
                    data.VariableNames = dialogInput.Split(',');
                });
            DataTable = GenerateDataTable(data);
        }

        public void SaveTableData()
        {
            TrainingDataset dataset = GetExamplesFromTable(dataTable, 3);
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
            List<TrainingDataExample> dataset = trainingDataset.TrainingExamples.ToList();
            dataset.AddRange(trainingDataset.TestExamples);
            int inputCount = dataset[0].inputValues.Length;
            int outputCount = dataset[0].expectedOutputs.Length;

            dataTable.Columns.Add(new DataColumn() { ColumnName = $"Test", DataType = typeof(bool) });
            for (int i = 0; i < trainingDataset.VariableNames.Count(); i++)
            {
                    dataTable.Columns.Add(new DataColumn() { ColumnName = trainingDataset.VariableNames.ElementAt(i), DataType = typeof(double) });
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

        private TrainingDataset GetExamplesFromTable(DataTable table, int inputCount)
        {
            List<TrainingDataExample> trainingExamples = new();
            List<TrainingDataExample> testExamples = new();
            List<string> variableNames = new();
            foreach(DataColumn col in table.Columns)
            {
                //skip first column
                if (table.Columns.IndexOf(col) == 0)
                    continue;

                variableNames.Add(col.ColumnName);
            }
            foreach(DataRow row in table.Rows)
            {
                double[] values = new double[row.ItemArray.Length-1];
                for (int i = 1; i < row.ItemArray.Length; i++)
                {
                    try
                    {
                        values[i - 1] = row.Field<double>(i);
                    }
                    catch
                    {
                        break;
                    }
                }
                TrainingDataExample example = new(values[..inputCount], values[inputCount..]);
                if (row.Field<bool>(0))
                    testExamples.Add(example);
                else
                    trainingExamples.Add(example);
            }

            return new TrainingDataset()
            {
                TrainingExamples = trainingExamples,
                TestExamples = testExamples,
                VariableNames = variableNames
            };
        }
    }
}
