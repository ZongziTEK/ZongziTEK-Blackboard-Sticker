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

namespace ZongziTEK_Blackboard_Sticker.Pages.WelcomePages
{
    /// <summary>
    /// WelcomePage3.xaml 的交互逻辑
    /// </summary>
    public partial class WelcomePage3 : Page
    {
        public WelcomePage3()
        {
            InitializeComponent();

            DataContext = MainWindow.Settings.InfoBoard;
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
                MainWindow.SaveSettings();
            }
        }
    }
}
