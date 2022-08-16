using System;
using System.Drawing;
using ImageLyric.ImageEngine.Analyze;

namespace ImageLyric.UI.Controls;

public sealed class UiGrayHistogram
{
    public UiGrayHistogram()
    {
        Id = Guid.NewGuid();
    }

    public Guid Id { get; }

    private bool _visible;

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

    public static UiGrayHistogram Extend(GrayHistogram hist, Color color, bool visible)
    {
        var histEx = new UiGrayHistogram();
        histEx.Histogram = hist;
        histEx.Color = color;
        histEx.Visible = visible;
        return histEx;
    }

    public event EventHandler VisibleChanged;

    private void OnVisibleChanged()
    {
        VisibleChanged?.Invoke(this, EventArgs.Empty);
    }

    #region Overrides of Object

    /// <summary>Determines whether the specified object is equal to the current object.</summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>
    /// <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not UiGrayHistogram hist)
            return false;
        return Equals(hist);
    }

    #region Equality members

    private bool Equals(UiGrayHistogram other)
    {
        return Id.Equals(other.Id);
    }

    /// <summary>Serves as the default hash function.</summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    #endregion

    #endregion
}