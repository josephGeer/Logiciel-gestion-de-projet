using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace app_test.Utilitaire
{
    public class NumberTaskToProgressBar: IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Projet project)
            {
                int number_Task = project.Tasks?.Count() ?? 0;
                int number_TaskDone = project.TasksHistory?.Count() ?? 0;
                int total = number_Task + number_TaskDone;

                if (total == 0)
                    return 0.0;

                double progress_bar = (number_TaskDone * 100.0) / total;
                return progress_bar;
            }

            return 100.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
