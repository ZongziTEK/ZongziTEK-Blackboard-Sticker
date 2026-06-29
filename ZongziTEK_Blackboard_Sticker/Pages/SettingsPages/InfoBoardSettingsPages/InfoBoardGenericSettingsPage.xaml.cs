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

namespace ZongziTEK_Blackboard_Sticker.Pages.SettingsPages.InfoBoardSettingsPages
{
    /// <summary>
    /// InfoBoardGenericSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class InfoBoardGenericSettingsPage : Page
    {
        private readonly Settings defaultSettings = new Settings();
        private readonly List<SettingsResetItem> resetItems = new List<SettingsResetItem>();
        private SettingsResetItem resetSwitchInterval;
        private SettingsResetItem resetInfoPages;

        public InfoBoardGenericSettingsPage()
        {
            InitializeComponent();

            DataContext = MainWindow.Settings.InfoBoard;
            InitializeResetItems();

            CheckBoxes = new List<CheckBox>
            {
                CheckBoxDate,
                CheckBoxCountdown,
                CheckBoxLiveWeather,
                CheckBoxCastWeather
            };

            UpdateResetButtons();
        }

        private List<CheckBox> CheckBoxes = new List<CheckBox>();

        private void SliderSwitchInterval_ValueChanged(object sender, RoutedEventArgs e)
        {
            ApplySwitchIntervalSettings();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            bool HasNoCheckBoxSelected = true;

            foreach (CheckBox checkBox in CheckBoxes)
            {
                if (checkBox.IsChecked == true)
                {
                    HasNoCheckBoxSelected = false;
                    break;
                }
            }

            if (HasNoCheckBoxSelected)
            {
                CheckBoxDate.IsChecked = true;
                return;
            }

            ApplyInfoPagesSettings();
        }

        private void InitializeResetItems()
        {
            resetSwitchInterval = SettingsResetItem.Register(
                resetItems,
                enabled => CardSwitchInterval.IsResetEnabled = enabled,
                () => MainWindow.Settings.InfoBoard.SwitchIntervalSeconds,
                () => defaultSettings.InfoBoard.SwitchIntervalSeconds,
                value => MainWindow.Settings.InfoBoard.SwitchIntervalSeconds = value,
                ApplySwitchIntervalSettings,
                SettingsResetItem.AreClose);

            resetInfoPages = SettingsResetItem.Register(
                resetItems,
                enabled => ButtonResetInfoPages.IsEnabled = enabled,
                HasInfoPagesChanged,
                ResetInfoPages,
                ApplyInfoPagesSettings);
        }

        private void ApplySwitchIntervalSettings()
        {
            MainWindow.SaveSettings();
            (Application.Current.MainWindow as MainWindow)?.ApplyInfoBoardSwitchInterval();
            UpdateResetButtons();
        }

        private void ApplyInfoPagesSettings()
        {
            MainWindow.SaveSettings();
            (Application.Current.MainWindow as MainWindow).LoadFrameInfoPagesList();
            UpdateResetButtons();
        }

        private bool HasInfoPagesChanged()
        {
            InfoBoard settings = MainWindow.Settings.InfoBoard;
            InfoBoard defaults = defaultSettings.InfoBoard;

            return settings.isDatePageEnabled != defaults.isDatePageEnabled ||
                   settings.isCountdownPageEnabled != defaults.isCountdownPageEnabled ||
                   settings.isWeatherPageEnabled != defaults.isWeatherPageEnabled ||
                   settings.isWeatherForecastPageEnabled != defaults.isWeatherForecastPageEnabled;
        }

        private void ResetInfoPages()
        {
            MainWindow.Settings.InfoBoard.isDatePageEnabled = defaultSettings.InfoBoard.isDatePageEnabled;
            MainWindow.Settings.InfoBoard.isCountdownPageEnabled = defaultSettings.InfoBoard.isCountdownPageEnabled;
            MainWindow.Settings.InfoBoard.isWeatherPageEnabled = defaultSettings.InfoBoard.isWeatherPageEnabled;
            MainWindow.Settings.InfoBoard.isWeatherForecastPageEnabled = defaultSettings.InfoBoard.isWeatherForecastPageEnabled;
        }

        private void CardSwitchInterval_ResetClicked(object sender, RoutedEventArgs e)
        {
            resetSwitchInterval.Reset();
        }

        private void ButtonResetInfoPages_Click(object sender, RoutedEventArgs e)
        {
            resetInfoPages.Reset();
            e.Handled = true;
        }

        private void UpdateResetButtons()
        {
            SettingsResetItem.UpdateAll(resetItems);
        }
    }
}
