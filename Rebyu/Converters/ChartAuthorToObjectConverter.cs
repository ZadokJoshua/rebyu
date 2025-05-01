using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Globalization;
using Avalonia.Media;
using Rebyu.Models;

namespace Rebyu.Converters;

public class ChatAuthorToObjectConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Message message)
        {
            var icon = message.SenderRole?.Equals("User", StringComparison.OrdinalIgnoreCase) == true
                ? "👤"
                : "🤖";
            
            var borderColor = message.Sender switch
            {
                "NL2SQL" => Brushes.CadetBlue,
                "ChartMaker" => Brushes.MediumPurple,
                "Coordinator" => Brushes.SeaGreen,
                "User" => Brushes.Gray,
                _ => Brushes.LightSlateGray
            };

            var senderName = message.Sender ?? "Unknown";

            // Return according to the requested parameter
            return parameter switch
            {
                "Icon" => icon,
                "SenderName" => senderName,
                "BorderBrush" => borderColor,
                _ => AvaloniaProperty.UnsetValue
            };
        }

        return AvaloniaProperty.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
