using NeuralNetwork.Core.Models;
using System.Threading.Tasks;

namespace NeuralNetwork.Core.DataAccess
{
    public interface IFileReader
    {
        /// <summary>
        /// Reads all data from file and convert it to TrainingDataset object.
        /// </summary>
        /// <param name="path">Path to data file</param>
        /// <param name="outputIndexes">Indexes of values that are outputs, starting with 0</param>
        /// <returns></returns>
        public Task<TrainingDataset> ReadInputData(string path, int[] outputIndexes);
        public Task<int> GetVariableCount(string path);
    }
}