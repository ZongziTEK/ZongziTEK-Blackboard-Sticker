using System;
using System.Collections.Generic;
using System.IO;
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
    /// BehaviorSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class BehaviorSettingsPage : Page
    {
        public BehaviorSettingsPage()
        {
            InitializeComponent();

            LoadSettings();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        private void ButtonRestart_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Restart();
        }

        private void ToggleSwitchRunOnStartup_Toggled(object sender, RoutedEventArgs e)
        {
            if (ToggleSwitchRunOnStartup.IsOn)
            {
                MainWindow.StartAutomaticallyCreate("ZongziTEK_Blackboard_Sticker");
            }
            else
            {
                MainWindow.StartAutomaticallyDel("ZongziTEK_Blackboard_Sticker");
            }
        }

        private void LoadSettings()
        {
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\ZongziTEK_Blackboard_Sticker" + ".lnk"))
            {
                ToggleSwitchRunOnStartup.IsOn = true;
            }
        }
    }
}
