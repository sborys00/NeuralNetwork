using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.Core.Models
{
    public class LearningManager : ILearningManager
    {
        public int Epoch { get; }

        /// <summary>
        /// Should be between 0.0 to 1.0.
        /// </summary>
        public double LearningRate { get; set; }
        public Func<double, double> ActivationFunction { get; set; }
        public List<IEnumerable<double>> TrainingSet { get; set; }
        public List<IEnumerable<double>> TestSet { get; set; }

        public Network Network { get; set; }

        public void TrainForMultipleEpochs(int numberOfEpochs)
        {
            throw new NotImplementedException();
        }

        public void TrainForOneEpoch()
        {
            throw new NotImplementedException();
        }
    }
}
