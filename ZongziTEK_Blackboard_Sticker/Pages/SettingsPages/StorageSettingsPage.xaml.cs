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

namespace ZongziTEK_Blackboard_Sticker.Pages.SettingsPages
{
    /// <summary>
    /// StorageSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class StorageSettingsPage : Page
    {
        private readonly Settings defaultSettings = new Settings();
        private readonly List<SettingsResetItem> resetItems = new List<SettingsResetItem>();
        private SettingsResetItem resetFilesSavingWithProgram;
        private SettingsResetItem resetDataPath;

        public StorageSettingsPage()
        {
            InitializeComponent();

            DataContext = MainWindow.Settings.Storage;
            InitializeResetItems();
            UpdateResetButtons();
        }

        private void ToggleSwitchIsFilesSavingWithProgram_Toggled(object sender, RoutedEventArgs e)
        {
            ApplyStorageSettings();
        }

        private void TextBoxDataPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            MainWindow.Settings.Storage.DataPath = TextBoxDataPath.Text;
            ApplyStorageSettings();
        }

        private void InitializeResetItems()
        {
            resetFilesSavingWithProgram = SettingsResetItem.Register(
                resetItems,
                enabled => ToggleSwitchIsFilesSavingWithProgram.IsResetEnabled = enabled,
                () => MainWindow.Settings.Storage.IsFilesSavingWithProgram,
                () => defaultSettings.Storage.IsFilesSavingWithProgram,
                value => MainWindow.Settings.Storage.IsFilesSavingWithProgram = value,
                ApplyStorageSettings);

            resetDataPath = SettingsResetItem.Register(
                resetItems,
                enabled => ButtonResetDataPath.IsEnabled = enabled,
                () => MainWindow.Settings.Storage.DataPath,
                () => defaultSettings.Storage.DataPath,
                value => MainWindow.Settings.Storage.DataPath = value,
                ApplyStorageSettings);
        }

        private void ApplyStorageSettings()
        {
            MainWindow.SaveSettings();
            UpdateResetButtons();
        }

        private void ButtonBrowse_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowser.ShowDialog();
            TextBoxDataPath.Text = folderBrowser.SelectedPath;
        }

        private void ToggleSwitchIsFilesSavingWithProgram_ResetClicked(object sender, RoutedEventArgs e)
        {
            resetFilesSavingWithProgram.Reset();
        }

        private void ButtonResetDataPath_Click(object sender, RoutedEventArgs e)
        {
            resetDataPath.Reset();
            e.Handled = true;
        }

        private void UpdateResetButtons()
        {
            SettingsResetItem.UpdateAll(resetItems);
        }
    }
}
