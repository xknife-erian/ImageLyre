using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace ImageLad.Views.Converters;

[ValueConversion(typeof(double[]), typeof(IEnumerable<object>))]
public class DoubleArray2Model : IValueConverter
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
        // if (array == null || array.Count() <= 0)
        //     return new PlotModel();
        // var model = new PlotModel();
        // var lineSerial = new LineSeries {Title = "Hello"};
        // for (var i = 0; i < array.Count(); i++)
        //     lineSerial.Points.Add(new DataPoint(i, array.ElementAt(i)));
        // model.Series.Add(lineSerial);
        // return model;
        return null;
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