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
using Page = System.Windows.Controls.Page;

namespace ZongziTEK.BlackboardSticker.Pages.SettingsPages.InfoBoardSettingsPages
{
    /// <summary>
    /// WeatherSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class WeatherSettingsPage : Page
    {
        public WeatherSettingsPage()
        {
            InitializeComponent();

            DataContext = MainWindow.Settings.InfoBoard;
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
        }

        private void ToggleSwitchIsRainForecastOnly_Toggled(object sender, RoutedEventArgs e)
        {
            MainWindow.SaveSettings();
        }
    }
}
