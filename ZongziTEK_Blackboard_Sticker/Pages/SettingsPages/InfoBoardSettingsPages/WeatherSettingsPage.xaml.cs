using iNKORE.UI.WPF.Modern.Controls;
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
using Page = System.Windows.Controls.Page;

namespace ZongziTEK_Blackboard_Sticker.Pages.SettingsPages.InfoBoardSettingsPages
{
    /// <summary>
    /// WeatherSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class WeatherSettingsPage : Page
    {
        private readonly Settings defaultSettings = new Settings();
        private readonly List<SettingsResetItem> resetItems = new List<SettingsResetItem>();
        private SettingsResetItem resetWeatherCity;
        private SettingsResetItem resetRainForecastOnly;

        public WeatherSettingsPage()
        {
            InitializeComponent();

            DataContext = MainWindow.Settings.InfoBoard;
            InitializeResetItems();
            UpdateResetButtons();
        }

        private async void ButtonEditWeatherCity_Click(object sender, RoutedEventArgs e)
        {
            Controls.DialogContents.WeatherCityPicker weatherCityPicker = new(MainWindow.Settings.InfoBoard.WeatherCity);
            ContentDialog weatherCityPickerDialog = new()
            {
                Title = "选择城市或行政区",
                CloseButtonText = "完成",
                DefaultButton = ContentDialogButton.Close,
                Content = weatherCityPicker
            };
            await weatherCityPickerDialog.ShowAsync();

            if (!string.IsNullOrWhiteSpace(weatherCityPicker.SelectedCityCode)
                && weatherCityPicker.SelectedCityCode != MainWindow.Settings.InfoBoard.WeatherCity)
            {
                MainWindow.Settings.InfoBoard.WeatherCity = weatherCityPicker.SelectedCityCode;
                ApplyWeatherSettings();
                return;
            }

            UpdateResetButtons();
        }

        private void ToggleSwitchIsRainForecastOnly_Toggled(object sender, RoutedEventArgs e)
        {
            ApplyWeatherSettings();
        }

        private void InitializeResetItems()
        {
            resetWeatherCity = SettingsResetItem.Register(
                resetItems,
                enabled => CardWeatherCity.IsResetEnabled = enabled,
                () => MainWindow.Settings.InfoBoard.WeatherCity,
                () => defaultSettings.InfoBoard.WeatherCity,
                value => MainWindow.Settings.InfoBoard.WeatherCity = value,
                ApplyWeatherSettings);

            resetRainForecastOnly = SettingsResetItem.Register(
                resetItems,
                enabled => ToggleSwitchIsRainForecastOnly.IsResetEnabled = enabled,
                () => MainWindow.Settings.InfoBoard.IsRainForecastOnly,
                () => defaultSettings.InfoBoard.IsRainForecastOnly,
                value => MainWindow.Settings.InfoBoard.IsRainForecastOnly = value,
                ApplyWeatherSettings);
        }

        private void ApplyWeatherSettings()
        {
            if (SettingsResetItem.IsResetting) return;

            MainWindow.SaveSettings();
            UpdateResetButtons();
        }

        private void CardWeatherCity_ResetClicked(object sender, RoutedEventArgs e)
        {
            resetWeatherCity.Reset();
        }

        private void ToggleSwitchIsRainForecastOnly_ResetClicked(object sender, RoutedEventArgs e)
        {
            resetRainForecastOnly.Reset();
        }

        private void UpdateResetButtons()
        {
            SettingsResetItem.UpdateAll(resetItems);
        }
    }
}
