using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Rebyu.Converters;

public class BoolToObjectConverter : IValueConverter
{
    public object TrueValue { get; set; }
    public object FalseValue { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolVal)
        {
            return boolVal ? TrueValue : FalseValue;
        }
        return AvaloniaProperty.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
