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
        /// <summary>
        /// List of arrays containing input values and expected result at the end of it. 
        /// </summary>
        public IEnumerable<double[]> DataSet; 
    }
}
