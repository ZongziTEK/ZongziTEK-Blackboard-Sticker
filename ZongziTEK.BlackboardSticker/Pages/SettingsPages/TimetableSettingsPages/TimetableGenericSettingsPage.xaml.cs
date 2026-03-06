using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ZongziTEK.BlackboardSticker.Pages.SettingsPages.TimetableSettingsPages
{
    /// <summary>
    /// TimetableGenericSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class TimetableGenericSettingsPage : Page
    {
        public TimetableGenericSettingsPage()
        {
            InitializeComponent();

            DataContext = MainWindow.Settings.TimetableSettings;
        }

        private void ToggleSwitchUseTimetable_Toggled(object sender, RoutedEventArgs e)
        {
            MainWindow.SaveSettings();

            (Application.Current.MainWindow as MainWindow).LoadTimetableOrCurriculum();
        }

        private void ToggleSwitchIsTimetableNotificationEnabled_Toggled(object sender, RoutedEventArgs e)
        {
            MainWindow.SaveSettings();
        }

        private void SliderBeginNotificationTime_ValueChanged(object sender, RoutedEventArgs e)
        {
            MainWindow.SaveSettings();
        }

        private void SliderOverNotificationTime_ValueChanged(object sender, RoutedEventArgs e)
        {
            MainWindow.SaveSettings();
        }

        private void SliderFontSize_ValueChanged(object sender, RoutedEventArgs e)
        {
            MainWindow.SaveSettings();

            (Application.Current.MainWindow as MainWindow).LoadTimetableOrCurriculum();
        }

        private void SliderTimeOffset_ValueChanged(object sender, RoutedEventArgs e)
        {
            MainWindow.SaveSettings();
        }

        private void ToggleSwitchIsClickToHideNotificationEnabled_Toggled(object sender, RoutedEventArgs e)
        {
            MainWindow.SaveSettings();
        }
    }
}
