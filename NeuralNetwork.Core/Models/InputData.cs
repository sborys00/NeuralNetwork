using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.Core.Models
{
    public class InputData
    {
        public IEnumerable<string> VariableNames;
        public IEnumerable<double[]> DataSet; 
    }
}
