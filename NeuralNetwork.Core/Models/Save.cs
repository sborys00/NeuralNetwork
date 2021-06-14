using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.Core.Models
{
    public class Save
    {
        public Network Network { get; set; }
        public TrainingDataset TrainingDataset { get; set; }
        public TrainingConfig TrainingConfig { get; set; }
    }
}
