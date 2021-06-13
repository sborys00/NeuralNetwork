using NeuralNetwork.Core.Models;
using System.Threading.Tasks;

namespace NeuralNetwork.Core.DataAccess
{
    public interface ISaveFileReader
    {
        Task<Save> ReadSave(string path);
        Task WriteSave(string path, Save save);
    }
}