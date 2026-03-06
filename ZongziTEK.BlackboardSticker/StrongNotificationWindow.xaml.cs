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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Speech.Synthesis;
using System.Windows.Media.Animation;
using ZongziTEK.BlackboardSticker.Helpers;

namespace ZongziTEK.BlackboardSticker
{
    /// <summary>
    /// StrongNotificationWindow.xaml 的交互逻辑
    /// </summary>
    public partial class StrongNotificationWindow : Window
    {
        public StrongNotificationWindow(string title, string subtitle)
        {
            InitializeComponent();

            TextBlockTitle.Text = title;
            TextBlockSubtitle.Text = subtitle;

            if (subtitle == "") TextBlockSubtitle.Visibility = Visibility.Collapsed;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 设置为 ToolWindow
            var hwnd = new WindowInteropHelper(this).Handle;
            WindowsHelper.SetWindowStyleToolWindow(hwnd);

            // 进入动画
            DoubleAnimation opacityAnimationIn = new()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };
            BeginAnimation(OpacityProperty, opacityAnimationIn);

            ThicknessAnimation viewboxMarginAnimationIn = new()
            {
                From = new Thickness(24),
                To = new Thickness(100),
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };
            ViewboxContent.BeginAnimation(MarginProperty, viewboxMarginAnimationIn);
            await Task.Delay(500);

            await Task.Delay(10000);

            // 退出动画
            DoubleAnimation opacityAnimationOut = new()
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseIn }
            };
            BeginAnimation(OpacityProperty, opacityAnimationOut);

            ThicknessAnimation viewboxMarginAnimationOut = new()
            {
                From = new Thickness(100),
                To = new Thickness(24),
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseIn }
            };
            ViewboxContent.BeginAnimation(MarginProperty, viewboxMarginAnimationOut);
            await Task.Delay(600);
            Close();
        }
    }
}
