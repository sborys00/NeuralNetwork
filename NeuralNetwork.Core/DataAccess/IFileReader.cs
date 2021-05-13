using NeuralNetwork.Core.Models;
using System.Threading.Tasks;

namespace NeuralNetwork.Core.DataAccess
{
    interface IFileReader
    {
        public Task<InputData> ReadInputData(string path);
    }
}