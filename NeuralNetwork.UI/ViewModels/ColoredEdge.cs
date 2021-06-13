using QuikGraph;
using QuikGraph.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NeuralNetwork.UI.ViewModels
{
    class ColoredEdge<T> : Edge<T>
    {
        public SolidColorBrush Brush { get; set; }

        public ColoredEdge(T source, T target, SolidColorBrush brush) : base(source, target)
        {
            Brush = brush;
        }
    }
}
