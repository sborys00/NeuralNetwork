﻿using NeuralNetwork.Core.Models;
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

        public async Task<InputData> ReadInputData(string path)
        {
            InputData inputData = new();
            List<double[]> data = new();
            CultureInfo culture = new("en-US");
            try
            {
                using StreamReader sr = _fileSystem.File.OpenText(path);
                while (!sr.EndOfStream)
                {
                    string line = await sr.ReadLineAsync();
                    string[] valuesStr = line.Split(",");
                    double[] values = new double[valuesStr.Length];
                    for (int i = 0; i < values.Length; i++)
                    {
                        values[i] = Convert.ToDouble(valuesStr[i], culture);
                    }
                    data.Add(values);
                }
                
            }
            catch
            {
                throw;
            }
            inputData.DataSet = data;
            return inputData;
        }
    }
}
