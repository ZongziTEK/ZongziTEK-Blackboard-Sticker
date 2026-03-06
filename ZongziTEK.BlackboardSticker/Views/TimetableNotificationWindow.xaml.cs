using iNKORE.UI.WPF.Common;
using iNKORE.UI.WPF.Modern.Controls.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using ZongziTEK.BlackboardSticker.Helpers;
using ZongziTEK.BlackboardSticker.Models;

namespace ZongziTEK.BlackboardSticker.Views
{
    /// <summary>
    /// TimetableNotificationWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TimetableNotificationWindow : Window
    {
        public TimetableNotificationWindow(string title, string subtitle, double time, bool isTextTimeVisible)
        {
            InitializeComponent();

            if (MainWindow.Settings.Look.IsWindowChromeDisabled) // AllowTransparency
            {
                WindowsHelper.SetAllowTransparency(this);
            }
            else // WindowChrome
            {
                WindowsHelper.SetWindowChrome(this);
            }

            Width = SystemParameters.WorkArea.Width;

            _totalTime = TimeSpan.FromSeconds(time);
            _timeLeft = _totalTime;
            _timeToHide = DateTime.Now.TimeOfDay + _timeLeft;

            TextTitle.Text = title;
            TextSubtitle.Text = subtitle;

            _timeTimer.Tick += Timer_Tick;
            _timeTimer.Start();

            GridNotification.Opacity = 0;

            if (!isTextTimeVisible)
            { 
                TextTime.Visibility = Visibility.Hidden;
                _isTimeHidden = true;
            }

            TextTime.Text = (_timeLeft.TotalSeconds - 1).ToString("00");
        }

        private TimeSpan _totalTime;
        private TimeSpan _timeLeft;
        private TimeSpan _timeToHide;
        private bool _isTimeHidden = false;
        private bool _isNotificationMinimized = false;
        private bool _isNotificationHidden = false;

        private HwndSource? _hwndSource;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _hwndSource = (HwndSource)PresentationSource.FromVisual(this);
            _hwndSource.AddHook(WndProc);

            //Timer_Tick(null, null);
            ShowNotification();

            DoubleAnimation barWidthAnimation = new()
            {
                From = BorderNotification.ActualWidth,
                To = 0,
                Duration = _totalTime
            };
            RectangleProgressBar.BeginAnimation(WidthProperty, barWidthAnimation);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            //想要让窗口透明穿透鼠标和触摸等，需要同时设置 WS_EX_LAYERED 和 WS_EX_TRANSPARENT 样式，
            //确保窗口始终有 WS_EX_LAYERED 这个样式，并在开启穿透时设置 WS_EX_TRANSPARENT 样式
            //但是WPF窗口在未设置 AllowsTransparency = true 时，会自动去掉 WS_EX_LAYERED 样式（在 HwndTarget 类中)，
            //如果设置了 AllowsTransparency = true 将使用WPF内置的低性能的透明实现，
            //所以这里通过 Hook 的方式，在不使用WPF内置的透明实现的情况下，强行保证这个样式存在。
            if (msg == (int)0x007C && (long)wParam == (long)-20)
            {
                var styleStruct = (WindowsHelper.StyleStruct)Marshal.PtrToStructure(lParam, typeof(WindowsHelper.StyleStruct));
                styleStruct.styleNew |= (int)WindowsHelper.WS_EX_LAYERED;
                Marshal.StructureToPtr(styleStruct, lParam, false);
                handled = true;
            }
            return IntPtr.Zero;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _timeTimer.Stop();
            _timeTimer.Tick -= Timer_Tick;

            if (_hwndSource != null)
            {
                _hwndSource.RemoveHook(WndProc);
            }
        }

        private DispatcherTimer _timeTimer = new()
        {
            Interval = TimeSpan.FromMilliseconds(10)
        };

        private void Timer_Tick(object sender, EventArgs e)
        {
            _timeLeft = _timeToHide - DateTime.Now.TimeOfDay;
            TextTime.Text = _timeLeft.TotalSeconds.ToString("00");

            if (_timeLeft.TotalSeconds <= 1)
            {
                if (!_isTimeHidden)
                    HideTime();
            }
            if (_timeLeft <= TimeSpan.Zero)
            {
                if (!_isNotificationHidden)
                    HideNotification();
            }
        }

        private void ShowNotification()
        {
            DoubleAnimation opacityAnimation = new()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };

            ThicknessAnimation marginAnimation = new()
            {
                From = new Thickness(0, -Height, 0, 0),
                To = new Thickness(0),
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };

            GridNotification.BeginAnimation(OpacityProperty, opacityAnimation);
            GridNotification.BeginAnimation(MarginProperty, marginAnimation);
        }

        private async void HideNotification()
        {
            _isNotificationHidden = true;

            DoubleAnimation opacityAnimation = new()
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseIn }
            };

            ThicknessAnimation marginAnimation = new()
            {
                From = new Thickness(0),
                To = new Thickness(0, -Height, 0, 0),
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseIn }
            };

            GridNotification.BeginAnimation(OpacityProperty, opacityAnimation);
            GridNotification.BeginAnimation(MarginProperty, marginAnimation);

            await Task.Delay(500);
            Close();
        }

        private void HideTime()
        {
            _isTimeHidden = true;

            DoubleAnimation opacityAnimaion = new()
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(750),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseIn }
            };

            TextTime.BeginAnimation(OpacityProperty, opacityAnimaion);
        }

        private async void MinimizeNotification()
        {
            _isNotificationMinimized = true;

            if (!MainWindow.Settings.TimetableSettings.IsClickToHideNotificationEnabled) // 当禁用点击隐藏通知时，允许在处于小岛模式时穿透鼠标事件
            {
                WindowsHelper.SetWindowExTransparent(this);
            }

            BorderNotification.HorizontalAlignment = HorizontalAlignment.Center;

            DoubleAnimation widthAnimation = new()
            {
                From = BorderNotification.ActualWidth,
                To = 480,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseInOut }
            };

            DoubleAnimation heightAnimation = new()
            {
                From = BorderNotification.ActualHeight,
                To = 48,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseInOut }
            };

            ThicknessAnimation marginAnimation = new()
            {
                From = BorderNotification.Margin,
                To = new Thickness(0, 12, 0, 0),
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseInOut }
            };

            DoubleAnimation notificationOpacityAnimation = new()
            {
                From = 1,
                To = 0.5,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseInOut }
            };

            DoubleAnimation bigOpacityAnimation = new()
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(50)
            };

            DoubleAnimation smallOpacityAnimation = new()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseInOut }
            };

            DoubleAnimation barOpacityHideAnimation = new()
            {
                From = 0.4,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(50)
            };

            CornerRadiusAnimation cornerRadiusAnimation = new()
            {
                From = BorderNotification.CornerRadius,
                To = new CornerRadius(24),
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseInOut }
            };

            GridBig.BeginAnimation(OpacityProperty, bigOpacityAnimation);
            BorderShadowEffect.BeginAnimation(OpacityProperty, bigOpacityAnimation);
            RectangleProgressBar.BeginAnimation(OpacityProperty, barOpacityHideAnimation);
            GridSmall.BeginAnimation(OpacityProperty, smallOpacityAnimation);
            GridSmall.Visibility = Visibility.Visible;
            BorderNotification.BeginAnimation(WidthProperty, widthAnimation);
            BorderNotification.BeginAnimation(HeightProperty, heightAnimation);
            BorderNotification.BeginAnimation(MarginProperty, marginAnimation);
            BorderNotification.BeginAnimation(OpacityProperty, notificationOpacityAnimation);
            BorderNotification.BeginAnimation(ClipHelper.CornerRadiusProperty, cornerRadiusAnimation);

            await Task.Delay(500);

            Height = BorderNotification.ActualHeight + BorderNotification.Margin.Top;
            Width = BorderNotification.ActualWidth;
            Left = (SystemParameters.WorkArea.Width - Width) / 2;

            DoubleAnimation barOpacityAppearAnimation = new()
            {
                From = 0,
                To = 0.4,
                Duration = TimeSpan.FromMilliseconds(100),
                EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut }
            };

            DoubleAnimation barWidthAnimation = new()
            {
                From = BorderNotification.ActualWidth * (_timeLeft.TotalSeconds / _totalTime.TotalSeconds),
                To = 0,
                Duration = _timeLeft
            };
            RectangleProgressBar.BeginAnimation(WidthProperty, barWidthAnimation);
            RectangleProgressBar.BeginAnimation(OpacityProperty, barOpacityAppearAnimation);
        }

        private void BorderNotification_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isNotificationMinimized)
            {
                MinimizeNotification();
            }
            else if (!_isNotificationHidden)
            {
                if (MainWindow.Settings.TimetableSettings.IsClickToHideNotificationEnabled) HideNotification();
            }
        }
    }
}
