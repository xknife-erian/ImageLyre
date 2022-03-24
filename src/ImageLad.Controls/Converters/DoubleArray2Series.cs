using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace ImageLad.Controls.Converters;

[ValueConversion(typeof(IEnumerable<double>), typeof(IEnumerable<ISeries[]>))]
public class DoubleArray2Series : IValueConverter
{
    #region Implementation of IValueConverter

    /// <summary>Converts a value.</summary>
    /// <param name="value">The value produced by the binding source.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>A converted value. If the method returns <see langword="null" />, the valid null value is used.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var array = value as IEnumerable<double>;
        if (array == null)
            return new ColumnSeries<double>();
        var series = new ColumnSeries<double>
        {
            Values = array.ToArray()
        };
        return new ISeries[] {series};
    }

    /// <summary>Converts a value.</summary>
    /// <param name="value">The value that is produced by the binding target.</param>
    /// <param name="targetType">The type to convert to.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>A converted value. If the method returns <see langword="null" />, the valid null value is used.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    #endregion
}