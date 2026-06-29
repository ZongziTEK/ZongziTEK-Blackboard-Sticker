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

        public WeatherSettingsPage()
        {
            InitializeComponent();

            DataContext = MainWindow.Settings.InfoBoard;
            UpdateResetButtons();
        }

        private async void ButtonEditWeatherCity_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog weatherCityPickerDialog = new()
            {
                Title = "选择城市或行政区",
                CloseButtonText = "完成",
                DefaultButton = ContentDialogButton.Close,
                Content = new Controls.DialogContents.WeatherCityPicker()
            };
            await weatherCityPickerDialog.ShowAsync();
            UpdateResetButtons();
        }

        private void ToggleSwitchIsRainForecastOnly_Toggled(object sender, RoutedEventArgs e)
        {
            MainWindow.SaveSettings();
            UpdateResetButtons();
        }

        private void CardWeatherCity_ResetClicked(object sender, RoutedEventArgs e)
        {
            MainWindow.Settings.InfoBoard.WeatherCity = defaultSettings.InfoBoard.WeatherCity;
            MainWindow.SaveSettings();
            UpdateResetButtons();
        }

        private void ToggleSwitchIsRainForecastOnly_ResetClicked(object sender, RoutedEventArgs e)
        {
            MainWindow.Settings.InfoBoard.IsRainForecastOnly = defaultSettings.InfoBoard.IsRainForecastOnly;
            ToggleSwitchIsRainForecastOnly_Toggled(sender, e);
        }

        private void UpdateResetButtons()
        {
            if (CardWeatherCity == null || ToggleSwitchIsRainForecastOnly == null) return;

            CardWeatherCity.IsResetEnabled = MainWindow.Settings.InfoBoard.WeatherCity != defaultSettings.InfoBoard.WeatherCity;
            ToggleSwitchIsRainForecastOnly.IsResetEnabled = MainWindow.Settings.InfoBoard.IsRainForecastOnly != defaultSettings.InfoBoard.IsRainForecastOnly;
        }
    }
}
