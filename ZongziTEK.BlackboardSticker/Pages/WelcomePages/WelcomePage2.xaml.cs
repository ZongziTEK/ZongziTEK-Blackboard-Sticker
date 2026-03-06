using iNKORE.UI.WPF.Controls;
using iNKORE.UI.WPF.Modern;
using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.Win32;
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
using ZongziTEK.BlackboardSticker.Helpers;
using Page = System.Windows.Controls.Page;

namespace ZongziTEK.BlackboardSticker.Pages.WelcomePages
{
    /// <summary>
    /// WelcomePage2.xaml 的交互逻辑
    /// </summary>
    public partial class WelcomePage2 : Page
    {
        public WelcomePage2()
        {
            InitializeComponent();

            selectedBackgroundKey = ThemeKeys.AccentButtonBackgroundKey;
            selectedBorderBrushKey = ThemeKeys.AccentButtonBorderBrushKey;
            normalBackgroundKey = ThemeKeys.CardBackgroundFillColorDefaultBrushKey;
            normalBorderBrushKey = ThemeKeys.ButtonBorderBrushKey;

            UpdateInterfaceStateAndSaveSettings();

            SystemEvents.UserPreferenceChanged += SystemEvents_UserPregerenceChanged;
        }

        MainWindow mainWindow = Application.Current.MainWindow as MainWindow;

        private void SystemEvents_UserPregerenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            UpdateInterfaceStateAndSaveSettings();
        }

        private string selectedBackgroundKey;
        private string selectedBorderBrushKey;
        private string normalBackgroundKey;
        private string normalBorderBrushKey;

        private void BorderNormal_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MainWindow.Settings.Look.LookMode = 0;
            UpdateInterfaceStateAndSaveSettings();
        }

        private void BorderLiteWithClock_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MainWindow.Settings.Look.LookMode = 1;
            UpdateInterfaceStateAndSaveSettings();
        }

        private void BorderLiteWithInfoBoard_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MainWindow.Settings.Look.LookMode = 2;
            UpdateInterfaceStateAndSaveSettings();
        }

        private void UpdateInterfaceStateAndSaveSettings()
        {
            MainWindow.SaveSettings();
            mainWindow.SwitchLookMode(MainWindow.Settings.Look.LookMode);

            foreach (Border border in StackPanelOptions.Children)
            {
                ControlsHelper.SetDynamicResource(border, Border.BackgroundProperty, normalBackgroundKey);
                ControlsHelper.SetDynamicResource(border, Border.BorderBrushProperty, normalBorderBrushKey);

                if (ThemeManager.GetActualTheme(this) == ElementTheme.Light)
                {
                    ((border.Child as Grid).Children[0] as FontIcon).Foreground = new SolidColorBrush(Colors.Black);
                    foreach (Label label in ((border.Child as Grid).Children[1] as SimpleStackPanel).Children)
                    {
                        label.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }
                else
                {
                    ((border.Child as Grid).Children[0] as FontIcon).Foreground = new SolidColorBrush(Colors.White);
                    foreach (Label label in ((border.Child as Grid).Children[1] as SimpleStackPanel).Children)
                    {
                        label.Foreground = new SolidColorBrush(Colors.White);
                    }
                }
            }

            switch (MainWindow.Settings.Look.LookMode)
            {
                case 0:
                    SetSelectedStyle(BorderNormal);
                    break;
                case 1:
                    SetSelectedStyle(BorderLiteWithClock);
                    break;
                case 2:
                    SetSelectedStyle(BorderLiteWithInfoBoard);
                    break;
            }
        }

        private void SetSelectedStyle(Border border)
        {
            ControlsHelper.SetDynamicResource(border, Border.BackgroundProperty, selectedBackgroundKey);
            ControlsHelper.SetDynamicResource(border, Border.BorderBrushProperty, selectedBorderBrushKey);

            if (ThemeManager.GetActualTheme(this) == ElementTheme.Light)
            {
                ((border.Child as Grid).Children[0] as FontIcon).Foreground = new SolidColorBrush(Colors.White);
                foreach (Label label in ((border.Child as Grid).Children[1] as SimpleStackPanel).Children)
                {
                    label.Foreground = new SolidColorBrush(Colors.White);
                }
            }
            else
            {
                ((border.Child as Grid).Children[0] as FontIcon).Foreground = new SolidColorBrush(Colors.Black);
                foreach (Label label in ((border.Child as Grid).Children[1] as SimpleStackPanel).Children)
                {
                    label.Foreground = new SolidColorBrush(Colors.Black);
                }
            }
        }

        private void Page_ActualThemeChanged(object sender, RoutedEventArgs e)
        {
            UpdateInterfaceStateAndSaveSettings();
        }
    }
}
