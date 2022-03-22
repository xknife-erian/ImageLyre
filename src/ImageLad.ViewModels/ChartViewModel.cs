using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace ImageLad.ViewModels
{
    public class ChartViewModel : ObservableRecipient
    {
        public IEnumerable<ISeries> Series { get; set; } = new ObservableCollection<ISeries>
        {
            new StepLineSeries<double?>
            {
                Values = new ObservableCollection<double?> { 2, 1, 3, 4, 3, 4, 6 },
                Fill = null
            }
        };

        public Point Location { get; private set; }

        public void SetStartLocation(Point location)
        {
            Location = location;
        }
    }
}
