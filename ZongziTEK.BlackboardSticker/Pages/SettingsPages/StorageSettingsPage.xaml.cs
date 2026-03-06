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

namespace ZongziTEK.BlackboardSticker.Pages.SettingsPages
{
    /// <summary>
    /// StorageSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class StorageSettingsPage : Page
    {
        public StorageSettingsPage()
        {
            InitializeComponent();

            DataContext = MainWindow.Settings.Storage;
        }

        private void ToggleSwitchIsFilesSavingWithProgram_Toggled(object sender, RoutedEventArgs e)
        {
            MainWindow.SaveSettings();
        }

        private void TextBoxDataPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            MainWindow.Settings.Storage.DataPath = TextBoxDataPath.Text;            
            MainWindow.SaveSettings();
        }

        private void ButtonBrowse_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowser.ShowDialog();
            TextBoxDataPath.Text = folderBrowser.SelectedPath;
        }
    }
}
