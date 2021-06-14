using NeuralNetwork.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NeuralNetwork.Core.DataAccess
{
    public class SaveFileReader : ISaveFileReader
    {
        private readonly IFileSystem _fileSystem;

        public SaveFileReader(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        public SaveFileReader() : this(new FileSystem()) { }
        public async Task<Save> ReadSave(string path)
        {
            using StreamReader sr = _fileSystem.File.OpenText(path);
            string json = await sr.ReadToEndAsync();
            Save save = JsonSerializer.Deserialize<Save>(json);
            return save;
        }
        public async Task WriteSave(string path, Save save)
        {
            using StreamWriter sw = _fileSystem.File.CreateText(path);
            try
            {
                string json = JsonSerializer.Serialize(save);
                await sw.WriteAsync(json);
            }
            catch(Exception e)
            {
                throw;
            }
        }
    }
}
