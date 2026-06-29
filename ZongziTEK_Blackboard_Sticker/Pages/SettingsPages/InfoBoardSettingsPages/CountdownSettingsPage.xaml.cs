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
    /// CountdownSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class CountdownSettingsPage : Page
    {
        private readonly Settings defaultSettings = new Settings();

        public CountdownSettingsPage()
        {
            InitializeComponent();

            DataContext = MainWindow.Settings.InfoBoard;
            UpdateResetButtons();
        }

        private void TextBoxName_TextChanged(object sender, RoutedEventArgs e)
        {
            MainWindow.SaveSettings();
            UpdateResetButtons();
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            MainWindow.SaveSettings();
            UpdateResetButtons();
        }

        private void SliderWarnThreshold_ValueChanged(object sender, RoutedEventArgs e)
        {
            MainWindow.SaveSettings();
            UpdateResetButtons();
        }

        private void TextBoxName_ResetClicked(object sender, RoutedEventArgs e)
        {
            MainWindow.Settings.InfoBoard.CountdownName = defaultSettings.InfoBoard.CountdownName;
            TextBoxName_TextChanged(sender, e);
        }

        private void CardCountdownDate_ResetClicked(object sender, RoutedEventArgs e)
        {
            MainWindow.Settings.InfoBoard.CountdownDate = defaultSettings.InfoBoard.CountdownDate;
            MainWindow.SaveSettings();
            UpdateResetButtons();
        }

        private void CardCountdownWarnDays_ResetClicked(object sender, RoutedEventArgs e)
        {
            MainWindow.Settings.InfoBoard.CountdownWarnDays = defaultSettings.InfoBoard.CountdownWarnDays;
            SliderWarnThreshold_ValueChanged(sender, e);
        }

        private void UpdateResetButtons()
        {
            if (TextBoxName == null || CardCountdownDate == null || CardCountdownWarnDays == null) return;

            InfoBoard settings = MainWindow.Settings.InfoBoard;
            InfoBoard defaults = defaultSettings.InfoBoard;

            TextBoxName.IsResetEnabled = settings.CountdownName != defaults.CountdownName;
            CardCountdownDate.IsResetEnabled = settings.CountdownDate != defaults.CountdownDate;
            CardCountdownWarnDays.IsResetEnabled = settings.CountdownWarnDays != defaults.CountdownWarnDays;
        }
    }
}
