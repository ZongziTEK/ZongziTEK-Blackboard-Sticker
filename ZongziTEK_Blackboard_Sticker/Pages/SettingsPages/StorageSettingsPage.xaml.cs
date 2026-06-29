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

namespace ZongziTEK_Blackboard_Sticker.Pages.SettingsPages
{
    /// <summary>
    /// StorageSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class StorageSettingsPage : Page
    {
        private readonly Settings defaultSettings = new Settings();

        public StorageSettingsPage()
        {
            InitializeComponent();

            DataContext = MainWindow.Settings.Storage;
            UpdateResetButtons();
        }

        private void ToggleSwitchIsFilesSavingWithProgram_Toggled(object sender, RoutedEventArgs e)
        {
            MainWindow.SaveSettings();
            UpdateResetButtons();
        }

        private void TextBoxDataPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            MainWindow.Settings.Storage.DataPath = TextBoxDataPath.Text;            
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
            MainWindow.Settings.Storage.IsFilesSavingWithProgram = defaultSettings.Storage.IsFilesSavingWithProgram;
            ToggleSwitchIsFilesSavingWithProgram_Toggled(sender, e);
        }

        private void ButtonResetDataPath_Click(object sender, RoutedEventArgs e)
        {
            TextBoxDataPath.Text = defaultSettings.Storage.DataPath;
            e.Handled = true;
        }

        private void UpdateResetButtons()
        {
            if (ToggleSwitchIsFilesSavingWithProgram == null || ButtonResetDataPath == null) return;

            ToggleSwitchIsFilesSavingWithProgram.IsResetEnabled = MainWindow.Settings.Storage.IsFilesSavingWithProgram != defaultSettings.Storage.IsFilesSavingWithProgram;
            ButtonResetDataPath.IsEnabled = MainWindow.Settings.Storage.DataPath != defaultSettings.Storage.DataPath;
        }
    }
}
