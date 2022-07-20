using System;
using System.Drawing;
using ImageLad.ImageEngine.Analyze;

namespace ImageLad.Controls
{
    public sealed class UiGrayHistogram
    {
        private bool _visible;

        public static UiGrayHistogram Extend(GrayHistogram hist, Color color, bool visible)
        {
            var histEx = new UiGrayHistogram();
            histEx.Histogram = hist;
            histEx.Color = color;
            histEx.Visible = visible;
            return histEx;
        }

        public GrayHistogram Histogram { get; set; }

        public Color Color { get; set; }

        public bool Visible
        {
            get => _visible;
            set
            {
                _visible = value;
                OnVisibleChanged();
            }
        }

        public event EventHandler VisibleChanged;

        private void OnVisibleChanged()
        {
            VisibleChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}