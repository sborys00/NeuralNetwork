using NeuralNetwork.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Abstractions;
using System.Globalization;

namespace NeuralNetwork.Core.DataAccess
{
    public class CSVReader : IFileReader
    {
        private readonly IFileSystem _fileSystem;

        public CSVReader(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        public CSVReader() : this(new FileSystem()) { }
        public async Task<TrainingDataset> ReadInputData(string path, int[] outputIndexes)
        {
            TrainingDataset inputData = new();
            List<TrainingDataExample> data = new();
            CultureInfo culture = new("en-US");
            try
            {
                using StreamReader sr = _fileSystem.File.OpenText(path);
                while (!sr.EndOfStream)
                {
                    string line = await sr.ReadLineAsync();
                    string[] valuesStr = line.Split(",");

                    if (inputData.VariableNames == null)
                        inputData.VariableNames = new string[valuesStr.Length];

                    double[] values = new double[valuesStr.Length];
                    
                    List<double> inputs = new();
                    List<double> outputs = new();
                    for (int i = 0; i < values.Length; i++)
                    {
                        double value = Convert.ToDouble(valuesStr[i], culture);
                        if (outputIndexes.Contains(i))
                            outputs.Add(value);
                        else
                            inputs.Add(value);
                    }

                    data.Add(new TrainingDataExample(inputs.ToArray(), outputs.ToArray()));
                }
            }
            catch
            {
                throw;
            }
            inputData.TrainingExamples = data;
            inputData.TestExamples = new List<TrainingDataExample>();
            return inputData;
        }
        
        public async Task WriteInputData(TrainingDataset dataset, string path)
        {
            try
            {
                using StreamWriter sw = _fileSystem.File.CreateText(path);
                foreach(var example in dataset.TrainingExamples)
                {
                    await sw.WriteLineAsync(ConvertExampleToString(example));
                }
                foreach (var example in dataset.TestExamples)
                {
                    await sw.WriteLineAsync(ConvertExampleToString(example));
                }
            }
            catch
            {
                throw;
            }
        }
        public async Task<int> GetVariableCount(string path)
        {
            using StreamReader sr = _fileSystem.File.OpenText(path);
            string line = await sr.ReadLineAsync();
            string[] valuesStr = line.Split(",");
            return valuesStr.Length;
        }

        private string ConvertExampleToString(TrainingDataExample example)
        {
            CultureInfo culture = new("en-US");
            string[] inputs = example.InputValues.Select(n => n.ToString(culture)).ToArray();
            string[] outputs = example.ExpectedOutputs.Select(n => n.ToString(culture)).ToArray();
            string content = string.Join(',', inputs) + "," + string.Join(',', outputs);
            return content;
        }
    }
}
