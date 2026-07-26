using Microsoft.UI.Xaml.Data;
using System;

namespace app_test.Utilitaire
{
    public class PlayPauseIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isRunning = value is bool b && b;
            return isRunning ? "\uE769" : "\uE768";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}