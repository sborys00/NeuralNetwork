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

        public async Task<TrainingDataset> ReadInputData(string path, int[] outputIndexes)
        {
            TrainingDataset inputData = new();
            List<TrainingDataUnit> data = new();
            CultureInfo culture = new("en-US");
            try
            {
                using StreamReader sr = _fileSystem.File.OpenText(path);
                while (!sr.EndOfStream)
                {
                    string line = await sr.ReadLineAsync();
                    string[] valuesStr = line.Split(",");
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

                    data.Add(new TrainingDataUnit(inputs.ToArray(), outputs.ToArray()));
                }
            }
            catch
            {
                throw;
            }
            inputData.Dataset = data;
            return inputData;
        }
    }
}
