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

namespace ZongziTEK.BlackboardSticker.Pages.SettingsPages
{
    /// <summary>
    /// AboutPage.xaml 的交互逻辑
    /// </summary>
    public partial class AboutPage : Page
    {
        public AboutPage()
        {
            InitializeComponent();

            DataContext = MainWindow.Settings.Update;

            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            TextBlockVersion.Text = version.ToString();
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
        }
    }
}
