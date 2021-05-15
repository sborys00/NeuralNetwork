using NeuralNetwork.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
namespace NeuralNetwork.Core.Tests
{
    public class ActivationFunctionTests
    {
        [Theory]
        [InlineData(1d, 0.73d)]
        [InlineData(2d, 0.88d)]
        [InlineData(-4d, 0.02d)]
        [InlineData(0d, 0.5d)]
        [InlineData(14d, 1d)]
        [InlineData(3d, 0.95d)]
        public void Sigmoid_ShouldReturnCorrectValues(double x, double expected)
        {
            ActivationFuntion func = new SigmoidActivationFunction();
            double actual = Math.Round(func.Function(x), 2);
            Assert.Equal(Math.Round(expected, 2),  actual);
        }

        [Theory]
        [InlineData(1d, 0.46d)]
        [InlineData(2d, 0.76d)]
        [InlineData(-4d, -0.96d)]
        [InlineData(0d, 0d)]
        [InlineData(14d, 1)]
        [InlineData(3d, 0.91)]
        public void SigmoidBipolar_ShouldReturnCorrectValues(double x, double expected)
        {
            ActivationFuntion func = new SigmoidBipolarActivationFunction();
            double actual = Math.Round(func.Function(x), 2);
            Assert.Equal(Math.Round(expected, 2), actual);
        }

        [Theory]
        [InlineData(1d, 1d)]
        [InlineData(2d, 2d)]
        [InlineData(-4d, 0d)]
        [InlineData(0d, 0d)]
        [InlineData(14d, 14d)]
        [InlineData(3d, 3d)]
        public void ReLU_ShouldReturnCorrectValues(double x, double expected)
        {
            ActivationFuntion func = new ReLUActivationFunction();
            double actual = Math.Round(func.Function(x), 2);
            Assert.Equal(Math.Round(expected, 2),  actual);
        }

        [Theory]
        [InlineData(1d, 0.76d)]
        [InlineData(2d, 0.96d)]
        [InlineData(-4d, -1d)]
        [InlineData(0d, 0d)]
        [InlineData(14d, 1d)]
        [InlineData(3d, 1d)]
        public void Tanh_ShouldReturnCorrectValues(double x, double expected)
        {
            ActivationFuntion func = new TanhActivationFunction();
            double actual = Math.Round(func.Function(x), 2);
            Assert.Equal(Math.Round(expected, 2), actual);
        }
    }
}
