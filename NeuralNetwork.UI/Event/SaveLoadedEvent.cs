﻿using NeuralNetwork.Core.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.UI.Event
{
    public class SaveLoadedEvent : PubSubEvent<Save>
    {
    }
}
