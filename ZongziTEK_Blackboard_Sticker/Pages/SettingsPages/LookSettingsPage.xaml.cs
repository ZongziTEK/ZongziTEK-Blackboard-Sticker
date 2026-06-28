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
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using ZongziTEK_Blackboard_Sticker.Helpers;
using ZongziTEK_Blackboard_Sticker.Services;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace ZongziTEK_Blackboard_Sticker.Pages.SettingsPages
{
    /// <summary>
    /// LookSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class LookSettingsPage : Page
    {
        public ObservableCollection<MonitorItem> Monitors { get; set; } = new();
        private bool isPageReady = false;

        public LookSettingsPage()
        {
            InitializeComponent();

            LoadMonitors();
            DataContext = MainWindow.Settings.Look;

            Loaded += LookSettingsPage_Loaded;
            Unloaded += LookSettingsPage_Unloaded;
        }

        private void LookSettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                isPageReady = true;
            }), DispatcherPriority.Loaded);
        }

        private void LookSettingsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            isPageReady = false;
        }

        private void LoadMonitors()
        {
            Monitors.Clear();
            List<WindowsHelper.RECT> monitorRects = new();
            WindowsHelper.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMonitor, IntPtr hdcMonitor, ref WindowsHelper.RECT lprcMonitor, IntPtr dwData) =>
            {
                monitorRects.Add(lprcMonitor);
                return true;
            }, IntPtr.Zero);

            uint pathCount, modeCount;
            WindowsHelper.GetDisplayConfigBufferSizes(WindowsHelper.QDC_ONLY_ACTIVE_PATHS, out pathCount, out modeCount);
            var paths = new WindowsHelper.DISPLAYCONFIG_PATH_INFO[pathCount];
            var modes = new WindowsHelper.DISPLAYCONFIG_MODE_INFO[modeCount];
            WindowsHelper.QueryDisplayConfig(WindowsHelper.QDC_ONLY_ACTIVE_PATHS, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);

            for (int i = 0; i < monitorRects.Count; i++)
            {
                string name = $"屏幕 {i + 1}：";
                if (i < paths.Length)
                {
                    var targetName = new WindowsHelper.DISPLAYCONFIG_TARGET_DEVICE_NAME();
                    targetName.header.size = (uint)Marshal.SizeOf(typeof(WindowsHelper.DISPLAYCONFIG_TARGET_DEVICE_NAME));
                    targetName.header.adapterId = paths[i].targetInfo.adapterId;
                    targetName.header.id = paths[i].targetInfo.id;
                    targetName.header.type = WindowsHelper.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME;
                    if (WindowsHelper.DisplayConfigGetDeviceInfo(ref targetName) == 0)
                    {
                        name += targetName.monitorFriendlyDeviceName;
                    }
                }
                var rect = monitorRects[i];
                Monitors.Add(new MonitorItem { Name = $"{name} ({rect.Right - rect.Left}*{rect.Bottom - rect.Top})", Index = i });
            }
        }

        public class MonitorItem
        {
            public string Name { get; set; }
            public int Index { get; set; }
        }

        private void ComboBoxMonitor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyLookSettings();
        }

        private bool isCurrentWindowChromeDisabled = MainWindow.Settings.Look.IsWindowChromeDisabled;
        MainWindow mainWindow = Application.Current.MainWindow as MainWindow;

        private void ComboBoxTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isPageReady) return;

            MainWindow.SaveSettings();
            MainWindow.SetTheme();
        }

        private void ComboBoxLookMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyLookSettings();
        }

        private void ToggleSwitchIsLauncherEnabled_Toggled(object sender, RoutedEventArgs e)
        {
            ApplyLookSettings();
        }

        private void ToggleSwitchIsWindowHeightAdjustmentEnabled_Toggled(object sender, RoutedEventArgs e)
        {
            ApplyLookSettings();
        }

        private void SliderWindowHeightPercent_ValueChanged(object sender, RoutedEventArgs e)
        {
            ApplyLookSettings();
        }

        private void ComboBoxWindowVerticalAlignment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyLookSettings();
        }

        private void SliderWindowScaleMultiplier_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (!isPageReady) return;

            MainWindow.SaveSettings();
            MainWindow.SetWindowScaleTransform(MainWindow.Settings.Look.WindowScaleMultiplier);
        }

        private void SliderWindowScaleMultiplier_ValueChangeStart(object sender, RoutedEventArgs e)
        {
            if (!isPageReady || mainWindow == null) return;

            if (MainWindow.Settings.Look.LookMode != 0) mainWindow.SwitchLookMode(0);
        }

        private void SliderWindowScaleMultiplier_ValueChangeEnd(object sender, RoutedEventArgs e)
        {
            if (!isPageReady || mainWindow == null) return;

            if (MainWindow.Settings.Look.LookMode != 0) mainWindow.SwitchLookMode(MainWindow.Settings.Look.LookMode);
        }

        private void ApplyLookSettings()
        {
            if (!isPageReady || mainWindow == null) return;

            MainWindow.SaveSettings();
            mainWindow.SwitchLookMode(MainWindow.Settings.Look.LookMode);

            var classIslandConnectorService = App.ServiceManager.GetService<ClassIslandConnectorService>();
            if (classIslandConnectorService != null) _ = classIslandConnectorService.RefreshIslandTerritory();
        }

        private void ToggleSwitchIsWindowChromeDisabled_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isPageReady) return;

            MainWindow.SaveSettings();

            if (ToggleSwitchIsWindowChromeDisabled.IsOn != isCurrentWindowChromeDisabled) MessageBox.Show("此更改需重启黑板贴后生效", "ZongziTEK 黑板贴", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
