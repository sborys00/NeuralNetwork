using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using NeuralNetwork.Core.DataAccess;
using NeuralNetwork.Core.Models;

namespace NeuralNetwork.Core.Tests
{
    public class IFileReaderTests
    {
        public object IFileReader { get; private set; }

        [Fact]
        public async void CSVReader_ShouldReadInputData()
        {
            string path = @"C:\data\input.txt";
            var mockFileSystem = new MockFileSystem();
            var mockInputFile = new MockFileData(
                "1.1,2.1,1.0\n" +
                "0.3,3.2,3.2\n" +
                "5.3,1.1,2.1\n");

            mockFileSystem.AddFile(@"C:\data\input.txt", mockInputFile);
            IFileReader fr = new CSVReader(mockFileSystem);
            TrainingDataset data = await fr.ReadInputData(path);
            Assert.Equal(1.1, data.Dataset.ElementAt(0).inputValues[0]);
            Assert.Equal(2.1, data.Dataset.ElementAt(0).inputValues[1]);
            Assert.Equal(1.0, data.Dataset.ElementAt(0).expectedOutput);
                                       
            Assert.Equal(0.3, data.Dataset.ElementAt(1).inputValues[0]);
            Assert.Equal(3.2, data.Dataset.ElementAt(1).inputValues[1]);
            Assert.Equal(3.2, data.Dataset.ElementAt(1).expectedOutput);
                                       
            Assert.Equal(5.3, data.Dataset.ElementAt(2).inputValues[0]);
            Assert.Equal(1.1, data.Dataset.ElementAt(2).inputValues[1]);
            Assert.Equal(2.1, data.Dataset.ElementAt(2).expectedOutput);
        }
    }
}
