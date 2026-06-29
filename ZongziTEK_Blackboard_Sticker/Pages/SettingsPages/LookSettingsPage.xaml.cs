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
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ZongziTEK_Blackboard_Sticker.Helpers;
using ZongziTEK_Blackboard_Sticker.Models;
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
        public ObservableCollection<BackgroundStyleCategoryEditor> BackgroundStyleCategories { get; } = new();
        private readonly Settings defaultSettings = new Settings();
        private readonly List<SettingsResetItem> resetItems = new List<SettingsResetItem>();
        private SettingsResetItem resetWindowScaleMultiplier;
        private SettingsResetItem resetTheme;
        private SettingsResetItem resetBackgroundStyle;
        private SettingsResetItem resetComponentTitleTextHidden;
        private SettingsResetItem resetLookMode;
        private SettingsResetItem resetLauncherEnabled;
        private SettingsResetItem resetWindowHeightAdjustment;
        private SettingsResetItem resetWindowVerticalAlignment;
        private SettingsResetItem resetTargetMonitor;
        private SettingsResetItem resetWindowChromeDisabled;
        private bool isPageReady = false;

        public LookSettingsPage()
        {
            InitializeComponent();

            LoadMonitors();
            BuildBackgroundStyleCategories();
            DataContext = MainWindow.Settings.Look;
            InitializeResetItems();

            Loaded += LookSettingsPage_Loaded;
            Unloaded += LookSettingsPage_Unloaded;
        }

        private void LookSettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                isPageReady = true;
                UpdateCustomBackgroundEditorVisibility();
                UpdateResetButtons();
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

        private void InitializeResetItems()
        {
            resetWindowScaleMultiplier = SettingsResetItem.Register(
                resetItems,
                enabled => CardWindowScaleMultiplier.IsResetEnabled = enabled,
                () => MainWindow.Settings.Look.WindowScaleMultiplier,
                () => defaultSettings.Look.WindowScaleMultiplier,
                value => MainWindow.Settings.Look.WindowScaleMultiplier = value,
                ApplyWindowScaleSettings,
                SettingsResetItem.AreClose);

            resetTheme = SettingsResetItem.Register(
                resetItems,
                enabled => CardTheme.IsResetEnabled = enabled,
                () => MainWindow.Settings.Look.Theme,
                () => defaultSettings.Look.Theme,
                value => MainWindow.Settings.Look.Theme = value,
                ApplyThemeSettings);

            resetBackgroundStyle = SettingsResetItem.Register(
                resetItems,
                enabled => CardBackgroundStyle.IsResetEnabled = enabled,
                HasBackgroundStyleChanged,
                ResetBackgroundStyle,
                ApplyVisualSettings);

            resetComponentTitleTextHidden = SettingsResetItem.Register(
                resetItems,
                enabled => CardIsComponentTitleTextHidden.IsResetEnabled = enabled,
                () => MainWindow.Settings.Look.IsComponentTitleTextHidden,
                () => defaultSettings.Look.IsComponentTitleTextHidden,
                value => MainWindow.Settings.Look.IsComponentTitleTextHidden = value,
                ApplyVisualSettings);

            resetLookMode = SettingsResetItem.Register(
                resetItems,
                enabled => CardLookMode.IsResetEnabled = enabled,
                () => MainWindow.Settings.Look.LookMode,
                () => defaultSettings.Look.LookMode,
                value => MainWindow.Settings.Look.LookMode = value,
                ApplyLookSettings);

            resetLauncherEnabled = SettingsResetItem.Register(
                resetItems,
                enabled => CardIsLauncherEnabled.IsResetEnabled = enabled,
                () => MainWindow.Settings.Look.IsLauncherEnabled,
                () => defaultSettings.Look.IsLauncherEnabled,
                value => MainWindow.Settings.Look.IsLauncherEnabled = value,
                ApplyLookSettings);

            resetWindowHeightAdjustment = SettingsResetItem.Register(
                resetItems,
                enabled => CardWindowHeightAdjustment.IsResetEnabled = enabled,
                HasWindowHeightAdjustmentChanged,
                ResetWindowHeightAdjustment,
                ApplyLookSettings);

            resetWindowVerticalAlignment = SettingsResetItem.Register(
                resetItems,
                enabled => CardWindowVerticalAlignment.IsResetEnabled = enabled,
                () => MainWindow.Settings.Look.WindowVerticalAlignment,
                () => defaultSettings.Look.WindowVerticalAlignment,
                value => MainWindow.Settings.Look.WindowVerticalAlignment = value,
                ApplyLookSettings);

            resetTargetMonitor = SettingsResetItem.Register(
                resetItems,
                enabled => CardTargetMonitor.IsResetEnabled = enabled,
                () => MainWindow.Settings.Look.TargetMonitor,
                () => defaultSettings.Look.TargetMonitor,
                value => MainWindow.Settings.Look.TargetMonitor = value,
                ApplyLookSettings);

            resetWindowChromeDisabled = SettingsResetItem.Register(
                resetItems,
                enabled => ToggleSwitchIsWindowChromeDisabled.IsResetEnabled = enabled,
                () => MainWindow.Settings.Look.IsWindowChromeDisabled,
                () => defaultSettings.Look.IsWindowChromeDisabled,
                value => MainWindow.Settings.Look.IsWindowChromeDisabled = value,
                ApplyWindowChromeSettings);
        }

        private void ComboBoxMonitor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyLookSettings();
        }

        private bool isCurrentWindowChromeDisabled = MainWindow.Settings.Look.IsWindowChromeDisabled;
        MainWindow mainWindow = Application.Current.MainWindow as MainWindow;

        private void ComboBoxTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyThemeSettings();
        }

        private void ComboBoxBackgroundStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedIndex >= 0)
            {
                MainWindow.Settings.Look.BackgroundStyle = comboBox.SelectedIndex;
            }

            UpdateCustomBackgroundEditorVisibility();
            ApplyVisualSettings();
        }

        private void ComboBoxLookMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyLookSettings();
        }

        private void ToggleSwitchIsLauncherEnabled_Toggled(object sender, RoutedEventArgs e)
        {
            ApplyLookSettings();
        }

        private void ToggleSwitchIsComponentTitleTextHidden_Toggled(object sender, RoutedEventArgs e)
        {
            ApplyVisualSettings();
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
            ApplyWindowScaleSettings();
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

            UpdateResetButtons();
        }

        private void ApplyWindowScaleSettings()
        {
            if (!isPageReady) return;

            MainWindow.SaveSettings();
            MainWindow.SetWindowScaleTransform(MainWindow.Settings.Look.WindowScaleMultiplier);
            UpdateResetButtons();
        }

        private void ApplyVisualSettings()
        {
            if (!isPageReady || mainWindow == null) return;

            MainWindow.SaveSettings();
            mainWindow.ApplyVisualSettings();
            UpdateResetButtons();
        }

        private void ApplyThemeSettings()
        {
            if (!isPageReady) return;

            MainWindow.SaveSettings();
            MainWindow.SetTheme();
            UpdateResetButtons();
        }

        private void ToggleSwitchIsWindowChromeDisabled_Toggled(object sender, RoutedEventArgs e)
        {
            ApplyWindowChromeSettings();
        }

        private void ApplyWindowChromeSettings()
        {
            if (!isPageReady) return;

            MainWindow.SaveSettings();
            UpdateResetButtons();

            if (MainWindow.Settings.Look.IsWindowChromeDisabled != isCurrentWindowChromeDisabled) MessageBox.Show("此更改需重启黑板贴后生效", "ZongziTEK 黑板贴", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UpdateResetButtons()
        {
            SettingsResetItem.UpdateAll(resetItems);
        }

        private bool HasBackgroundStyleChanged()
        {
            Look settings = MainWindow.Settings.Look;
            Look defaults = defaultSettings.Look;

            return settings.BackgroundStyle != defaults.BackgroundStyle ||
                   !AreCustomBackgroundStylesEqual(settings.CustomBackgroundStyle, defaults.CustomBackgroundStyle);
        }

        private void ResetBackgroundStyle()
        {
            MainWindow.Settings.Look.BackgroundStyle = defaultSettings.Look.BackgroundStyle;
            MainWindow.Settings.Look.CustomBackgroundStyle = new CustomBackgroundStyle();
            BuildBackgroundStyleCategories();
            UpdateCustomBackgroundEditorVisibility();
        }

        private bool HasWindowHeightAdjustmentChanged()
        {
            Look settings = MainWindow.Settings.Look;
            Look defaults = defaultSettings.Look;

            return settings.IsWindowHeightAdjustmentEnabled != defaults.IsWindowHeightAdjustmentEnabled ||
                   !SettingsResetItem.AreClose(settings.WindowHeightPercent, defaults.WindowHeightPercent);
        }

        private void ResetWindowHeightAdjustment()
        {
            MainWindow.Settings.Look.IsWindowHeightAdjustmentEnabled = defaultSettings.Look.IsWindowHeightAdjustmentEnabled;
            MainWindow.Settings.Look.WindowHeightPercent = defaultSettings.Look.WindowHeightPercent;
        }

        private static bool AreBackgroundElementStylesEqual(BackgroundElementStyle current, BackgroundElementStyle defaults)
        {
            if (current == null || defaults == null) return current == defaults;

            return string.Equals(NormalizeColorText(current.Color, "#FEFEFE"), NormalizeColorText(defaults.Color, "#FEFEFE"), StringComparison.OrdinalIgnoreCase)
                && SettingsResetItem.AreClose(current.Opacity, defaults.Opacity);
        }

        private static bool AreCustomBackgroundStylesEqual(CustomBackgroundStyle current, CustomBackgroundStyle defaults)
        {
            if (current == null || defaults == null) return current == defaults;

            return AreBackgroundElementStylesEqual(current.MainPanel, defaults.MainPanel)
                && AreBackgroundElementStylesEqual(current.TopPanel, defaults.TopPanel)
                && AreBackgroundElementStylesEqual(current.BlackboardPanel, defaults.BlackboardPanel)
                && AreBackgroundElementStylesEqual(current.LauncherPanel, defaults.LauncherPanel)
                && AreBackgroundElementStylesEqual(current.TimetablePanel, defaults.TimetablePanel)
                && AreBackgroundElementStylesEqual(current.FunctionMenu, defaults.FunctionMenu)
                && AreBackgroundElementStylesEqual(current.BlackboardTitleBar, defaults.BlackboardTitleBar)
                && AreBackgroundElementStylesEqual(current.LauncherTitleBar, defaults.LauncherTitleBar)
                && AreBackgroundElementStylesEqual(current.TimetableTitleBar, defaults.TimetableTitleBar);
        }

        private void BuildBackgroundStyleCategories()
        {
            BackgroundStyleCategories.Clear();

            CustomBackgroundStyle customStyle = MainWindow.Settings.Look.CustomBackgroundStyle;

            BackgroundStyleCategories.Add(new BackgroundStyleCategoryEditor("主容器",
                new BackgroundStyleItemEditor("主面板", customStyle.MainPanel)));

            BackgroundStyleCategories.Add(new BackgroundStyleCategoryEditor("内容面板",
                new BackgroundStyleItemEditor("顶部看板", customStyle.TopPanel),
                new BackgroundStyleItemEditor("小黑板", customStyle.BlackboardPanel),
                new BackgroundStyleItemEditor("启动台", customStyle.LauncherPanel),
                new BackgroundStyleItemEditor("课程表", customStyle.TimetablePanel),
                new BackgroundStyleItemEditor("功能菜单", customStyle.FunctionMenu)));

            BackgroundStyleCategories.Add(new BackgroundStyleCategoryEditor("标题栏",
                new BackgroundStyleItemEditor("小黑板标题栏", customStyle.BlackboardTitleBar),
                new BackgroundStyleItemEditor("启动台标题栏", customStyle.LauncherTitleBar),
                new BackgroundStyleItemEditor("课程表标题栏", customStyle.TimetableTitleBar)));
        }

        private void UpdateCustomBackgroundEditorVisibility()
        {
            if (CustomBackgroundEditorExpander == null) return;

            int backgroundStyle = MainWindow.Settings.Look.BackgroundStyle;
            if (backgroundStyle != 4)
            {
                CustomBackgroundEditorExpander.IsExpanded = false;
            }
        }

        private void ButtonExpandCustomBackgroundEditor_Click(object sender, RoutedEventArgs e)
        {
            CustomBackgroundEditorExpander.IsExpanded = true;
        }

        private static string NormalizeColorText(string colorText, string fallbackColor)
        {
            string fallback = string.IsNullOrWhiteSpace(fallbackColor) ? "#FEFEFE" : fallbackColor.Trim();
            string normalizedText = colorText?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedText)) return fallback;

            if (!normalizedText.StartsWith("#") && (normalizedText.Length == 3 || normalizedText.Length == 6 || normalizedText.Length == 8))
            {
                normalizedText = "#" + normalizedText;
            }

            try
            {
                object convertedColor = ColorConverter.ConvertFromString(normalizedText);
                if (convertedColor is Color color)
                {
                    return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                }
            }
            catch
            {
            }

            return fallback;
        }

        private static double ClampOpacity(double opacity)
        {
            if (double.IsNaN(opacity) || double.IsInfinity(opacity)) return 60;
            if (opacity < 0) return 0;
            if (opacity > 100) return 100;
            return opacity;
        }

        private void CommitCustomBackgroundColorTextBox(TextBox textBox)
        {
            if (textBox == null) return;

            if (textBox.DataContext is BackgroundStyleItemEditor item)
            {
                item.Style.Color = NormalizeColorText(textBox.Text, item.Style.Color);
                SaveCustomBackgroundStyle();
                return;
            }

            if (textBox.DataContext is BackgroundStyleCategoryEditor category)
            {
                category.BatchColor = NormalizeColorText(textBox.Text, category.BatchColor);
            }
        }

        private void CustomBackgroundColorTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            CommitCustomBackgroundColorTextBox(sender as TextBox);
        }

        private void CustomBackgroundColorTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            CommitCustomBackgroundColorTextBox(sender as TextBox);
            Keyboard.ClearFocus();
            e.Handled = true;
        }

        private void CustomBackgroundItem_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is BackgroundStyleItemEditor item)
            {
                item.Style.Opacity = ClampOpacity(item.Style.Opacity);
            }

            SaveCustomBackgroundStyle();
        }

        private void ButtonApplyBackgroundCategory_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement element) || !(element.DataContext is BackgroundStyleCategoryEditor category)) return;

            category.BatchColor = NormalizeColorText(category.BatchColor, "#FEFEFE");
            category.BatchOpacity = ClampOpacity(category.BatchOpacity);

            foreach (BackgroundStyleItemEditor item in category.Items)
            {
                item.Style.Color = category.BatchColor;
                item.Style.Opacity = category.BatchOpacity;
            }

            SaveCustomBackgroundStyle();
        }

        private void SaveCustomBackgroundStyle()
        {
            if (!isPageReady || mainWindow == null) return;

            MainWindow.SaveSettings();
            mainWindow.ApplyVisualSettings();
            UpdateResetButtons();
        }

        private void CardWindowScaleMultiplier_ResetClicked(object sender, RoutedEventArgs e)
        {
            resetWindowScaleMultiplier.Reset();
        }

        private void CardTheme_ResetClicked(object sender, RoutedEventArgs e)
        {
            resetTheme.Reset();
        }

        private void CardBackgroundStyle_ResetClicked(object sender, RoutedEventArgs e)
        {
            resetBackgroundStyle.Reset();
        }

        private void CardIsComponentTitleTextHidden_ResetClicked(object sender, RoutedEventArgs e)
        {
            resetComponentTitleTextHidden.Reset();
        }

        private void CardLookMode_ResetClicked(object sender, RoutedEventArgs e)
        {
            resetLookMode.Reset();
        }

        private void CardIsLauncherEnabled_ResetClicked(object sender, RoutedEventArgs e)
        {
            resetLauncherEnabled.Reset();
        }

        private void CardWindowHeightAdjustment_ResetClicked(object sender, RoutedEventArgs e)
        {
            resetWindowHeightAdjustment.Reset();
        }

        private void CardWindowVerticalAlignment_ResetClicked(object sender, RoutedEventArgs e)
        {
            resetWindowVerticalAlignment.Reset();
        }

        private void CardTargetMonitor_ResetClicked(object sender, RoutedEventArgs e)
        {
            resetTargetMonitor.Reset();
        }

        private void ToggleSwitchIsWindowChromeDisabled_ResetClicked(object sender, RoutedEventArgs e)
        {
            resetWindowChromeDisabled.Reset();
        }

        public class BackgroundStyleCategoryEditor : INotifyPropertyChanged
        {
            public BackgroundStyleCategoryEditor(string header, params BackgroundStyleItemEditor[] items)
            {
                Header = header;

                foreach (BackgroundStyleItemEditor item in items)
                {
                    Items.Add(item);
                }

                if (Items.Count > 0)
                {
                    BatchColor = Items[0].Style.Color;
                    BatchOpacity = Items[0].Style.Opacity;
                }
            }

            public string Header { get; }
            public ObservableCollection<BackgroundStyleItemEditor> Items { get; } = new ObservableCollection<BackgroundStyleItemEditor>();

            private string _batchColor = "#FEFEFE";
            public string BatchColor
            {
                get => _batchColor;
                set
                {
                    if (_batchColor != value)
                    {
                        _batchColor = value;
                        OnPropertyChanged();
                    }
                }
            }

            private double _batchOpacity = 60;
            public double BatchOpacity
            {
                get => _batchOpacity;
                set
                {
                    if (_batchOpacity != value)
                    {
                        _batchOpacity = value;
                        OnPropertyChanged();
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class BackgroundStyleItemEditor
        {
            public BackgroundStyleItemEditor(string header, BackgroundElementStyle style)
            {
                Header = header;
                Style = style;
            }

            public string Header { get; }
            public BackgroundElementStyle Style { get; }
        }
    }

    public class HexColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string colorText = value as string;

            if (string.IsNullOrWhiteSpace(colorText)) return Brushes.Transparent;

            try
            {
                object convertedColor = ColorConverter.ConvertFromString(colorText.Trim());
                if (convertedColor is Color color)
                {
                    return new SolidColorBrush(Color.FromRgb(color.R, color.G, color.B));
                }
            }
            catch
            {
            }

            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class CustomBackgroundStyleVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int backgroundStyle && backgroundStyle == 4) return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
