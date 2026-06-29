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
using ZongziTEK_Blackboard_Sticker.Helpers;

namespace ZongziTEK_Blackboard_Sticker.Pages.SettingsPages
{
    /// <summary>
    /// BehaviorSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class BehaviorSettingsPage : Page
    {
        private const string StartupShortcutName = "ZongziTEK_Blackboard_Sticker";
        private readonly List<SettingsResetItem> resetItems = new List<SettingsResetItem>();
        private SettingsResetItem resetRunOnStartup;

        public BehaviorSettingsPage()
        {
            InitializeComponent();

            InitializeResetItems();
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
            ApplyRunOnStartupSetting();
        }

        private void InitializeResetItems()
        {
            resetRunOnStartup = SettingsResetItem.Register(
                resetItems,
                enabled => ToggleSwitchRunOnStartup.IsResetEnabled = enabled,
                () => File.Exists(GetStartupShortcutPath()),
                () =>
                {
                    ToggleSwitchRunOnStartup.IsOn = false;
                    MainWindow.StartAutomaticallyDel(StartupShortcutName);
                },
                UpdateResetButtons);
        }

        private void ApplyRunOnStartupSetting()
        {
            if (ToggleSwitchRunOnStartup.IsOn)
            {
                MainWindow.StartAutomaticallyCreate(StartupShortcutName);
            }
            else
            {
                MainWindow.StartAutomaticallyDel(StartupShortcutName);
            }

            UpdateResetButtons();
        }

        private void LoadSettings()
        {
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\ZongziTEK_Blackboard_Sticker" + ".lnk"))
            {
                ToggleSwitchRunOnStartup.IsOn = true;
            }

            UpdateResetButtons();
        }

        private void ToggleSwitchRunOnStartup_ResetClicked(object sender, RoutedEventArgs e)
        {
            resetRunOnStartup.Reset();
        }

        private void UpdateResetButtons()
        {
            SettingsResetItem.UpdateAll(resetItems);
        }

        private static string GetStartupShortcutPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\" + StartupShortcutName + ".lnk";
        }
    }
}
