using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace ImageLad.ViewModels
{
    public class ChartViewModel : ObservableRecipient
    {
        public ChartViewModel()
        {
            var count = 200;
            var random = new Random((int) DateTime.Now.Ticks);
            for (int i = 0; i < count; i++)
            {
                var a = random.Next(100, 150);
                var b = random.Next(100, 10000);
                var c = double.Parse($"{a}.{b}");
                DataSource.Add(c);
            }
        }

        public ObservableCollection<double> DataSource { get; set; } = new();

        public Point Location { get; private set; }

        public void SetStartLocation(Point location)
        {
            Location = location;
        }
    }
}
