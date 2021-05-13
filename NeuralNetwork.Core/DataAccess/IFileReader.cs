using NeuralNetwork.Core.Models;

namespace NeuralNetwork.Core.DataAccess
{
    interface IFileReader
    {
        InputData ReadInputData(string path);
    }
}