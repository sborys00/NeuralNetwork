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
using System.Text.Json;
using System;

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
            (string[] names, bool[] outputs) = SelectOutputsAndVariableNames(await _fileReader.GetVariableCount(fileName));
            List<int> outputIndexes = new();
            for (int i = 0; i < outputs.Length; i++)
            {
                if (outputs[i])
                    outputIndexes.Add(i);
            }
            TrainingDataset data = await _fileReader.ReadInputData(fileName, outputIndexes.ToArray());
            data.VariableNames = names;
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

        private (string[] names, bool[] outputs) SelectOutputsAndVariableNames(int variableCount)
        {
            string message = message = $"Enter variable names and specify outputs";
            string[] names = Array.Empty<string>();
            bool[] outputs = Array.Empty<bool>(); 
            _dialogService.ShowDialog("DataPickerDialogView", new DialogParameters { { "message", message }, { "count", variableCount } },
                r =>
                {
                    names = JsonSerializer.Deserialize<string[]>(r.Parameters.GetValue<string>("names"));
                    outputs = JsonSerializer.Deserialize<bool[]>(r.Parameters.GetValue<string>("outputs"));
                });
            return (names, outputs);
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
