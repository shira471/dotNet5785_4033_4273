using System;
using System.Globalization;
using System.Windows.Data;

namespace PL.Converters
{
    public class SimulatorButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isRunning)
                return isRunning ? "Stop Simulator" : "Start Simulator";
            return "Start Simulator";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}