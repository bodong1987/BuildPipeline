using Avalonia.Data.Converters;
using Avalonia;
using System.Globalization;

namespace BuildPipeline.GUI.Utils
{
    /// <summary>
    /// Calls <see cref="string.Format(string, object[])"/> on the passed in values, where the first element in the list
    /// is the string, and everything after it is passed into the object array in order.
    /// </summary>
    public class StringFormatConverter : IMultiValueConverter
    {
        /// <summary>
        /// Converts multi-binding inputs to a final value.
        /// </summary>
        /// <param name="values">The values to convert.</param>
        /// <param name="targetType">The type of the target.</param>
        /// <param name="parameter">A user-defined parameter.</param>
        /// <param name="culture">The culture to use.</param>
        /// <returns>The converted value.</returns>
        /// <remarks>This method should not throw exceptions. If the value is not convertible, return
        /// <see cref="F:Avalonia.AvaloniaProperty.UnsetValue" />. Any exception thrown will be treated as
        /// an application exception.</remarks>
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is string format)
            {
                try
                {
                    return string.Format(format, values.Skip(1).ToArray());
                }
                catch
                {
                    return AvaloniaProperty.UnsetValue;
                }
            }
            return AvaloniaProperty.UnsetValue;
        }
    }

}
