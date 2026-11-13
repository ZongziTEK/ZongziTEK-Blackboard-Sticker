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

namespace ZongziTEK_Blackboard_Sticker.Pages.SettingsPages
{
    /// <summary>
    /// AboutPage.xaml 的交互逻辑
    /// </summary>
    public partial class AboutPage : Page
    {
        public AboutPage()
        {
            InitializeComponent();

            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            TextBlockVersion.Text = version.ToString();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/STBBRD/ZongziTEK-Blackboard-Sticker");
        }

        private void OpenForkReleases_Click(object sender, RoutedEventArgs e)
        {
            try { Process.Start("https://github.com/JoshZ-zzx/Blackboard-Sticker/releases"); }
            catch (System.ComponentModel.Win32Exception ex)
            {
                iNKORE.UI.WPF.Modern.Controls.MessageBox.Show(ex.Message, "无法打开发布页");
            }
        }
    }
}
