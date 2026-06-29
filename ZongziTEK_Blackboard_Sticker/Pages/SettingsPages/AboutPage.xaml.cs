using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
    /// AboutPage.xaml 的交互逻辑
    /// </summary>
    public partial class AboutPage : Page
    {
        private readonly Settings defaultSettings = new Settings();

        public AboutPage()
        {
            InitializeComponent();

            DataContext = MainWindow.Settings.Update;

            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            TextBlockVersion.Text = version.ToString();
            UpdateResetButtons();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/STBBRD/ZongziTEK-Blackboard-Sticker");
        }

        private void ToggleSwitchAutoUpdate_Toggled(object sender, RoutedEventArgs e)
        {
            MainWindow.SaveSettings();

            if (MainWindow.Settings.Update.IsUpdateAutomatic)
            {
                MainWindow.CheckUpdate();
            }

            UpdateResetButtons();
        }

        private void CardIsUpdateAutomatic_ResetClicked(object sender, RoutedEventArgs e)
        {
            MainWindow.Settings.Update.IsUpdateAutomatic = defaultSettings.Update.IsUpdateAutomatic;
            ToggleSwitchAutoUpdate_Toggled(sender, e);
        }

        private void UpdateResetButtons()
        {
            if (CardIsUpdateAutomatic == null) return;

            CardIsUpdateAutomatic.IsResetEnabled = MainWindow.Settings.Update.IsUpdateAutomatic != defaultSettings.Update.IsUpdateAutomatic;
        }
    }
}
