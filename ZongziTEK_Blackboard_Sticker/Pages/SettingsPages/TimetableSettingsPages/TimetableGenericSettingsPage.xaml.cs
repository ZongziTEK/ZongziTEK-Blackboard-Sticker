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
using ZongziTEK_Blackboard_Sticker.Helpers;
using ZongziTEK_Blackboard_Sticker.Models;

namespace ZongziTEK_Blackboard_Sticker.Pages.SettingsPages.TimetableSettingsPages
{
    /// <summary>
    /// TimetableGenericSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class TimetableGenericSettingsPage : Page
    {
        private readonly Settings defaultSettings = new Settings();
        private readonly List<SettingsResetItem> resetItems = new List<SettingsResetItem>();
        private SettingsResetItem resetFontSize;
        private SettingsResetItem resetUseTimetable;
        private SettingsResetItem resetTimetableNotification;
        private SettingsResetItem resetBeginNotificationTime;
        private SettingsResetItem resetOverNotificationTime;
        private SettingsResetItem resetTimeOffset;
        private SettingsResetItem resetClickToHideNotification;

        public TimetableGenericSettingsPage()
        {
            InitializeComponent();

            DataContext = MainWindow.Settings.TimetableSettings;
            InitializeResetItems();
            UpdateResetButtons();
        }

        private void ToggleSwitchUseTimetable_Toggled(object sender, RoutedEventArgs e)
        {
            ApplyTimetableDisplaySettings();
        }

        private void ToggleSwitchIsTimetableNotificationEnabled_Toggled(object sender, RoutedEventArgs e)
        {
            ApplyTimetableSettings();
        }

        private void SliderBeginNotificationTime_ValueChanged(object sender, RoutedEventArgs e)
        {
            ApplyTimetableSettings();
        }

        private void SliderOverNotificationTime_ValueChanged(object sender, RoutedEventArgs e)
        {
            ApplyTimetableSettings();
        }

        private void SliderFontSize_ValueChanged(object sender, RoutedEventArgs e)
        {
            ApplyTimetableDisplaySettings();
        }

        private void SliderTimeOffset_ValueChanged(object sender, RoutedEventArgs e)
        {
            ApplyTimetableSettings();
        }

        private void ToggleSwitchIsClickToHideNotificationEnabled_Toggled(object sender, RoutedEventArgs e)
        {
            ApplyTimetableSettings();
        }

        private void InitializeResetItems()
        {
            resetFontSize = SettingsResetItem.Register(
                resetItems,
                enabled => CardFontSize.IsResetEnabled = enabled,
                () => MainWindow.Settings.TimetableSettings.FontSize,
                () => defaultSettings.TimetableSettings.FontSize,
                value => MainWindow.Settings.TimetableSettings.FontSize = value,
                ApplyTimetableDisplaySettings,
                SettingsResetItem.AreClose);

            resetUseTimetable = SettingsResetItem.Register(
                resetItems,
                enabled => ToggleSwitchUseTimetable.IsResetEnabled = enabled,
                () => MainWindow.Settings.TimetableSettings.IsTimetableEnabled,
                () => defaultSettings.TimetableSettings.IsTimetableEnabled,
                value => MainWindow.Settings.TimetableSettings.IsTimetableEnabled = value,
                ApplyTimetableDisplaySettings);

            resetTimetableNotification = SettingsResetItem.Register(
                resetItems,
                enabled => CardIsTimetableNotificationEnabled.IsResetEnabled = enabled,
                () => MainWindow.Settings.TimetableSettings.IsTimetableNotificationEnabled,
                () => defaultSettings.TimetableSettings.IsTimetableNotificationEnabled,
                value => MainWindow.Settings.TimetableSettings.IsTimetableNotificationEnabled = value,
                ApplyTimetableSettings);

            resetBeginNotificationTime = SettingsResetItem.Register(
                resetItems,
                enabled => CardBeginNotificationTime.IsResetEnabled = enabled,
                () => MainWindow.Settings.TimetableSettings.BeginNotificationTime,
                () => defaultSettings.TimetableSettings.BeginNotificationTime,
                value => MainWindow.Settings.TimetableSettings.BeginNotificationTime = value,
                ApplyTimetableSettings,
                SettingsResetItem.AreClose);

            resetOverNotificationTime = SettingsResetItem.Register(
                resetItems,
                enabled => CardOverNotificationTime.IsResetEnabled = enabled,
                () => MainWindow.Settings.TimetableSettings.OverNotificationTime,
                () => defaultSettings.TimetableSettings.OverNotificationTime,
                value => MainWindow.Settings.TimetableSettings.OverNotificationTime = value,
                ApplyTimetableSettings,
                SettingsResetItem.AreClose);

            resetTimeOffset = SettingsResetItem.Register(
                resetItems,
                enabled => CardTimeOffset.IsResetEnabled = enabled,
                () => MainWindow.Settings.TimetableSettings.TimeOffset,
                () => defaultSettings.TimetableSettings.TimeOffset,
                value => MainWindow.Settings.TimetableSettings.TimeOffset = value,
                ApplyTimetableSettings,
                SettingsResetItem.AreClose);

            resetClickToHideNotification = SettingsResetItem.Register(
                resetItems,
                enabled => ToggleSwitchIsClickToHideNotificationEnabled.IsResetEnabled = enabled,
                () => MainWindow.Settings.TimetableSettings.IsClickToHideNotificationEnabled,
                () => defaultSettings.TimetableSettings.IsClickToHideNotificationEnabled,
                value => MainWindow.Settings.TimetableSettings.IsClickToHideNotificationEnabled = value,
                ApplyTimetableSettings);
        }

        private void ApplyTimetableDisplaySettings()
        {
            if (SettingsResetItem.IsResetting) return;

            MainWindow.SaveSettings();
            (Application.Current.MainWindow as MainWindow).LoadTimetableOrCurriculum();
            UpdateResetButtons();
        }

        private void ApplyTimetableSettings()
        {
            if (SettingsResetItem.IsResetting) return;

            MainWindow.SaveSettings();
            UpdateResetButtons();
        }

        private void CardFontSize_ResetClicked(object sender, RoutedEventArgs e)
        {
            resetFontSize.Reset();
        }

        private void ToggleSwitchUseTimetable_ResetClicked(object sender, RoutedEventArgs e)
        {
            resetUseTimetable.Reset();
        }

        private void CardIsTimetableNotificationEnabled_ResetClicked(object sender, RoutedEventArgs e)
        {
            resetTimetableNotification.Reset();
        }

        private void CardBeginNotificationTime_ResetClicked(object sender, RoutedEventArgs e)
        {
            resetBeginNotificationTime.Reset();
        }

        private void CardOverNotificationTime_ResetClicked(object sender, RoutedEventArgs e)
        {
            resetOverNotificationTime.Reset();
        }

        private void CardTimeOffset_ResetClicked(object sender, RoutedEventArgs e)
        {
            resetTimeOffset.Reset();
        }

        private void ToggleSwitchIsClickToHideNotificationEnabled_ResetClicked(object sender, RoutedEventArgs e)
        {
            resetClickToHideNotification.Reset();
        }

        private void UpdateResetButtons()
        {
            SettingsResetItem.UpdateAll(resetItems);
        }
    }
}
