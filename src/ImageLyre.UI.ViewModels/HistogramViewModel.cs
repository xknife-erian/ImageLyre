using System.Collections.ObjectModel;
using System.Drawing;
using ImageLyre.Utils;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace ImageLyre.UI.ViewModels
{
    public class HistogramViewModel : ObservableRecipient
    {
        public HistogramViewModel()
        {
            var count = 256;
            var random = new Random((int) DateTime.Now.Ticks);
            var src = DataGen.RandomNormal(random, count, 11, 55.45d, 100);
            for (int i = 0; i < src.Length; i++)
            {
                DataSource.Add(src[i]);
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
