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

namespace ZongziTEK_Blackboard_Sticker.Pages.SettingsPages.InfoBoardSettingsPages
{
    /// <summary>
    /// InfoBoardGenericSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class InfoBoardGenericSettingsPage : Page
    {
        private readonly Settings defaultSettings = new Settings();

        public InfoBoardGenericSettingsPage()
        {
            InitializeComponent();

            DataContext = MainWindow.Settings.InfoBoard;

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
            MainWindow.SaveSettings();
            (Application.Current.MainWindow as MainWindow)?.ApplyInfoBoardSwitchInterval();
            UpdateResetButtons();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.SaveSettings();

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

            (Application.Current.MainWindow as MainWindow).LoadFrameInfoPagesList();
            UpdateResetButtons();
        }

        private void CardSwitchInterval_ResetClicked(object sender, RoutedEventArgs e)
        {
            MainWindow.Settings.InfoBoard.SwitchIntervalSeconds = defaultSettings.InfoBoard.SwitchIntervalSeconds;
            SliderSwitchInterval_ValueChanged(sender, e);
        }

        private void ButtonResetInfoPages_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Settings.InfoBoard.isDatePageEnabled = defaultSettings.InfoBoard.isDatePageEnabled;
            MainWindow.Settings.InfoBoard.isCountdownPageEnabled = defaultSettings.InfoBoard.isCountdownPageEnabled;
            MainWindow.Settings.InfoBoard.isWeatherPageEnabled = defaultSettings.InfoBoard.isWeatherPageEnabled;
            MainWindow.Settings.InfoBoard.isWeatherForecastPageEnabled = defaultSettings.InfoBoard.isWeatherForecastPageEnabled;

            MainWindow.SaveSettings();
            (Application.Current.MainWindow as MainWindow).LoadFrameInfoPagesList();
            UpdateResetButtons();
            e.Handled = true;
        }

        private void UpdateResetButtons()
        {
            if (CardSwitchInterval == null || ButtonResetInfoPages == null) return;

            InfoBoard settings = MainWindow.Settings.InfoBoard;
            InfoBoard defaults = defaultSettings.InfoBoard;

            CardSwitchInterval.IsResetEnabled = !AreClose(settings.SwitchIntervalSeconds, defaults.SwitchIntervalSeconds);
            ButtonResetInfoPages.IsEnabled =
                settings.isDatePageEnabled != defaults.isDatePageEnabled ||
                settings.isCountdownPageEnabled != defaults.isCountdownPageEnabled ||
                settings.isWeatherPageEnabled != defaults.isWeatherPageEnabled ||
                settings.isWeatherForecastPageEnabled != defaults.isWeatherForecastPageEnabled;
        }

        private static bool AreClose(double a, double b)
        {
            return Math.Abs(a - b) < 0.0000001;
        }
    }
}
