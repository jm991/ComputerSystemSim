using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace ComputerSystemSim
{
    /// <summary>
    /// Converts negative slider values to fractions based on the inverse of the absolute value.
    /// </summary>
    public class NegativeToFractionalConverter : IValueConverter
    {
        /// <summary>
        /// Convert negative slider values to fractions.
        /// </summary>
        /// <param name="value">Input slider value</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns>If value positive, value; if value negative, fraction</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double)
            {
                double rounded = Math.Round((double)value);

                if (rounded < 0)
                {
                    rounded = 1 / Math.Abs(rounded);
                }

                return rounded;
            }

            return 0;
        }

        /// <summary>
        /// Converts from fractions back to negative slider value.
        /// </summary>
        /// <param name="value">Fractional input</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns>Negative slider value</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is double)
            {
                double doubleVal = (double)value;

                if (doubleVal < 0)
                {
                    return -1 / doubleVal;
                }

                return doubleVal;
            }

            return 0;
        }
    }

    /// <summary>
    /// Provides common string formatting operations for display in UI.
    /// </summary>
    public class StringFormatConverter : IValueConverter
    {
        /// <summary>
        /// Format a given string value with a parameter.
        /// </summary>
        /// <param name="value">String to format</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">Format style</param>
        /// <param name="language"></param>
        /// <returns>Formatted string</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // No format provided.
            if (parameter == null)
            {
                return value;
            }

            return String.Format((String)parameter, value);
        }

        /// <summary>
        /// Impossible to convert back.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
