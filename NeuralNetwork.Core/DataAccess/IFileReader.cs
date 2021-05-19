using NeuralNetwork.Core.Models;
using System.Threading.Tasks;

namespace NeuralNetwork.Core.DataAccess
{
    public interface IFileReader
    {
        public Task<TrainingDataset> ReadInputData(string path);
    }
}