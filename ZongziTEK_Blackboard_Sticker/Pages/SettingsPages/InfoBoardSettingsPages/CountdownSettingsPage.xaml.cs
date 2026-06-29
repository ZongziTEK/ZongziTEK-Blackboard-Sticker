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
    /// CountdownSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class CountdownSettingsPage : Page
    {
        private readonly Settings defaultSettings = new Settings();
        private readonly List<SettingsResetItem> resetItems = new List<SettingsResetItem>();
        private SettingsResetItem resetCountdownName;
        private SettingsResetItem resetCountdownDate;
        private SettingsResetItem resetCountdownWarnDays;

        public CountdownSettingsPage()
        {
            InitializeComponent();

            DataContext = MainWindow.Settings.InfoBoard;
            InitializeResetItems();
            UpdateResetButtons();
        }

        private void TextBoxName_TextChanged(object sender, RoutedEventArgs e)
        {
            ApplyCountdownSettings();
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyCountdownSettings();
        }

        private void SliderWarnThreshold_ValueChanged(object sender, RoutedEventArgs e)
        {
            ApplyCountdownSettings();
        }

        private void InitializeResetItems()
        {
            resetCountdownName = SettingsResetItem.Register(
                resetItems,
                enabled => TextBoxName.IsResetEnabled = enabled,
                () => MainWindow.Settings.InfoBoard.CountdownName,
                () => defaultSettings.InfoBoard.CountdownName,
                value => MainWindow.Settings.InfoBoard.CountdownName = value,
                ApplyCountdownSettings);

            resetCountdownDate = SettingsResetItem.Register(
                resetItems,
                enabled => CardCountdownDate.IsResetEnabled = enabled,
                () => MainWindow.Settings.InfoBoard.CountdownDate,
                () => defaultSettings.InfoBoard.CountdownDate,
                value => MainWindow.Settings.InfoBoard.CountdownDate = value,
                ApplyCountdownSettings);

            resetCountdownWarnDays = SettingsResetItem.Register(
                resetItems,
                enabled => CardCountdownWarnDays.IsResetEnabled = enabled,
                () => MainWindow.Settings.InfoBoard.CountdownWarnDays,
                () => defaultSettings.InfoBoard.CountdownWarnDays,
                value => MainWindow.Settings.InfoBoard.CountdownWarnDays = value,
                ApplyCountdownSettings);
        }

        private void ApplyCountdownSettings()
        {
            MainWindow.SaveSettings();
            UpdateResetButtons();
        }

        private void TextBoxName_ResetClicked(object sender, RoutedEventArgs e)
        {
            resetCountdownName.Reset();
        }

        private void CardCountdownDate_ResetClicked(object sender, RoutedEventArgs e)
        {
            resetCountdownDate.Reset();
        }

        private void CardCountdownWarnDays_ResetClicked(object sender, RoutedEventArgs e)
        {
            resetCountdownWarnDays.Reset();
        }

        private void UpdateResetButtons()
        {
            SettingsResetItem.UpdateAll(resetItems);
        }
    }
}
