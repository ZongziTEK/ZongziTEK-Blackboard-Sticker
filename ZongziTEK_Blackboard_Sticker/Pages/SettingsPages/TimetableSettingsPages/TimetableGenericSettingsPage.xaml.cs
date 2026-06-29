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
using ZongziTEK_Blackboard_Sticker.Models;

namespace ZongziTEK_Blackboard_Sticker.Pages.SettingsPages.TimetableSettingsPages
{
    /// <summary>
    /// TimetableGenericSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class TimetableGenericSettingsPage : Page
    {
        private readonly Settings defaultSettings = new Settings();

        public TimetableGenericSettingsPage()
        {
            InitializeComponent();

            DataContext = MainWindow.Settings.TimetableSettings;
            UpdateResetButtons();
        }

        private void ToggleSwitchUseTimetable_Toggled(object sender, RoutedEventArgs e)
        {
            MainWindow.SaveSettings();

            (Application.Current.MainWindow as MainWindow).LoadTimetableOrCurriculum();
            UpdateResetButtons();
        }

        private void ToggleSwitchIsTimetableNotificationEnabled_Toggled(object sender, RoutedEventArgs e)
        {
            MainWindow.SaveSettings();
            UpdateResetButtons();
        }

        private void SliderBeginNotificationTime_ValueChanged(object sender, RoutedEventArgs e)
        {
            MainWindow.SaveSettings();
            UpdateResetButtons();
        }

        private void SliderOverNotificationTime_ValueChanged(object sender, RoutedEventArgs e)
        {
            MainWindow.SaveSettings();
            UpdateResetButtons();
        }

        private void SliderFontSize_ValueChanged(object sender, RoutedEventArgs e)
        {
            MainWindow.SaveSettings();

            (Application.Current.MainWindow as MainWindow).LoadTimetableOrCurriculum();
            UpdateResetButtons();
        }

        private void SliderTimeOffset_ValueChanged(object sender, RoutedEventArgs e)
        {
            MainWindow.SaveSettings();
            UpdateResetButtons();
        }

        private void ToggleSwitchIsClickToHideNotificationEnabled_Toggled(object sender, RoutedEventArgs e)
        {
            MainWindow.SaveSettings();
            UpdateResetButtons();
        }

        private void CardFontSize_ResetClicked(object sender, RoutedEventArgs e)
        {
            MainWindow.Settings.TimetableSettings.FontSize = defaultSettings.TimetableSettings.FontSize;
            SliderFontSize_ValueChanged(sender, e);
        }

        private void ToggleSwitchUseTimetable_ResetClicked(object sender, RoutedEventArgs e)
        {
            MainWindow.Settings.TimetableSettings.IsTimetableEnabled = defaultSettings.TimetableSettings.IsTimetableEnabled;
            ToggleSwitchUseTimetable_Toggled(sender, e);
        }

        private void CardIsTimetableNotificationEnabled_ResetClicked(object sender, RoutedEventArgs e)
        {
            MainWindow.Settings.TimetableSettings.IsTimetableNotificationEnabled = defaultSettings.TimetableSettings.IsTimetableNotificationEnabled;
            ToggleSwitchIsTimetableNotificationEnabled_Toggled(sender, e);
        }

        private void CardBeginNotificationTime_ResetClicked(object sender, RoutedEventArgs e)
        {
            MainWindow.Settings.TimetableSettings.BeginNotificationTime = defaultSettings.TimetableSettings.BeginNotificationTime;
            SliderBeginNotificationTime_ValueChanged(sender, e);
        }

        private void CardOverNotificationTime_ResetClicked(object sender, RoutedEventArgs e)
        {
            MainWindow.Settings.TimetableSettings.OverNotificationTime = defaultSettings.TimetableSettings.OverNotificationTime;
            SliderOverNotificationTime_ValueChanged(sender, e);
        }

        private void CardTimeOffset_ResetClicked(object sender, RoutedEventArgs e)
        {
            MainWindow.Settings.TimetableSettings.TimeOffset = defaultSettings.TimetableSettings.TimeOffset;
            SliderTimeOffset_ValueChanged(sender, e);
        }

        private void ToggleSwitchIsClickToHideNotificationEnabled_ResetClicked(object sender, RoutedEventArgs e)
        {
            MainWindow.Settings.TimetableSettings.IsClickToHideNotificationEnabled = defaultSettings.TimetableSettings.IsClickToHideNotificationEnabled;
            ToggleSwitchIsClickToHideNotificationEnabled_Toggled(sender, e);
        }

        private void UpdateResetButtons()
        {
            if (CardFontSize == null || ToggleSwitchUseTimetable == null || CardIsTimetableNotificationEnabled == null ||
                CardBeginNotificationTime == null || CardOverNotificationTime == null || CardTimeOffset == null ||
                ToggleSwitchIsClickToHideNotificationEnabled == null) return;

            TimetableSettings settings = MainWindow.Settings.TimetableSettings;
            TimetableSettings defaults = defaultSettings.TimetableSettings;

            CardFontSize.IsResetEnabled = !AreClose(settings.FontSize, defaults.FontSize);
            ToggleSwitchUseTimetable.IsResetEnabled = settings.IsTimetableEnabled != defaults.IsTimetableEnabled;
            CardIsTimetableNotificationEnabled.IsResetEnabled = settings.IsTimetableNotificationEnabled != defaults.IsTimetableNotificationEnabled;
            CardBeginNotificationTime.IsResetEnabled = !AreClose(settings.BeginNotificationTime, defaults.BeginNotificationTime);
            CardOverNotificationTime.IsResetEnabled = !AreClose(settings.OverNotificationTime, defaults.OverNotificationTime);
            CardTimeOffset.IsResetEnabled = !AreClose(settings.TimeOffset, defaults.TimeOffset);
            ToggleSwitchIsClickToHideNotificationEnabled.IsResetEnabled = settings.IsClickToHideNotificationEnabled != defaults.IsClickToHideNotificationEnabled;
        }

        private static bool AreClose(double a, double b)
        {
            return Math.Abs(a - b) < 0.0000001;
        }
    }
}
