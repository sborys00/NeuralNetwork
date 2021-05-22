using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.Core.Models
{
    public class TrainingDataset
    {
        public IEnumerable<string> VariableNames;
        public IEnumerable<TrainingDataExample> Dataset; 
    }
}
