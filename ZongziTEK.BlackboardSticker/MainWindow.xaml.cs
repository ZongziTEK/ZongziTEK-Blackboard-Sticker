using AutoUpdaterDotNET;
using iNKORE.UI.WPF.Controls;
using iNKORE.UI.WPF.Modern;
using iNKORE.UI.WPF.Modern.Controls;
using IWshRuntimeLibrary;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Input.StylusPlugIns;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ZongziTEK.BlackboardSticker.Helpers;
using ZongziTEK.BlackboardSticker.Models;
using ZongziTEK.BlackboardSticker.Pages;
using ZongziTEK.BlackboardSticker.Services;
using ZongziTEK.BlackboardSticker.Views;
using Drawing = System.Drawing;
using File = System.IO.File;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace ZongziTEK.BlackboardSticker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DrawingAttributes _drawingAttributes;

        private bool _isSettingsLoaded = false;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = Settings;

            // 小黑板 1
            _drawingAttributes = new DrawingAttributes();
            inkCanvas.DefaultDrawingAttributes = _drawingAttributes;
            _drawingAttributes.Color = Colors.White;
            _drawingAttributes.Width = 1.75;
            _drawingAttributes.Height = 1.75;
            _drawingAttributes.StylusTip = StylusTip.Ellipse;
            _drawingAttributes.FitToCurve = true;
            squarePicker.SelectedColor = inkCanvas.DefaultDrawingAttributes.Color;
            _originalColorPickerMargin = borderColorPicker.Margin;

            // 窗体
            SetWindowVerticalSize();

            /*windowTimer.Tick += windowTimer_Tick; // 强力置底，可能导致界面闪烁，故注释
            windowTimer.Start();*/

            // 加载文件
            LoadSettings();
            LoadStrokes();
            Task.Run(() =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    LoadLauncher();
                });
            });

            SetTheme();
            SetWindowScaleTransform(Settings.Look.WindowScaleMultiplier);

            // 设置透明窗口
            if (Settings.Look.IsWindowChromeDisabled) // AllowTransparency
            {
                WindowsHelper.SetAllowTransparency(this);
            }
            else // WindowChrome
            {
                WindowsHelper.SetWindowChrome(this);
            }

            // 检查更新
            if (Settings.Update.IsUpdateAutomatic) CheckUpdate();

            // 看板
            textBlockTime.Text = DateTime.Now.ToString(("HH':'mm':'ss"));
            _clockTimer = new DispatcherTimer();
            _clockTimer.Tick += ClockTimer_Tick;
            _clockTimer.Interval = new TimeSpan(0, 0, 0, 0, 5);
            _clockTimer.Start();

            LoadFrameInfoPagesList();
            _frameInfoNavigationTimer.Tick += FrameInfoNavigationTimer_Tick;
            _frameInfoNavigationTimer.Interval = TimeSpan.FromSeconds(4);
            _frameInfoNavigationTimer.Start();

            // 课程表
            _timetableTimer = new DispatcherTimer();
            _timetableTimer.Tick += CheckTimetable;
            _timetableTimer.Interval = new TimeSpan(0, 0, 1);
            LoadTimetableOrCurriculum();

            // 颜色主题
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
            SystemEvents_UserPreferenceChanged(null, null);

            // 小黑板 2
            CheckIsBlackboardLocked();

            // 隐藏管家助手
            timerHideSeewoServiceAssistant.Tick += TimerHideSeewoServiceAssistant_Tick;
            timerHideSeewoServiceAssistant.Interval = TimeSpan.FromSeconds(1);

            // 服务
            if (App.ServiceManager != null)
            {
                if (Settings.Interactions.IsClassIslandConnectorEnabled)
                {
                    App.ServiceManager.RegisterService<ClassIslandConnectorService>();
                }
            }
        }

        #region Window
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var hwnd = new WindowInteropHelper(this).Handle;
            WindowsHelper.SetWindowStyleToolWindow(hwnd);
        }

        private void window_StateChanged(object sender, EventArgs e)
        {
            WindowState = WindowState.Normal;

            if (Settings.Automation.IsBottomMost) WindowsHelper.SetBottom(window);
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            SwitchLookMode(0);
            SwitchLookMode(Settings.Look.LookMode);

            if (Settings.Look.IsAnimationEnhanced)
            {
                ThicknessAnimation marginAnimation = new()
                {
                    From = new Thickness(BorderMain.ActualWidth + BorderMain.Margin.Right, 8, -BorderMain.ActualWidth + BorderMain.Margin.Right, 8),
                    To = new Thickness(8),
                    Duration = TimeSpan.FromMilliseconds(1000),
                    EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
                };

                BorderMain.BeginAnimation(MarginProperty, marginAnimation);
            }

            _timetableScrollTimer.Tick += TimetableScrollTimer_Tick;
            _timetableScrollTimer.Start();
            ScrollToCurrentLesson();
        }

        private void window_Activated(object sender, EventArgs e)
        {
            if (Settings.Automation.IsBottomMost) WindowsHelper.SetBottom(window);
        }

        public static bool IsClosingFromButton = false;
        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!IsClosingFromButton)
            {
                e.Cancel = true;
                if (MessageBox.Show("是否继续关闭 ZongziTEK 黑板贴", "ZongziTEK 黑板贴", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    if (MessageBox.Show("真的狠心关闭 ZongziTEK 黑板贴 吗？", "ZongziTEK 黑板贴", MessageBoxButton.OKCancel, MessageBoxImage.Error) == MessageBoxResult.OK)
                    {
                        if (MessageBox.Show("是否取消关闭 ZongziTEK 黑板贴 ？", "ZongziTEK 黑板贴", MessageBoxButton.YesNo, MessageBoxImage.Error) != MessageBoxResult.Yes)
                        {
                            e.Cancel = false;
                            Application.Current.Shutdown();
                        }
                    }
                }
            }
        }

        // 没用
        /*
        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);

            Dispatcher.BeginInvoke(() =>
            {
                if (MessageBox.Show("检测到系统 DPI 变化。为确保黑板贴显示正常，需要重启黑板贴。\r\n是否立即重启黑板贴？", "ZongziTEK 黑板贴", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Restart();
                }
            });
        }
        */

        private void SetWindowVerticalSize()
        {
            Height = SystemParameters.WorkArea.Height;
            Top = 0;
        }

        public void Creep(double marginVertical, bool isTop)
        {
            var previousMargin = GridRoot.Margin;
            var newMargin = isTop ? new Thickness(0, marginVertical, 0, 0) : new Thickness(0, 0, 0, marginVertical);

            ThicknessAnimation marginAnimation = new()
            {
                From = previousMargin,
                To = newMargin,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseInOut }
            };

            GridRoot.BeginAnimation(MarginProperty, marginAnimation);
        }

        private async void iconSwitchLeft_MouseDown(object sender, MouseButtonEventArgs e)
        {
            window.BeginAnimation(LeftProperty, null);

            iconSwitchLeft.Visibility = Visibility.Collapsed;
            iconSwitchRight.Visibility = Visibility.Visible;

            Rect targetWorkArea = GetTargetWorkArea();

            DoubleAnimation leftAnimation = new()
            {
                From = Left,
                To = targetWorkArea.Left,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseInOut }
            };
            BeginAnimation(LeftProperty, leftAnimation);

            await Task.Delay(500);

            window.BeginAnimation(LeftProperty, null);
        }

        private async void iconSwitchRight_MouseDown(object sender, MouseButtonEventArgs e)
        {
            window.BeginAnimation(LeftProperty, null);

            iconSwitchRight.Visibility = Visibility.Collapsed;
            iconSwitchLeft.Visibility = Visibility.Visible;

            Rect targetWorkArea = GetTargetWorkArea();

            DoubleAnimation leftAnimation = new()
            {
                From = Left,
                To = targetWorkArea.Left + (targetWorkArea.Width - ActualWidth),
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseInOut }
            };
            BeginAnimation(LeftProperty, leftAnimation);

            await Task.Delay(500);

            window.BeginAnimation(LeftProperty, null);
        }

        /*private DispatcherTimer windowTimer = new DispatcherTimer() // 强力置底，可能导致界面闪烁
        {
            Interval = new TimeSpan(0, 0, 0, 0, 500)
        };

        private void windowTimer_Tick(object sender, EventArgs e)
        {
            WindowsHelper.SetBottom(window);
        }*/
        #endregion

        #region Blackboard
        private void penButton_Click(object sender, RoutedEventArgs e)
        {
            if (inkCanvas.EditingMode == InkCanvasEditingMode.Ink)
            {
                //if (!confirmingClear)
                //{
                if (borderColorPicker.Visibility == Visibility.Collapsed) ShowColorPicker();
                else HideColorPicker();
                //}
            }
            else
            {
                inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
            }
        }

        private void eraserButton_Click(object sender, RoutedEventArgs e)
        {
            HideColorPicker();
            inkCanvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
        }

        //bool confirmingClear = false;

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            HideColorPicker();

            //borderClearConfirm.Visibility = Visibility.Visible;
            //confirmingClear = true;
            //touchGrid.Visibility = Visibility.Collapsed;
        }

        private void btnClearCancel_Click(object sender, RoutedEventArgs e)
        {
            //borderClearConfirm.Visibility = Visibility.Collapsed;
            //touchGrid.Visibility = Visibility.Visible;
            //confirmingClear = false;
        }

        private void btnClearOK_Click(object sender, RoutedEventArgs e)
        {
            //confirmingClear = false;

            inkCanvas.Strokes.Clear();

            Flyout flyout = FlyoutService.GetFlyout(clearButton) as Flyout;
            if (flyout != null) flyout.Hide();

            string path;

            if (Settings.Storage.IsFilesSavingWithProgram)
            {
                path = System.AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                path = Settings.Storage.DataPath;
            }

            if (!path.EndsWith("\\") || !path.EndsWith("/"))
            {
                path += "\\";
            }

            if (File.Exists(path + "sticker.icstk"))
            {
                File.Delete(path + "sticker.icstk");
            }
            //borderClearConfirm.Visibility = Visibility.Collapsed;
            //touchGrid.Visibility = Visibility.Visible;
        }

        private void BorderColorSelector_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border)
            {
                Border border = (Border)sender;
                squarePicker.SelectedColor = ((SolidColorBrush)border.Background).Color;
            }
        }

        private void squarePicker_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (squarePicker.ColorState.HSL_S == 0 || squarePicker.ColorState.HSL_L == 0)
            {
                TeachingTipColorPicker.IsOpen = true;
            }
        }

        private void squarePicker_ColorChanged(object sender, RoutedEventArgs e)
        {
            borderShowColor.Background = new SolidColorBrush(squarePicker.SelectedColor);
            inkCanvas.DefaultDrawingAttributes.Color = squarePicker.SelectedColor;
        }

        private void btnCloseColorPicker_Click(object sender, RoutedEventArgs e)
        {
            HideColorPicker();
        }

        private void SliderPenThickness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_drawingAttributes != null)
            {
                _drawingAttributes.Width = SliderPenThickness.Value;
                _drawingAttributes.Height = SliderPenThickness.Value;
            }
        }

        private void ToggleButtonLock_Click(object sender, RoutedEventArgs e)
        {
            if (!_isSettingsLoaded) return;

            Settings.Blackboard.IsLocked = ToggleButtonLock.IsChecked.Value;
            CheckIsBlackboardLocked();

            SaveSettings();
        }

        private void CheckIsBlackboardLocked()
        {
            if (Settings.Blackboard.IsLocked)
            {
                //if (confirmingClear)
                //{
                btnClearCancel_Click(null, null);
                //}

                BorderLockBlackboard.Visibility = Visibility.Visible;

                if (GetIsLightTheme()) ToggleButtonLock.Foreground = new SolidColorBrush(Colors.White); else ToggleButtonLock.Foreground = new SolidColorBrush(Colors.Black);

                eraserButton.Visibility = Visibility.Hidden;
                HideColorPicker();

            }
            else
            {
                BorderLockBlackboard.Visibility = Visibility.Collapsed;

                if (GetIsLightTheme()) ToggleButtonLock.Foreground = new SolidColorBrush(Colors.Black); else ToggleButtonLock.Foreground = new SolidColorBrush(Colors.White);

                eraserButton_Click(null, null);
                penButton_Click(null, null);

                eraserButton.Visibility = Visibility.Visible;
            }
        }

        private void BorderLockBlackboard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HighlightLockState();
        }

        private bool _isHighlightingLockState = false;
        private async void HighlightLockState()
        {
            if (!_isHighlightingLockState)
            {
                _isHighlightingLockState = true;

                StackPanelHighlightBlackboardLockState.Visibility = Visibility.Visible;
                if (GetIsLightTheme())
                {
                    ToggleButtonLock.Foreground = new SolidColorBrush(Colors.Black);
                    await Task.Delay(200);
                    ToggleButtonLock.Foreground = new SolidColorBrush(Colors.White);
                    await Task.Delay(200);
                    ToggleButtonLock.Foreground = new SolidColorBrush(Colors.Black);
                    await Task.Delay(200);
                    ToggleButtonLock.Foreground = new SolidColorBrush(Colors.White);
                    await Task.Delay(200);
                    ToggleButtonLock.Foreground = new SolidColorBrush(Colors.Black);
                    await Task.Delay(200);
                    ToggleButtonLock.Foreground = new SolidColorBrush(Colors.White);
                }
                else
                {
                    ToggleButtonLock.Foreground = new SolidColorBrush(Colors.White);
                    await Task.Delay(200);
                    ToggleButtonLock.Foreground = new SolidColorBrush(Colors.Black);
                    await Task.Delay(200);
                    ToggleButtonLock.Foreground = new SolidColorBrush(Colors.White);
                    await Task.Delay(200);
                    ToggleButtonLock.Foreground = new SolidColorBrush(Colors.Black);
                    await Task.Delay(200);
                    ToggleButtonLock.Foreground = new SolidColorBrush(Colors.White);
                    await Task.Delay(200);
                    ToggleButtonLock.Foreground = new SolidColorBrush(Colors.Black);
                }
                StackPanelHighlightBlackboardLockState.Visibility = Visibility.Collapsed;

                _isHighlightingLockState = false;
                CheckIsBlackboardLocked();
            }
        }

        private Thickness _originalColorPickerMargin;

        private void ShowColorPicker()
        {
            ThicknessAnimation marginAnimation = new()
            {
                From = new Thickness(_originalColorPickerMargin.Left, _originalColorPickerMargin.Top, _originalColorPickerMargin.Right, _originalColorPickerMargin.Bottom - 50),
                To = _originalColorPickerMargin,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };
            DoubleAnimation opacityAnimation = new()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };

            borderColorPicker.Visibility = Visibility.Visible;
            borderColorPicker.BeginAnimation(OpacityProperty, opacityAnimation);
            borderColorPicker.BeginAnimation(MarginProperty, marginAnimation);
        }

        private async void HideColorPicker()
        {
            TeachingTipColorPicker.IsOpen = false;

            ThicknessAnimation marginAnimation = new()
            {
                From = _originalColorPickerMargin,
                To = new Thickness(_originalColorPickerMargin.Left, _originalColorPickerMargin.Top, _originalColorPickerMargin.Right, _originalColorPickerMargin.Bottom - 50),
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseIn }
            };
            DoubleAnimation opacityAnimation = new()
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseIn }
            };

            borderColorPicker.BeginAnimation(OpacityProperty, opacityAnimation);
            borderColorPicker.BeginAnimation(MarginProperty, marginAnimation);

            await Task.Delay(250);

            borderColorPicker.Visibility = Visibility.Collapsed;
        }

        private void SaveStrokes()
        {
            FileStream fileStream = new FileStream(GetDataPath() + "sticker.icstk", FileMode.Create);
            inkCanvas.Strokes.Save(fileStream);
            fileStream.Close();
        }

        private void LoadStrokes()
        {
            if (File.Exists(GetDataPath() + "sticker.icstk"))
            {
                FileStream fileStream = new FileStream(GetDataPath() + "sticker.icstk", FileMode.Open);
                inkCanvas.Strokes = new StrokeCollection(fileStream);
                fileStream.Close();
            }
        }

        /*private void inkCanvas_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                foreach (Stroke stroke in inkCanvas.Strokes)
                {
                    Matrix matrix = new Matrix();
                    matrix.ScaleAt(0.95, 0.95, Mouse.GetPosition(inkCanvas).X, Mouse.GetPosition(inkCanvas).Y);
                    matrix.Translate(1.2, 0);
                    stroke.Transform(matrix, false);
                }
            }
            else
            {
                foreach (Stroke stroke in inkCanvas.Strokes)
                {
                    Matrix matrix = new Matrix();
                    matrix.ScaleAt(1/0.95, 1/0.95, Mouse.GetPosition(inkCanvas).X, Mouse.GetPosition(inkCanvas).Y);
                    matrix.Translate(0, 1.2);
                    stroke.Transform(matrix, false);
                }
            }
            SaveStrokes();
        }*/

        private void inkCanvas_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            HideColorPicker();
        }

        private List<int> _touchDeviceIds = new List<int>(); //记录触摸设备ID
        private Point _centerPoint; //中心点

        private void touchGrid_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = inkCanvas;
            e.Mode = ManipulationModes.All;
        }

        private void touchGrid_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (_touchDeviceIds.Count >= 2)
            {
                ManipulationDelta md = e.DeltaManipulation;
                Vector trans = md.Translation;  // 获得位移矢量
                double rotate = md.Rotation;  // 获得旋转角度
                Vector scale = md.Scale;  // 获得缩放倍数

                Matrix m = new Matrix();

                // Find center of element and then transform to get current location of center
                FrameworkElement fe = e.Source as FrameworkElement;
                Point center = new Point(fe.ActualWidth / 2, fe.ActualHeight / 2);
                center = m.Transform(center);  // 转换为矩阵缩放和旋转的中心点

                // Update matrix to reflect translation/rotation
                m.Translate(trans.X, trans.Y);  // 移动
                m.ScaleAt(scale.X, scale.Y, center.X, center.Y);  // 缩放

                foreach (Stroke stroke in inkCanvas.Strokes)
                {
                    stroke.Transform(m, false);

                    try
                    {
                        stroke.DrawingAttributes.Width *= md.Scale.X;
                        stroke.DrawingAttributes.Height *= md.Scale.Y;
                    }
                    catch { }
                }
            }
        }

        private InkCanvasEditingMode _lastEditingMode = new InkCanvasEditingMode();

        private void inkCanvas_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            HideColorPicker();

            _touchDeviceIds.Add(e.TouchDevice.Id);
            //设备1个的时候，记录中心点
            if (_touchDeviceIds.Count == 1)
            {
                TouchPoint touchPoint = e.GetTouchPoint(inkCanvas);
                _centerPoint = touchPoint.Position;
            }
            //设备两个及两个以上，将画笔功能关闭
            if (_touchDeviceIds.Count > 1)
            {
                if (inkCanvas.EditingMode != InkCanvasEditingMode.None)
                {
                    _lastEditingMode = inkCanvas.EditingMode;
                }
                if (inkCanvas.EditingMode != InkCanvasEditingMode.None)
                {
                    inkCanvas.EditingMode = InkCanvasEditingMode.None;
                }
            }
        }

        private void inkCanvas_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            //手势完成后切回之前的状态
            if (_touchDeviceIds.Count > 1)
            {
                if (inkCanvas.EditingMode == InkCanvasEditingMode.None)
                {
                    inkCanvas.EditingMode = _lastEditingMode;
                    SaveStrokes();
                }
            }
            _touchDeviceIds.Remove(e.TouchDevice.Id);
        }


        private void inkCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            // 检查是否是压感笔书写
            foreach (StylusPoint stylusPoint in e.Stroke.StylusPoints)
            {
                if (stylusPoint.PressureFactor != 0.5 && stylusPoint.PressureFactor != 0)
                {
                    return;
                }
            }

            try
            {
                StylusPointCollection stylusPoints = new StylusPointCollection();
                int n = e.Stroke.StylusPoints.Count - 1;
                double pressure = 0.1;
                int x = 10;
                if (n == 1) return;
                if (n >= x)
                {
                    for (int i = 0; i < n - x; i++)
                    {
                        StylusPoint point = new StylusPoint();

                        point.PressureFactor = (float)0.5;
                        point.X = e.Stroke.StylusPoints[i].X;
                        point.Y = e.Stroke.StylusPoints[i].Y;
                        stylusPoints.Add(point);
                    }
                    for (int i = n - x; i <= n; i++)
                    {
                        StylusPoint point = new StylusPoint();

                        point.PressureFactor = (float)((0.5 - pressure) * (n - i) / x + pressure);
                        point.X = e.Stroke.StylusPoints[i].X;
                        point.Y = e.Stroke.StylusPoints[i].Y;
                        stylusPoints.Add(point);
                    }
                }
                else
                {
                    for (int i = 0; i <= n; i++)
                    {
                        StylusPoint point = new StylusPoint();

                        point.PressureFactor = (float)(0.4 * (n - i) / n + pressure);
                        point.X = e.Stroke.StylusPoints[i].X;
                        point.Y = e.Stroke.StylusPoints[i].Y;
                        stylusPoints.Add(point);
                    }
                }
                e.Stroke.StylusPoints = stylusPoints;
            }
            catch
            {

            }

            SaveStrokes();
        }

        private void inkCanvas_StrokeErased(object sender, RoutedEventArgs e)
        {
            SaveStrokes();
        }

        #region Brush effect from WXRIW
        // A StylusPlugin that renders ink with a linear gradient brush effect.
        class CustomDynamicRenderer : DynamicRenderer
        {
            [ThreadStatic]
            static private Brush brush = null;

            [ThreadStatic]
            static private Pen pen = null;

            private Point prevPoint;

            protected override void OnStylusDown(RawStylusInput rawStylusInput)
            {
                // Allocate memory to store the previous point to draw from.
                prevPoint = new Point(double.NegativeInfinity, double.NegativeInfinity);
                base.OnStylusDown(rawStylusInput);
            }
            //protected override void OnDraw(System.Windows.Media.DrawingContext drawingContext, System.Windows.Input.StylusPointCollection stylusPoints, System.Windows.Media.Geometry geometry, System.Windows.Media.Brush fillBrush)
            //{


            //    ImageSource img = new BitmapImage(new Uri("pack://application:,,,/Resources/maobi.png"));

            //    //前一个点的绘制。
            //    Point prevPoint = new Point(double.NegativeInfinity,
            //                                double.NegativeInfinity);


            //    var w = Global.StrokeWidth + 15;    //输出时笔刷的实际大小


            //    Point pt = new Point(0, 0);
            //    Vector v = new Vector();            //前一个点与当前点的距离
            //    var subtractY = 0d;                 //当前点处前一点的Y偏移
            //    var subtractX = 0d;                 //当前点处前一点的X偏移
            //    var pointWidth = Global.StrokeWidth;
            //    double x = 0, y = 0;
            //    for (int i = 0; i < stylusPoints.Count; i++)
            //    {
            //        pt = (Point)stylusPoints[i];
            //        v = Point.Subtract(prevPoint, pt);

            //        Debug.WriteLine("X " + pt.X + "\t" + pt.Y);

            //        subtractY = (pt.Y - prevPoint.Y) / v.Length;    //设置stylusPoints两个点之间需要填充的XY偏移
            //        subtractX = (pt.X - prevPoint.X) / v.Length;

            //        if (w - v.Length < Global.StrokeWidth)          //控制笔刷大小
            //        {
            //            pointWidth = Global.StrokeWidth;
            //        }
            //        else
            //        {
            //            pointWidth = w - v.Length;                  //在两个点距离越大的时候，笔刷所展示的大小越小
            //        }


            //        for (double j = 0; j < v.Length; j = j + 1d)    //填充stylusPoints两个点之间
            //        {
            //            x = 0; y = 0;

            //            if (prevPoint.X == double.NegativeInfinity || prevPoint.Y == double.NegativeInfinity || double.PositiveInfinity == prevPoint.X || double.PositiveInfinity == prevPoint.Y)
            //            {
            //                y = pt.Y;
            //                x = pt.X;
            //            }
            //            else
            //            {
            //                y = prevPoint.Y + subtractY;
            //                x = prevPoint.X + subtractX;
            //            }

            //            drawingContext.DrawImage(img, new Rect(x - pointWidth / 2, y - pointWidth / 2, pointWidth, pointWidth));    //在当前点画笔刷图片
            //            prevPoint = new Point(x, y);


            //            if (double.IsNegativeInfinity(v.Length) || double.IsPositiveInfinity(v.Length))
            //            { break; }
            //        }
            //    }
            //    stylusPoints = null;
            //}
            protected override void OnDraw(DrawingContext drawingContext,
                                           StylusPointCollection stylusPoints,
                                           Geometry geometry, Brush fillBrush)
            {
                // Create a new Brush, if necessary.
                //brush ??= new LinearGradientBrush(Colors.Red, Colors.Blue, 20d);

                // Create a new Pen, if necessary.
                //pen ??= new Pen(brush, 2d);

                // Draw linear gradient ellipses between 
                // all the StylusPoints that have come in.
                for (int i = 0; i < stylusPoints.Count; i++)
                {
                    Point pt = (Point)stylusPoints[i];
                    Vector v = Point.Subtract(prevPoint, pt);

                    // Only draw if we are at least 4 units away 
                    // from the end of the last ellipse. Otherwise, 
                    // we're just redrawing and wasting cycles.
                    if (v.Length > 4)
                    {
                        // Set the thickness of the stroke based 
                        // on how hard the user pressed.
                        double radius = stylusPoints[i].PressureFactor * 10d;
                        drawingContext.DrawEllipse(brush, pen, pt, radius, radius);
                        prevPoint = pt;
                    }
                }
            }
        }
        public class Global
        {
            public static double StrokeWidth = 2.5;
        }
        public class CustomRenderingInkCanvas : InkCanvas
        {
            CustomDynamicRenderer customRenderer = new CustomDynamicRenderer();

            public CustomRenderingInkCanvas() : base()
            {
                // Use the custom dynamic renderer on the
                // custom InkCanvas.
                this.DynamicRenderer = customRenderer;
            }

            protected override void OnStrokeCollected(InkCanvasStrokeCollectedEventArgs e)
            {
                // Remove the original stroke and add a custom stroke.
                this.Strokes.Remove(e.Stroke);
                CustomStroke customStroke = new CustomStroke(e.Stroke.StylusPoints);
                this.Strokes.Add(customStroke);

                // Pass the custom stroke to base class' OnStrokeCollected method.
                InkCanvasStrokeCollectedEventArgs args =
                    new InkCanvasStrokeCollectedEventArgs(customStroke);
                base.OnStrokeCollected(args);
            }
        }// A class for rendering custom strokes
        class CustomStroke : Stroke
        {
            Brush brush;
            Pen pen;

            public CustomStroke(StylusPointCollection stylusPoints)
                : base(stylusPoints)
            {
                // Create the Brush and Pen used for drawing.
                brush = new LinearGradientBrush(Colors.Red, Colors.Blue, 20d);
                pen = new Pen(brush, 2d);
            }
            //protected override void DrawCore(DrawingContext drawingContext, DrawingAttributes drawingAttributes)
            //{


            //            ImageSource img = new BitmapImage(new Uri("pack://application:,,,/Resources/maobi.png"));

            //    //前一个点的绘制。
            //    Point prevPoint = new Point(double.NegativeInfinity,
            //                                double.NegativeInfinity);


            //    var w = Global.StrokeWidth + 15;    //输出时笔刷的实际大小


            //    Point pt = new Point(0, 0);
            //    Vector v = new Vector();            //前一个点与当前点的距离
            //    var subtractY = 0d;                 //当前点处前一点的Y偏移
            //    var subtractX = 0d;                 //当前点处前一点的X偏移
            //    var pointWidth = Global.StrokeWidth;
            //    double x = 0, y = 0;
            //    for (int i = 0; i < stylusPoints.Count; i++)
            //    {
            //        pt = (Point)stylusPoints[i];
            //        v = Point.Subtract(prevPoint, pt);

            //        Debug.WriteLine("X " + pt.X + "\t" + pt.Y);

            //        subtractY = (pt.Y - prevPoint.Y) / v.Length;    //设置stylusPoints两个点之间需要填充的XY偏移
            //        subtractX = (pt.X - prevPoint.X) / v.Length;

            //        if (w - v.Length < Global.StrokeWidth)          //控制笔刷大小
            //        {
            //            pointWidth = Global.StrokeWidth;
            //        }
            //        else
            //        {
            //            pointWidth = w - v.Length;                  //在两个点距离越大的时候，笔刷所展示的大小越小
            //        }


            //        for (double j = 0; j < v.Length; j = j + 1d)    //填充stylusPoints两个点之间
            //        {
            //            x = 0; y = 0;

            //            if (prevPoint.X == double.NegativeInfinity || prevPoint.Y == double.NegativeInfinity || double.PositiveInfinity == prevPoint.X || double.PositiveInfinity == prevPoint.Y)
            //            {
            //                y = pt.Y;
            //                x = pt.X;
            //            }
            //            else
            //            {
            //                y = prevPoint.Y + subtractY;
            //                x = prevPoint.X + subtractX;
            //            }

            //            drawingContext.DrawImage(img, new Rect(x - pointWidth / 2, y - pointWidth / 2, pointWidth, pointWidth));    //在当前点画笔刷图片
            //            prevPoint = new Point(x, y);


            //            if (double.IsNegativeInfinity(v.Length) || double.IsPositiveInfinity(v.Length))
            //            { break; }
            //        }
            //    }
            //    stylusPoints = null;
            //}
            protected override void DrawCore(DrawingContext drawingContext,
                                             DrawingAttributes drawingAttributes)
            {
                // Allocate memory to store the previous point to draw from.
                Point prevPoint = new Point(double.NegativeInfinity,
                                            double.NegativeInfinity);

                // Draw linear gradient ellipses between
                // all the StylusPoints in the Stroke.
                for (int i = 0; i < this.StylusPoints.Count; i++)
                {
                    Point pt = (Point)this.StylusPoints[i];
                    Vector v = Point.Subtract(prevPoint, pt);

                    // Only draw if we are at least 4 units away
                    // from the end of the last ellipse. Otherwise,
                    // we're just redrawing and wasting cycles.
                    if (v.Length > 4)
                    {
                        // Set the thickness of the stroke
                        // based on how hard the user pressed.
                        double radius = this.StylusPoints[i].PressureFactor * 10d;
                        drawingContext.DrawEllipse(brush, pen, pt, radius, radius);
                        prevPoint = pt;
                    }
                }
            }
        }
        #endregion

        #endregion

        #region Timetable & Curriculum

        public static int TimetableToShowIndex = (int)DateTime.Today.DayOfWeek;

        public void LoadTimetableOrCurriculum()
        {
            var classIslandConnectorService = App.ServiceManager.GetService<ClassIslandConnectorService>();

            if (Settings.TimetableSettings.IsTimetableEnabled || (classIslandConnectorService != null && classIslandConnectorService.IsTimetableSyncEnabled))
            {
                LoadTimetable();

                textBlockCurriculum.Visibility = Visibility.Collapsed;
                StackPanelShowTimetable.Visibility = Visibility.Visible;
                MenuItemTimetableAutoScroll.IsChecked = true;

                _timetableTimer.Start();
            }
            else
            {
                LoadCurriculum();

                textBlockCurriculum.Visibility = Visibility.Visible;
                StackPanelShowTimetable.Visibility = Visibility.Collapsed;
                MenuItemTimetableAutoScroll.IsChecked = false;

                _timetableTimer.Stop();
            }

            CheckTimetableMenuItems();
        }

        private void MenuItemShowMondayTimetable_Click(object sender, RoutedEventArgs e)
        {
            TimetableToShowIndex = 1;
            LoadTimetableOrCurriculum();
        }

        private void MenuItemShowTuesdayTimetable_Click(object sender, RoutedEventArgs e)
        {
            TimetableToShowIndex = 2;
            LoadTimetableOrCurriculum();
        }

        private void MenuItemShowWednesdayTimetable_Click(object sender, RoutedEventArgs e)
        {
            TimetableToShowIndex = 3;
            LoadTimetableOrCurriculum();
        }

        private void MenuItemShowThursdayTimetable_Click(object sender, RoutedEventArgs e)
        {
            TimetableToShowIndex = 4;
            LoadTimetableOrCurriculum();
        }

        private void MenuItemShowFridayTimetable_Click(object sender, RoutedEventArgs e)
        {
            TimetableToShowIndex = 5;
            LoadTimetableOrCurriculum();
        }

        private void MenuItemShowSaturdayTimetable_Click(object sender, RoutedEventArgs e)
        {
            TimetableToShowIndex = 6;
            LoadTimetableOrCurriculum();
        }

        private void MenuItemShowSundayTimetable_Click(object sender, RoutedEventArgs e)
        {
            TimetableToShowIndex = 0;
            LoadTimetableOrCurriculum();
        }

        private void MenuItemShowTempTimetable_Click(object sender, RoutedEventArgs e)
        {
            TimetableToShowIndex = 7;
            LoadTimetableOrCurriculum();
        }

        private void CheckTimetableMenuItems() // 通过菜单项的 IsChecked 状态来告知用户正在展示哪个课表
        {
            foreach (MenuItem menuItem in MenuChooseTimetableToShow.Items)
            {
                menuItem.IsChecked = false;
            }

            switch (TimetableToShowIndex)
            {
                case 1: // 周一
                    MenuItemShowMondayTimetable.IsChecked = true;
                    break;
                case 2: // 周二
                    MenuItemShowTuesdayTimetable.IsChecked = true;
                    break;
                case 3: // 周三
                    MenuItemShowWednesdayTimetable.IsChecked = true;
                    break;
                case 4: // 周四
                    MenuItemShowThursdayTimetable.IsChecked = true;
                    break;
                case 5: // 周五
                    MenuItemShowFridayTimetable.IsChecked = true;
                    break;
                case 6: // 周六
                    MenuItemShowSaturdayTimetable.IsChecked = true;
                    break;
                case 0: // 周日
                    MenuItemShowSundayTimetable.IsChecked = true;
                    break;
                case 7: // 临时
                    MenuItemShowTempTimetable.IsChecked = true;
                    break;
            }
        }

        #region Curriculum
        public static Curriculums Curriculums = new Curriculums();
        public const string CurriculumsFileName = "Curriculums.json";

        private void SaveCurriculum()
        {
            Curriculums.Monday.Curriculums = textBoxMonday.Text;
            Curriculums.Tuesday.Curriculums = textBoxTuesday.Text;
            Curriculums.Wednesday.Curriculums = textBoxWednesday.Text;
            Curriculums.Thursday.Curriculums = textBoxThursday.Text;
            Curriculums.Friday.Curriculums = textBoxFriday.Text;
            Curriculums.Saturday.Curriculums = textBoxSaturday.Text;
            Curriculums.Sunday.Curriculums = textBoxSunday.Text;
            Curriculums.Temp.Curriculums = textBoxTempCurriculums.Text;

            string text = JsonConvert.SerializeObject(Curriculums, Formatting.Indented);

            try
            {
                File.WriteAllText(GetDataPath() + CurriculumsFileName, text);
            }
            catch { }
        }

        private void LoadCurriculum()
        {
            if (File.Exists(GetDataPath() + CurriculumsFileName))
            {
                try
                {
                    string text = File.ReadAllText(GetDataPath() + CurriculumsFileName);
                    Curriculums = JsonConvert.DeserializeObject<Curriculums>(text);
                }
                catch { }
            }

            textBoxMonday.Text = Curriculums.Monday.Curriculums;
            textBoxTuesday.Text = Curriculums.Tuesday.Curriculums;
            textBoxWednesday.Text = Curriculums.Wednesday.Curriculums;
            textBoxThursday.Text = Curriculums.Thursday.Curriculums;
            textBoxFriday.Text = Curriculums.Friday.Curriculums;
            textBoxSaturday.Text = Curriculums.Saturday.Curriculums;
            textBoxSunday.Text = Curriculums.Sunday.Curriculums;
            textBoxTempCurriculums.Text = Curriculums.Temp.Curriculums;

            textBlockCurriculum.FontSize = Settings.TimetableSettings.FontSize;

            switch (TimetableToShowIndex)
            {
                case 1: // 周一
                    textBlockCurriculum.Text = Curriculums.Monday.Curriculums;
                    break;
                case 2: // 周二
                    textBlockCurriculum.Text = Curriculums.Tuesday.Curriculums;
                    break;
                case 3: // 周三
                    textBlockCurriculum.Text = Curriculums.Wednesday.Curriculums;
                    break;
                case 4: // 周四
                    textBlockCurriculum.Text = Curriculums.Thursday.Curriculums;
                    break;
                case 5: // 周五
                    textBlockCurriculum.Text = Curriculums.Friday.Curriculums;
                    break;
                case 6: // 周六
                    textBlockCurriculum.Text = Curriculums.Saturday.Curriculums;
                    break;
                case 0: // 周日
                    textBlockCurriculum.Text = Curriculums.Sunday.Curriculums;
                    break;
                case 7: // 临时
                    textBlockCurriculum.Text = Curriculums.Temp.Curriculums;
                    break;
            }

            if (textBlockCurriculum.Text.Trim() == string.Empty)
            {
                LabelNoClass.Visibility = Visibility.Visible;
            }
            else
            {
                LabelNoClass.Visibility = Visibility.Collapsed;
            }
        }

        private bool _isTimetableEditorOpen = false;

        private void editCurriculumButton_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.TimetableSettings.IsTimetableEnabled)
            {
                if (!_isTimetableEditorOpen)
                {
                    TimetableEditor timetableEditor = new();
                    timetableEditor.Closed += TimetableEditor_Closed;
                    _isTimetableEditorOpen = true;
                    timetableEditor.Show();
                }
            }
            else
            {
                ScrollViewerShowCurriculum.Visibility = Visibility.Collapsed;
                editCurriculumButton.Visibility = Visibility.Collapsed;

                saveCurriculumButton.Visibility = Visibility.Visible;
                ScrollViewerCurriculum.Visibility = Visibility.Visible;
            }
        }

        private void TimetableEditor_Closed(object sender, EventArgs e)
        {
            LoadTimetableOrCurriculum();
            _isTimetableEditorOpen = false;
        }

        private void saveCurriculumButton_Click(object sender, RoutedEventArgs e)
        {
            SaveCurriculum();

            ScrollViewerShowCurriculum.Visibility = Visibility.Visible;
            editCurriculumButton.Visibility = Visibility.Visible;

            saveCurriculumButton.Visibility = Visibility.Collapsed;
            ScrollViewerCurriculum.Visibility = Visibility.Collapsed;

            LoadTimetableOrCurriculum();
        }
        #endregion

        #region Timetable
        public static Timetable Timetable = new Timetable();
        public const string TimetableFileName = "Timetable.json";
        private DispatcherTimer _timetableTimer;
        private List<Lesson> _timetableCurrent = new();

        private void LoadTimetable()
        {
            var classIslandConnectorService = App.ServiceManager.GetService<ClassIslandConnectorService>();

            if (classIslandConnectorService == null || classIslandConnectorService.IsConnected == false || !classIslandConnectorService.IsTimetableSyncEnabled) // 加载黑板贴本地课表
            {
                if (File.Exists(GetDataPath() + TimetableFileName))
                {
                    try
                    {
                        string text = File.ReadAllText(GetDataPath() + TimetableFileName);
                        Timetable = JsonConvert.DeserializeObject<Timetable>(text);
                    }
                    catch (Exception ex)
                    {
                        ConsoleHelper.WriteLog("加载本地课程表时遇到错误", "error");
                        Console.WriteLine("--- 错误信息 ---");
                        Console.WriteLine(ex);
                        Console.WriteLine("--- 错误信息末尾 ---");
                    }
                }

                string day = DateTime.Today.DayOfWeek.ToString();

                switch (TimetableToShowIndex)
                {
                    case 1: // 周一
                        _timetableCurrent = Timetable.Monday;
                        break;
                    case 2: // 周二
                        _timetableCurrent = Timetable.Tuesday;
                        break;
                    case 3: // 周三
                        _timetableCurrent = Timetable.Wednesday;
                        break;
                    case 4: // 周四
                        _timetableCurrent = Timetable.Thursday;
                        break;
                    case 5: // 周五
                        _timetableCurrent = Timetable.Friday;
                        break;
                    case 6: // 周六
                        _timetableCurrent = Timetable.Saturday;
                        break;
                    case 0: // 周日
                        _timetableCurrent = Timetable.Sunday;
                        break;
                    case 7: // 临时
                        _timetableCurrent = Timetable.Temp;
                        break;
                }

                MenuChooseTimetableToShow.IsEnabled = true;
            }
            else // 加载共享课表，来自 ClassIsland 连接器
            {
                _timetableCurrent = classIslandConnectorService.TimetableShared;
                MenuChooseTimetableToShow.IsEnabled = false;
            }

            PresentTimetable(_timetableCurrent);

            _lessonIndex = -1;

            double scale = (0.5 / 16) * Settings.TimetableSettings.FontSize + (1 - (0.5 / 16) * 24);
            if (scale < 0.5) scale = 0.5;
            else if (scale > 1.6) scale = 1.6;
            ScaleTimetable.ScaleX = scale;
            ScaleTimetable.ScaleY = scale;
        }

        private void PresentTimetable(List<Lesson> timetable)
        {
            StackPanelShowTimetable.Children.Clear();
            if (timetable == null || timetable.Count == 0)
            {
                LabelNoClass.Visibility = Visibility.Visible;
            }
            else
            {
                LabelNoClass.Visibility = Visibility.Collapsed;

                foreach (Lesson lesson in timetable)
                {
                    TimetableLesson timetableLesson = new()
                    {
                        Subject = lesson.Subject,
                        Time = lesson.StartTime.ToString(@"hh\:mm")
                    };
                    if (lesson.IsSplitBelow)
                    {
                        timetableLesson.Margin = new Thickness(0, 0, 0, 8);
                    }

                    StackPanelShowTimetable.Children.Add(timetableLesson);
                }
            }
        }

        private int _lessonIndex = -1; // 第几节课
        private bool _isInClass = false; // 是否是上课时段
        private int _lastLessonIndex = -1;
        private DayOfWeek _lastDay = DateTime.Today.DayOfWeek;

        private void CheckTimetable(object sender, EventArgs e)
        {
            _timetableTimer.Stop();

            if (_lastDay != DateTime.Now.DayOfWeek)
            {
                _lastDay = DateTime.Now.DayOfWeek;

                TimetableToShowIndex = (int)DateTime.Now.DayOfWeek;
                LoadTimetable();
            }

            TimeSpan currentTime = new TimeSpan
                (DateTime.Now.TimeOfDay.Hours, DateTime.Now.TimeOfDay.Minutes, DateTime.Now.TimeOfDay.Seconds) + TimeSpan.FromSeconds(Settings.TimetableSettings.TimeOffset);

            if (_timetableCurrent != null && _timetableCurrent.Count != 0)
            {
                // 获取上课状态 lessonIndex 和 isInClass
                foreach (var lesson in _timetableCurrent)
                {
                    if (currentTime >= lesson.StartTime) // 在这节课开始后
                    {
                        if (_timetableCurrent.IndexOf(lesson) + 1 < _timetableCurrent.Count) // 不是最后一节课
                        {
                            if (currentTime < _timetableCurrent[_timetableCurrent.IndexOf(lesson) + 1].StartTime) // 在下一节课上课前
                            {
                                _lessonIndex = _timetableCurrent.IndexOf(lesson);
                                _isInClass = currentTime < lesson.EndTime;
                                break;
                            }
                        }
                        else // 是最后一节课
                        {
                            _lessonIndex = _timetableCurrent.Count - 1;
                            _isInClass = currentTime < lesson.EndTime;
                            break;
                        }
                    }
                    else if (_timetableCurrent.IndexOf(lesson) == 0) // 在第一节课开始前
                    {
                        _lessonIndex = -1;
                        _isInClass = false;
                        break;
                    }
                }

                // 上下课通知和语音
                if (_lessonIndex != -1 && currentTime == _timetableCurrent[_lessonIndex].EndTime) // 下课时
                {
                    if (_lessonIndex + 1 < _timetableCurrent.Count) // 不是最后一节课
                    {
                        ShowClassOverNotification(_timetableCurrent, _lessonIndex);
                    }
                    else ShowLastClassOverNotification(_timetableCurrent[_lessonIndex].IsStrongClassOverNotificationEnabled);
                }
                if (_lessonIndex + 1 < _timetableCurrent.Count && !_isInClass && currentTime == _timetableCurrent[_lessonIndex + 1].StartTime - TimeSpan.FromSeconds(Settings.TimetableSettings.BeginNotificationTime)) // 有下一节课，在下一节课开始的数秒前
                {
                    ShowClassBeginPreNotification(_timetableCurrent, _lessonIndex);
                }

                // 在界面中高亮当前课程或下一节课
                int lessonToHighlightIndex = -1; // lessonToHighlightIndex 为 -1 时，不高亮任何课程

                if (_isInClass) // 上课时，高亮当前课程
                {
                    lessonToHighlightIndex = _lessonIndex;
                }
                else if (_lessonIndex + 1 < _timetableCurrent.Count) // 在第一节课前或课间，高亮下一节课
                {
                    lessonToHighlightIndex = _lessonIndex + 1;
                }
                else // 最后一节课下课后，不高亮任何课程
                {
                    lessonToHighlightIndex = -1;
                }

                if (!(StackPanelShowTimetable.Children[0] is TextBlock))
                {
                    foreach (TimetableLesson timetableLesson in StackPanelShowTimetable.Children)
                    {
                        if (StackPanelShowTimetable.Children.IndexOf(timetableLesson) == lessonToHighlightIndex) // 高亮要高亮的课程，在课程开始后显示距离其结束的时间
                        {
                            timetableLesson.Activate();
                            TimeSpan timeLeft = _timetableCurrent[lessonToHighlightIndex].EndTime - currentTime;
                            if (_isInClass)
                            {
                                if (timeLeft.Hours == 0)
                                {
                                    timetableLesson.Time = timeLeft.ToString(@"mm\:ss");
                                }
                                else
                                {
                                    timetableLesson.Time = timeLeft.ToString(@"hh\:mm\:ss");
                                }
                            }
                        }
                        else // 取消高亮不要高亮的课程，并恢复时间显示
                        {
                            timetableLesson.Deactivate();
                            timetableLesson.Time = _timetableCurrent[StackPanelShowTimetable.Children.IndexOf(timetableLesson)].StartTime.ToString(@"hh\:mm");
                        }
                    }
                }

                // 自动滚动课程表
                if (_lessonIndex != _lastLessonIndex) ScrollToCurrentLesson();
                _lastLessonIndex = _lessonIndex;
            }

            _timetableTimer.Start();
        }

        private void ShowClassBeginPreNotification(List<Lesson> today, int index)
        {
            int nextLessonIndex = index + 1;

            if (Settings.TimetableSettings.IsTimetableNotificationEnabled)
            {
                string startTimeString = today[nextLessonIndex].StartTime.ToString(@"hh\:mm");
                string endTimeString = today[nextLessonIndex].EndTime.ToString(@"hh\:mm");

                string title = today[nextLessonIndex].Subject + "课 即将开始";
                string subtitle = "此课程将从 " + startTimeString + " 开始，到 " + endTimeString + " 结束";

                new TimetableNotificationWindow(title, subtitle, Settings.TimetableSettings.BeginNotificationTime, true).Show();
            }

            if (Settings.TimetableSettings.IsBeginSpeechEnabled)
            {
                int timeLeft = Convert.ToInt32(Settings.TimetableSettings.BeginNotificationTime / 60);
                if (timeLeft > 0)
                {
                    TTSHelper.PlayText("距上课还有" + timeLeft.ToString() + "分钟。" + "准备上课，" + today[nextLessonIndex].Subject + "课即将开始");
                }
                else
                {
                    TTSHelper.PlayText("准备上课，" + today[nextLessonIndex].Subject + "课即将开始");
                }
            }
        }

        private void ShowClassOverNotification(List<Lesson> today, int index)
        {
            int nextLessonIndex = index + 1;

            if (Settings.TimetableSettings.IsTimetableNotificationEnabled)
            {
                string startTimeString = today[nextLessonIndex].StartTime.ToString(@"hh\:mm");

                string title = "下一节 " + today[nextLessonIndex].Subject + "课";
                string subtitle = "课堂结束，下一节课将于 " + startTimeString + " 开始";

                if (!today[index].IsStrongClassOverNotificationEnabled)
                {
                    new TimetableNotificationWindow(title, subtitle, Settings.TimetableSettings.OverNotificationTime, false).Show();
                }
                else
                {
                    title = "下一节是 " + today[nextLessonIndex].Subject + "课";
                    new StrongNotificationWindow(title, subtitle).Show();
                    TTSHelper.PlayText("下课。下一节是" + today[nextLessonIndex].Subject + "课");
                    return;
                }
            }

            if (Settings.TimetableSettings.IsOverSpeechEnabled)
            {
                TTSHelper.PlayText("下课。下一节是" + today[nextLessonIndex].Subject + "课");
            }
        }

        private void ShowLastClassOverNotification(bool isStrongNotificationEnabled)
        {
            if (Settings.TimetableSettings.IsTimetableNotificationEnabled)
            {
                if (!isStrongNotificationEnabled)
                {
                    new TimetableNotificationWindow("课堂结束", "", Settings.TimetableSettings.OverNotificationTime, false).Show();
                }
                else
                {
                    new StrongNotificationWindow("课堂结束", "").Show();
                    TTSHelper.PlayText("课堂结束");
                    return;
                }
            }

            if (Settings.TimetableSettings.IsOverSpeechEnabled)
            {
                TTSHelper.PlayText("课堂结束");
            }
        }

        private void ScrollToCurrentLesson()
        {
            if (Settings.TimetableSettings.IsTimetableEnabled && StackPanelShowTimetable.ActualHeight > (ScrollViewerShowCurriculum.ActualHeight - 32))
            {
                int extraMarginCount = 0;
                double scale = ScaleTimetable.ScaleX;

                foreach (Lesson lesson in _timetableCurrent)
                {
                    if (lesson.IsSplitBelow) extraMarginCount++;
                    if (_timetableCurrent.IndexOf(lesson) >= _lessonIndex) break;
                }

                double offset = (((_lessonIndex + 1) * 48) + 8 * extraMarginCount - 48) * scale;
                if (offset > (StackPanelShowTimetable.ActualHeight * scale + 32 - ScrollViewerShowCurriculum.ActualHeight))
                    offset = (StackPanelShowTimetable.ActualHeight * scale + 32 - ScrollViewerShowCurriculum.ActualHeight);

                DoubleAnimation offsetAnimation = new()
                {
                    From = ScrollViewerShowCurriculum.VerticalOffset,
                    To = offset,
                    Duration = TimeSpan.FromMilliseconds(750),
                    EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut }
                };
                ScrollViewerShowCurriculum.BeginAnimation(Helpers.ScrollViewerBehavior.VerticalOffsetProperty, offsetAnimation);
            }
        }

        private int _scrollFreeTime = 0;

        private DispatcherTimer _timetableScrollTimer = new()
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        private void TimetableScrollTimer_Tick(object sender, EventArgs e)
        {
            _scrollFreeTime++;

            if (_scrollFreeTime > 5)
            {
                ScrollToCurrentLesson();
            }
        }

        private void ScrollViewerShowCurriculum_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            _scrollFreeTime = 0;
        }

        private void MenuItemTimetableAutoScroll_Click(object sender, RoutedEventArgs e)
        {
            if (MenuItemTimetableAutoScroll.IsChecked)
            {
                ScrollToCurrentLesson();
                _timetableScrollTimer.Start();
            }
            else
            {
                _timetableScrollTimer.Stop();
            }
        }
        #endregion
        #endregion

        #region Clock
        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            textBlockTime.Text = DateTime.Now.ToString("HH':'mm':'ss");
        }

        private DispatcherTimer _clockTimer;

        private void iconShowBigClock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            new FullScreenClock().Show();
        }
        #endregion

        #region Launcher
        private async void LoadLauncher()
        {
            ScrollViewerLauncher.Visibility = Visibility.Collapsed;
            ProgressBarLauncher.Visibility = Visibility.Visible;

            Dictionary<string, Drawing.Bitmap> fileInfo = new();
            string LinkPath = AppDomain.CurrentDomain.BaseDirectory + @"LauncherLinks\";
            if (!new DirectoryInfo(LinkPath).Exists)
            {
                try { new DirectoryInfo(LinkPath).Create(); }
                catch { }
            }
            try
            {
                string[] files = Directory.GetFiles(LinkPath);
                foreach (string filePath in files)
                {
                    if (!filePath.EndsWith(".lnk"))
                    {
                        continue;
                    }
                    WshShell shell = new();
                    IWshShortcut wshShortcut = (IWshShortcut)shell.CreateShortcut(filePath);
                    /*if (!File.Exists(wshShortcut.TargetPath))
                    {
                        continue;
                    }*/
                    try
                    {
#pragma warning disable CS8602 // 解引用可能出现空引用。
                        Drawing.Bitmap icon = Drawing.Icon.ExtractAssociatedIcon(wshShortcut.TargetPath).ToBitmap();
#pragma warning restore CS8602 // 解引用可能出现空引用。
                        //fileInfo.Add(wshShortcut.TargetPath, icon);
                        fileInfo.Add(filePath, icon);
                    }
                    catch { }
                }

                foreach (KeyValuePair<string, Drawing.Bitmap> file in fileInfo)
                {
                    //启动台里面的按钮
                    Button LinkButton = new()
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        Height = 36,
                        HorizontalContentAlignment = HorizontalAlignment.Left,
                        Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                        BorderThickness = new Thickness(0)
                    };

                    //按钮里面的布局
                    SimpleStackPanel ContentStackPanel = new()
                    {
                        Spacing = 8,
                        Orientation = Orientation.Horizontal
                    };

                    //图标
                    Image image = new()
                    {
                        Height = 19,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    BitmapSource bitmapSource = System.Windows.Interop.Imaging.
                        CreateBitmapSourceFromHBitmap(file.Value.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    image.Source = bitmapSource;

                    //名字
                    TextBlock textBlockFileName = new()
                    {
                        Text = Path.GetFileName(file.Key),
                        Visibility = Visibility.Collapsed
                    };

                    TextBlock textBlockLinkName = new()
                    {
                        Text = Path.GetFileName(file.Key).Remove(Path.GetFileName(file.Key).LastIndexOf("."), 4),
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    //开始组装按钮
                    ContentStackPanel.Children.Add(image);
                    ContentStackPanel.Children.Add(textBlockFileName);
                    ContentStackPanel.Children.Add(textBlockLinkName);
                    LinkButton.Content = ContentStackPanel;
                    LinkButton.Click += LinkButton_Click;

                    //往启动台里面添加按钮
                    await Task.Run(() =>
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            StackPanelLauncher.Children.Add(LinkButton);
                        }));
                    });
                }

                ScrollViewerLauncher.Visibility = Visibility.Visible;
                ProgressBarLauncher.Visibility = Visibility.Collapsed;
            }
            catch (Exception e)
            {
                MessageBox.Show("加载启动台时出现错误：\r\n" + e.Message);
            }
        }

        private void LinkButton_Click(object sender, RoutedEventArgs e)
        {
            string LinkPath = AppDomain.CurrentDomain.BaseDirectory + @"LauncherLinks\";
            string filePath = LinkPath + ((TextBlock)((SimpleStackPanel)((Button)sender).Content).Children[2]).Text + ".lnk";
            WshShell shell = new();
            IWshShortcut wshShortcut = (IWshShortcut)shell.CreateShortcut(filePath);
            Process.Start("explorer.exe", wshShortcut.TargetPath);

            if (Settings.Automation.IsBottomMost) WindowsHelper.SetBottom(window);
        }
        private void buttonExplorer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(@"C:\Windows\System32\explorer.exe", "/s,");
            }
            catch (Exception) { }

            if (Settings.Automation.IsBottomMost) WindowsHelper.SetBottom(window);
        }
        private void ButtonReloadLauncher_Click(object sender, RoutedEventArgs e)
        {
            ScrollViewerLauncher.Visibility = Visibility.Collapsed;
            ProgressBarLauncher.Visibility = Visibility.Visible;

            Button buttonExplorerBackup = buttonExplorer;

            StackPanelLauncher.Children.Clear();

            Task.Run(() =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    StackPanelLauncher.Children.Add(buttonExplorerBackup);
                }));
            });

            Task.Run(() =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    LoadLauncher();
                });
            });
        }

        private bool _isLauncherEditorOpen = false;

        private void ButtonEditLauncher_Click(object sender, RoutedEventArgs e)
        {
            if (!_isLauncherEditorOpen)
            {
                LauncherEditor launcherEditor = new();
                launcherEditor.Closed += LauncherEditor_Closed;
                _isLauncherEditorOpen = true;
                launcherEditor.Show();
            }
        }

        private void LauncherEditor_Closed(object sender, EventArgs e)
        {
            ButtonReloadLauncher_Click(null, null);
            _isLauncherEditorOpen = false;
        }

        #endregion

        #region Settings

        #region Panel Show & Hide

        private bool _isSettingsWindowOpen = false;

        private void iconShowSettingsPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            /*if (borderSettingsPanel.Visibility == Visibility.Collapsed) borderSettingsPanel.Visibility = Visibility.Visible;
            else btnHideSettingsPanel_Click(null, null);

            ButtonRefreshBNSStatus_Click(null, null);*/
            if (!_isSettingsWindowOpen)
            {
                SettingsWindow settingsWindow = new();
                settingsWindow.Closed += SettingsWindow_Closed;
                settingsWindow.Show();
                _isSettingsWindowOpen = true;
            }
        }

        private void SettingsWindow_Closed(object sender, EventArgs e)
        {
            _isSettingsWindowOpen = false;
        }

        private void WelcomeWindow_Closed(object sender, EventArgs e)
        {
            iconShowSettingsPanel.Visibility = Visibility.Visible;
        }
        #endregion
        private void LoadSettings()
        {
            if (File.Exists(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + SettingsFileName))
            {
                try
                {
                    string text = File.ReadAllText(SettingsFileName);
                    Settings = JsonConvert.DeserializeObject<Settings>(text);
                }
                catch { }
            }
            else
            {
                iconShowSettingsPanel.Visibility = Visibility.Collapsed;
                WelcomeWindow welcomeWindow = new WelcomeWindow();
                welcomeWindow.Closed += WelcomeWindow_Closed;
                welcomeWindow.Show();
            }

            ToggleButtonLock.IsChecked = Settings.Blackboard.IsLocked;

            if (Settings.Automation.IsAutoHideHugoAssistantEnabled) timerHideSeewoServiceAssistant.Start();

            _isSettingsLoaded = true;
        }
        public static Settings Settings = new Settings();
        public const string SettingsFileName = "Settings.json";
        public static void SaveSettings()
        {
            string text = JsonConvert.SerializeObject(Settings, Formatting.Indented);
            try
            {
                File.WriteAllText(SettingsFileName, text);
            }
            catch { }
        }
        #endregion

        #region Utility
        #region FileDrag

        /*private bool isCopying = false;
        private void window_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            TextBlockDragHint.Text = "松手以将文件添加到桌面";
            ProgressBarDragEnter.Visibility = Visibility.Collapsed;
            BorderDragEnter.Visibility = Visibility.Visible;
        }

        private void window_DragLeave(object sender, System.Windows.DragEventArgs e)
        {
            if (!isCopying) BorderDragEnter.Visibility = Visibility.Collapsed;
        }

        private async void window_Drop(object sender, System.Windows.DragEventArgs e)
        {
            isCopying = true;

            ProgressBarDragEnter.Visibility = Visibility.Visible;
            TextBlockDragHint.Text = "正在添加文件到桌面，请稍等";

            BorderDragEnter.Visibility = Visibility.Collapsed;

            string folderFileName;
            try { folderFileName = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString(); }
            catch
            {
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                Task.Run(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("请将此文件拖到桌面空白处", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    });
                });
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法

                BorderDragEnter.Visibility = Visibility.Collapsed;
                ProgressBarDragEnter.Visibility = Visibility.Collapsed;

                return;
            }

            string dest = Path.GetFileName(folderFileName);
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\";

            await Task.Run(() =>
            {
                if (folderFileName != desktop + dest)
                {
                    if (new DirectoryInfo(folderFileName).Exists)
                    {
                        if (new DirectoryInfo(desktop + dest).Exists) new DirectoryInfo(desktop + dest).Delete(true);
                        try
                        {
                            FileUtility.CopyFolder(folderFileName, desktop + dest);
                        }
                        catch (Exception ex) { MessageBox.Show(Convert.ToString(ex)); }
                    }
                    else
                    {
                        if (File.Exists(desktop + dest)) File.Delete(desktop + dest);
                        try
                        {
                            File.Copy(folderFileName, desktop + dest);
                        }
                        catch (Exception ex) { MessageBox.Show(Convert.ToString(ex)); }
                    }
                }
            });

            BorderDragEnter.Visibility = Visibility.Collapsed;
            ProgressBarDragEnter.Visibility = Visibility.Collapsed;

            isCopying = false;
        }*/
        private void window_Drop(object sender, System.Windows.DragEventArgs e)
        {
            Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    System.Windows.MessageBox.Show("请将此文件拖到桌面空白处", null, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                });
            });
        }
        #endregion
        #endregion

        #region Check
        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (Settings.Look.Theme == 0) SetTheme();
        }

        public static bool IsSystemThemeLight()
        {
            bool light = true;
            try
            {
                RegistryKey registryKey = Registry.CurrentUser;
                RegistryKey themeKey = registryKey.OpenSubKey("software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize");
                int keyValue = 0;
                if (themeKey != null)
                {
                    keyValue = (int)themeKey.GetValue("SystemUsesLightTheme");
                }
                if (keyValue == 1) light = true;
                else light = false;
            }
            catch { }
            return light;
        }

        #region AutoHideSeewoServiceAssistant
        public DispatcherTimer timerHideSeewoServiceAssistant = new DispatcherTimer();
        public static bool IsSeewoServiceAssistantHidden = false;
        private void TimerHideSeewoServiceAssistant_Tick(object sender, EventArgs e)
        {
            if (WindowsHelper.MinimizeSeewoServiceAssistant())
            {
                timerHideSeewoServiceAssistant.Stop();
                IsSeewoServiceAssistantHidden = true;
            }
        }
        #endregion

        #endregion

        #region InfoBoard
        private List<Page> _frameInfoPages = new();
        private int _frameInfoPageIndex = 0;
        private DispatcherTimer _frameInfoNavigationTimer = new DispatcherTimer();

        private void BorderSwitchFrameInfoPage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SwitchFrameInfoPage();
        }

        private void FrameInfoNavigationTimer_Tick(object sender, EventArgs e)
        {
            SwitchFrameInfoPage();
        }

        public void SwitchFrameInfoPage()
        {
            if (_frameInfoPages.Count == 0) return;

            _frameInfoNavigationTimer.Stop();

            FrameInfo.NavigationService.RemoveBackEntry();

            _frameInfoPageIndex++;
            if (_frameInfoPageIndex >= _frameInfoPages.Count) _frameInfoPageIndex = 0;
            FrameInfo.Navigate(_frameInfoPages[_frameInfoPageIndex]);

            if (_frameInfoPages.Count > 1) _frameInfoNavigationTimer.Start();
        }

        public void LoadFrameInfoPagesList()
        {
            _frameInfoPages.Clear();

            if (Settings.InfoBoard.isDatePageEnabled) _frameInfoPages.Add(new DatePage());
            if (Settings.InfoBoard.isCountdownPageEnabled) _frameInfoPages.Add(new CountdownPage());
            if (Settings.InfoBoard.isWeatherPageEnabled) _frameInfoPages.Add(new WeatherPage());
            if (Settings.InfoBoard.isWeatherForecastPageEnabled) _frameInfoPages.Add(new WeatherForecastPage());

            if (_frameInfoPages.Count == 0) return;

            FrameInfo.Navigate(_frameInfoPages[0]);

            if (_frameInfoPages.Count == 1)
            {
                _frameInfoNavigationTimer.Stop();
            }
            else
            {
                _frameInfoNavigationTimer.Start();
            }
        }
        #endregion

        #region Other Functions
        public static void Restart()
        {
            Process.Start(System.Windows.Forms.Application.ExecutablePath, "-m");

            IsClosingFromButton = true;
            System.Windows.Application.Current.Shutdown();
        }

        public static string GetDataPath()
        {
            string path;

            if (Settings.Storage.IsFilesSavingWithProgram)
            {
                path = System.AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                path = Settings.Storage.DataPath;
                if (!path.EndsWith("\\") || !path.EndsWith("/"))
                {
                    path += "\\";
                }

                if (!new DirectoryInfo(path).Exists)
                {
                    try { new DirectoryInfo(path).Create(); }
                    catch { }
                }
            }
            return path;
        }

        public static void SetWindowScaleTransform(double multiplier)
        {
            MainWindow window = Application.Current.MainWindow as MainWindow;

            window.windowScale.ScaleX = multiplier;
            window.windowScale.ScaleY = multiplier;
        }

        public static void SetTheme()
        {
            MainWindow window = Application.Current.MainWindow as MainWindow;

            if (Settings.Look.Theme == 1 || (Settings.Look.Theme == 0 && IsSystemThemeLight())) // 浅色模式
            {
                ThemeManager.SetRequestedTheme(window, ElementTheme.Light);
                ResourceDictionary resourceDictionary = new ResourceDictionary() { Source = new Uri("Style/Light.xaml", UriKind.Relative) };
                Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);

                if (window.ToggleButtonLock.IsChecked.Value) window.ToggleButtonLock.Foreground = new SolidColorBrush(Colors.White); else window.ToggleButtonLock.Foreground = new SolidColorBrush(Colors.Black);


                if (window.inkCanvas.DefaultDrawingAttributes.Color == Colors.White)
                {
                    window.inkCanvas.DefaultDrawingAttributes.Color = Colors.Black;
                }

                foreach (Stroke stroke in window.inkCanvas.Strokes)
                {
                    if (stroke.DrawingAttributes.Color == Colors.White)
                    {
                        stroke.DrawingAttributes.Color = Colors.Black;
                    }
                }
            }
            else if (Settings.Look.Theme == 2 || (Settings.Look.Theme == 0 && (!IsSystemThemeLight()))) // 深色模式
            {
                ThemeManager.SetRequestedTheme(window, ElementTheme.Dark);
                ResourceDictionary resourceDictionary = new ResourceDictionary() { Source = new Uri("Style/Dark.xaml", UriKind.Relative) };
                Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);

                if (window.ToggleButtonLock.IsChecked.Value) window.ToggleButtonLock.Foreground = new SolidColorBrush(Colors.Black); else window.ToggleButtonLock.Foreground = new SolidColorBrush(Colors.White);


                if (window.inkCanvas.DefaultDrawingAttributes.Color == Colors.Black)
                {
                    window.inkCanvas.DefaultDrawingAttributes.Color = Colors.White;
                }
                foreach (Stroke stroke in window.inkCanvas.Strokes)
                {
                    if (stroke.DrawingAttributes.Color == Colors.Black)
                    {
                        stroke.DrawingAttributes.Color = Colors.White;
                    }
                }
            }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/STBBRD/ZongziTEK-Blackboard-Sticker");
        }

        public static bool StartAutomaticallyCreate(string exeName)
        {
            try
            {
                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\" + exeName + ".lnk");
                shortcut.TargetPath = System.Windows.Forms.Application.ExecutablePath;
                shortcut.WorkingDirectory = System.Environment.CurrentDirectory;
                shortcut.WindowStyle = 1;
                shortcut.Description = exeName + "_link";
                shortcut.Save();
                return true;
            }
            catch (Exception) { }
            return false;
        }

        public static bool StartAutomaticallyDel(string exeName)
        {
            try
            {
                System.IO.File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\" + exeName + ".lnk");
                return true;
            }
            catch (Exception) { }
            return false;
        }

        public Rect GetTargetWorkArea()
        {
            List<Rect> monitorWorkAreas = new();
            WindowsHelper.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMonitor, IntPtr hdcMonitor, ref WindowsHelper.RECT lprcMonitor, IntPtr dwData) =>
            {
                WindowsHelper.MONITORINFO mi = new WindowsHelper.MONITORINFO();
                mi.cbSize = Marshal.SizeOf(mi);
                if (WindowsHelper.GetMonitorInfo(hMonitor, ref mi))
                {
                    WindowsHelper.GetDpiForMonitor(hMonitor, WindowsHelper.MonitorDpiType.Effective, out uint dpiX, out uint dpiY);
                    double scaleX = dpiX / 96.0;
                    monitorWorkAreas.Add(new Rect(mi.rcWork.Left / scaleX, mi.rcWork.Top / scaleX, (mi.rcWork.Right - mi.rcWork.Left) / scaleX, (mi.rcWork.Bottom - mi.rcWork.Top) / scaleX));
                }
                return true;
            }, IntPtr.Zero);

            int targetIndex = Settings.Look.TargetMonitor;
            if (monitorWorkAreas.Count == 0)
            {
                monitorWorkAreas.Add(SystemParameters.WorkArea);
                targetIndex = 0;
            }
            if (targetIndex < 0 || targetIndex >= monitorWorkAreas.Count) targetIndex = 0;
            return monitorWorkAreas[targetIndex];
        }

        public void SwitchLookMode(int mode)
        {
            BeginAnimation(LeftProperty, null);

            Rect targetWorkArea = GetTargetWorkArea();
            Left = targetWorkArea.Left;

            double liteModeWidth = ColumnLauncher.ActualWidth;

            switch (mode)            {
                case 0: // 默认
                    BorderMain.ClearValue(WidthProperty);
                    BorderMain.ClearValue(HorizontalAlignmentProperty);
                    iconSwitchLeft.Visibility = Visibility.Visible;
                    iconSwitchRight.Visibility = Visibility.Collapsed;

                    ColumnCanvas.Width = new GridLength(1, GridUnitType.Star);
                    ColumnInfoBoard.Width = new GridLength(1, GridUnitType.Star);
                    ColumnClock.Width = GridLength.Auto;

                    SwitchFrameInfoPage();
                    _frameInfoNavigationTimer.Start();

                    Width = targetWorkArea.Width / 2;
                    break;

                case 1: // 简约（顶部为时钟）
                    BorderMain.Width = liteModeWidth;
                    BorderMain.HorizontalAlignment = HorizontalAlignment.Right;
                    iconSwitchLeft.Visibility = Visibility.Collapsed;
                    iconSwitchRight.Visibility = Visibility.Collapsed;

                    ColumnCanvas.Width = new GridLength(0);
                    ColumnInfoBoard.Width = new GridLength(0);
                    ColumnClock.Width = new GridLength(1, GridUnitType.Star);

                    _frameInfoNavigationTimer.Stop();
                    if (_frameInfoPages.Count > 0) FrameInfo.Navigate(_frameInfoPages[0]);  // 切换到日期页面防止继续调用天气 API

                    Width = (liteModeWidth + BorderMain.Margin.Left + BorderMain.Margin.Right) * windowScale.ScaleX;
                    break;

                case 2: // 简约（顶部为看板）
                    BorderMain.Width = liteModeWidth;
                    BorderMain.HorizontalAlignment = HorizontalAlignment.Right;
                    iconSwitchLeft.Visibility = Visibility.Collapsed;
                    iconSwitchRight.Visibility = Visibility.Collapsed;

                    ColumnCanvas.Width = new GridLength(0);
                    ColumnClock.Width = new GridLength(0);
                    ColumnInfoBoard.Width = new GridLength(1, GridUnitType.Star);

                    SwitchFrameInfoPage();
                    _frameInfoNavigationTimer.Start();

                    Width = (liteModeWidth + BorderMain.Margin.Left + BorderMain.Margin.Right) * windowScale.ScaleX;
                    break;
            }

            UpdateLayout();

            Dispatcher.BeginInvoke(new Action(() =>
            {
                Left = targetWorkArea.Left + (targetWorkArea.Width - ActualWidth);
                Top = targetWorkArea.Top;
                Height = targetWorkArea.Height;
            }), DispatcherPriority.Render);
        }

        public static bool GetIsLightTheme()
        {
            if (Settings.Look.Theme == 1 || (Settings.Look.Theme == 0 && IsSystemThemeLight())) return true;
            else return false;
        }

        public static void CheckUpdate()
        {
            AutoUpdater.PersistenceProvider = new JsonFilePersistenceProvider("AutoUpdater.json");
            switch (Settings.Update.UpdateChannel)
            {
                case 6: // Preview 频道
                    AutoUpdater.Start($"https://update.zongzitek.top/zbsupdatepreview.xml");
                    break;
                default: // Release 频道
                    AutoUpdater.Start($"https://update.zongzitek.top/zbsupdate.xml");
                    break;
            }
            AutoUpdater.ApplicationExitEvent += () =>
            {
                Environment.Exit(0);
            };
        }
        #endregion
    }
}
