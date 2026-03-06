using iNKORE.UI.WPF.Modern;
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
using ZongziTEK.BlackboardSticker.Helpers;

namespace ZongziTEK.BlackboardSticker.Pages.WelcomePages
{
    /// <summary>
    /// WelcomePage1.xaml 的交互逻辑
    /// </summary>
    public partial class WelcomePage1 : Page
    {
        public WelcomePage1()
        {
            InitializeComponent();

            selectedBackgroundKey = ThemeKeys.CardBackgroundFillColorDefaultBrushKey;
            selectedBorderBrushKey = ThemeKeys.ButtonBorderBrushKey;

            LoadSettings();
        }

        private string selectedBackgroundKey;
        private string selectedBorderBrushKey;

        private void LoadSettings()
        {
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\ZongziTEK_Blackboard_Sticker" + ".lnk"))
            {
                ControlsHelper.SetDynamicResource(BorderYes, Border.BackgroundProperty, selectedBackgroundKey);
                ControlsHelper.SetDynamicResource(BorderYes, Border.BorderBrushProperty, selectedBorderBrushKey);
            }
            else
            {
                ControlsHelper.SetDynamicResource(BorderNo, BackgroundProperty, selectedBackgroundKey);
                ControlsHelper.SetDynamicResource(BorderNo, Border.BorderBrushProperty, selectedBorderBrushKey);
            }
        }

        private void BorderYes_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MainWindow.StartAutomaticallyCreate("ZongziTEK_Blackboard_Sticker");

            ControlsHelper.SetDynamicResource(BorderYes, Border.BackgroundProperty, selectedBackgroundKey);
            ControlsHelper.SetDynamicResource(BorderYes, Border.BorderBrushProperty, selectedBorderBrushKey);
            BorderNo.Background = new SolidColorBrush(Colors.Transparent);
            BorderNo.BorderBrush = null;
        }

        private void BorderNo_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MainWindow.StartAutomaticallyDel("ZongziTEK_Blackboard_Sticker");

            ControlsHelper.SetDynamicResource(BorderNo, Border.BackgroundProperty, selectedBackgroundKey);
            ControlsHelper.SetDynamicResource(BorderNo, Border.BorderBrushProperty, selectedBorderBrushKey);
            BorderYes.Background = new SolidColorBrush(Colors.Transparent);
            BorderYes.BorderBrush = null;
        }
    }
}
