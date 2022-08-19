﻿using System.Drawing;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace ImageLyre.UI.ViewModels
{
    public class OptionViewModel : ObservableRecipient
    {
        public Point Location { get; private set; }

        public void SetStartLocation(Point location)
        {
            Location = location;
        }
    }
}