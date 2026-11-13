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
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ZongziTEK_Blackboard_Sticker.Controls;
using ZongziTEK_Blackboard_Sticker.Helpers;
using ZongziTEK_Blackboard_Sticker.Models;
using ZongziTEK_Blackboard_Sticker.Pages;
using ZongziTEK_Blackboard_Sticker.Services;
using Drawing = System.Drawing;
using File = System.IO.File;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;
using ScrollViewerBehavior = ZongziTEK_Blackboard_Sticker.Helpers.ScrollViewerBehavior;

namespace ZongziTEK_Blackboard_Sticker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // 窗体
            SetWindowVerticalSize();

            /*windowTimer.Tick += windowTimer_Tick; // 强力置底，可能导致界面闪烁，故注释
            windowTimer.Start();*/

            // 加载文件
            LoadSettings();
            DataContext = Settings;
            lastDataDirectory = GetDataPath();
            BlackboardCard.OpenStorage(lastDataDirectory);
            ApplyLauncherVisibility();
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

            // 看板
            textBlockTime.Text = DateTime.Now.ToString(("HH':'mm':'ss"));
            clockTimer = new DispatcherTimer();
            clockTimer.Tick += ClockTimer_Tick;
            clockTimer.Interval = TimeSpan.FromMilliseconds(200);
            clockTimer.Start();

            frameInfoNavigationTimer.Tick += FrameInfoNavigationTimer_Tick;
            frameInfoNavigationTimer.Interval = TimeSpan.FromSeconds(4);
            LoadFrameInfoPagesList();

            // 课程表
            timetableTimer = new DispatcherTimer();
            timetableTimer.Tick += CheckTimetable;
            timetableTimer.Interval = TimeSpan.FromSeconds(1);
            LoadTimetableOrCurriculum();
            timetableTimer.Start();

            // 颜色主题
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
            SystemEvents_UserPreferenceChanged(null, null);

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

            timetableScrollTimer.Tick += TimetableScrollTimer_Tick;
            timetableScrollTimer.Start();
            ScrollToCurrentLesson();
        }

        private void window_Activated(object sender, EventArgs e)
        {
            if (Settings.Automation.IsBottomMost) WindowsHelper.SetBottom(window);
        }

        public static bool CloseIsFromButton = false;
        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!CloseIsFromButton)
            {
                e.Cancel = true;
                if (MessageBox.Show("是否继续关闭 ZongziTEK 黑板贴", "ZongziTEK 黑板贴", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    if (MessageBox.Show("真的狠心关闭 ZongziTEK 黑板贴 吗？", "ZongziTEK 黑板贴", MessageBoxButton.OKCancel, MessageBoxImage.Error) == MessageBoxResult.OK)
                    {
                        if (MessageBox.Show("是否取消关闭 ZongziTEK 黑板贴 ？", "ZongziTEK 黑板贴", MessageBoxButton.YesNo, MessageBoxImage.Error) != MessageBoxResult.Yes)
                        {
                            e.Cancel = false;
                        }
                    }
                }
            }
            if (!e.Cancel && !BlackboardCard.Flush())
            {
                e.Cancel = MessageBox.Show("小黑板保存失败。是否放弃未保存内容并退出？", "小黑板",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes;
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

        private bool showingDetailedTimetable;
        private int? shownDutySource;
        private DutySnapshot shownTemporaryDuty;
        private string lastScheduleLoadError;

        private void ReportScheduleLoadError(Exception error)
        {
            if (lastScheduleLoadError == error.Message) return;
            lastScheduleLoadError = error.Message;
            MessageBox.Show(error.Message, "课表读取失败", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void RefreshDutyCard()
        {
            MenuCopyToTemporary.IsEnabled = shownDutySource.HasValue && shownDutySource.Value < 7;
            try
            {
                DutyCard.Present(new DutyRosterStore(GetDataPath()).Load().Present(shownDutySource, shownTemporaryDuty), null);
            }
            catch (Exception ex)
            {
                var fallback = new DutyRoster().Present(shownDutySource, shownTemporaryDuty);
                DutyCard.Present(fallback, "值日数据读取失败：" + ex.Message);
            }
        }

        private void DutyCard_EditRequested(object sender, RoutedEventArgs e)
        {
            try
            {
                var editor = new DutyEditor(new DutyRosterStore(GetDataPath()), shownDutySource) { Owner = this };
                if (editor.ShowDialog() == true) RefreshDutyCard();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "值日安排读取失败", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CopyToTemporary_Click(object sender, RoutedEventArgs e)
        {
            if (!shownDutySource.HasValue || shownDutySource.Value == 7) return;
            if (isTimetableEditorOpen)
            {
                MessageBox.Show("请先关闭课表编辑窗口，或在编辑窗口中使用复制、粘贴并保存。", "复制到临时课表");
                return;
            }

            int source = shownDutySource.Value;
            if (MessageBox.Show($"用{DutyRoster.SourceNames[source]}的课程和单双周值日安排替换临时内容？",
                "复制到临时课表", MessageBoxButton.OKCancel) != MessageBoxResult.OK) return;

            try
            {
                string directory = GetDataPath();
                var duty = new DutyRosterStore(directory).Load().Capture(source);
                if (showingDetailedTimetable)
                {
                    string path = Path.Combine(directory, timetableFileName);
                    var draft = LocalJson.ReadOrNew<Timetable>(path);
                    draft.Normalize();
                    draft.ReplaceTemporary(draft.GetDay(source), duty);
                    LocalJson.Write(path, draft);
                }
                else
                {
                    string path = Path.Combine(directory, curriculumsFileName);
                    var draft = LocalJson.ReadOrNew<Curriculums>(path);
                    draft.ReplaceTemporary(source, duty);
                    LocalJson.Write(path, draft);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "复制到临时课表失败", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            LoadTimetableOrCurriculum();
        }

        internal void ApplyLauncherVisibility()
        {
            LauncherLayout.Apply(TimetableCard, LauncherCard, Settings.Look.ShowLauncher);
        }

        #region Timetable & Curriculum

        public static int timetableToShow_index = (int)DateTime.Today.DayOfWeek;

        public void LoadTimetableOrCurriculum()
        {
            var classIslandConnectorService = App.ServiceManager.GetService<ClassIslandConnectorService>();

            showingDetailedTimetable = Settings.TimetableSettings.IsTimetableEnabled ||
                (classIslandConnectorService != null && classIslandConnectorService.IsTimetableSyncEnabled);
            if (showingDetailedTimetable)
            {
                LoadTimetable();

                textBlockCurriculum.Visibility = Visibility.Collapsed;
                StackPanelShowTimetable.Visibility = Visibility.Visible;
                MenuItemTimetableAutoScroll.IsChecked = true;

            }
            else
            {
                LoadCurriculum();

                textBlockCurriculum.Visibility = Visibility.Visible;
                StackPanelShowTimetable.Visibility = Visibility.Collapsed;
                MenuItemTimetableAutoScroll.IsChecked = false;

                MenuChooseTimetableToShow.IsEnabled = true;
                shownDutySource = timetableToShow_index;
                shownTemporaryDuty = Curriculums.TemporaryDuty;
            }

            CheckTimetableMenuItems();
            RefreshDutyCard();
        }

        private void MenuItemShowMondayTimetable_Click(object sender, RoutedEventArgs e)
        {
            timetableToShow_index = 1;
            LoadTimetableOrCurriculum();
        }

        private void MenuItemShowTuesdayTimetable_Click(object sender, RoutedEventArgs e)
        {
            timetableToShow_index = 2;
            LoadTimetableOrCurriculum();
        }

        private void MenuItemShowWednesdayTimetable_Click(object sender, RoutedEventArgs e)
        {
            timetableToShow_index = 3;
            LoadTimetableOrCurriculum();
        }

        private void MenuItemShowThursdayTimetable_Click(object sender, RoutedEventArgs e)
        {
            timetableToShow_index = 4;
            LoadTimetableOrCurriculum();
        }

        private void MenuItemShowFridayTimetable_Click(object sender, RoutedEventArgs e)
        {
            timetableToShow_index = 5;
            LoadTimetableOrCurriculum();
        }

        private void MenuItemShowSaturdayTimetable_Click(object sender, RoutedEventArgs e)
        {
            timetableToShow_index = 6;
            LoadTimetableOrCurriculum();
        }

        private void MenuItemShowSundayTimetable_Click(object sender, RoutedEventArgs e)
        {
            timetableToShow_index = 0;
            LoadTimetableOrCurriculum();
        }

        private void MenuItemShowTempTimetable_Click(object sender, RoutedEventArgs e)
        {
            timetableToShow_index = 7;
            LoadTimetableOrCurriculum();
        }

        private void CheckTimetableMenuItems() // 通过菜单项的 IsChecked 状态来告知用户正在展示哪个课表
        {
            foreach (MenuItem menuItem in MenuChooseTimetableToShow.Items)
            {
                menuItem.IsChecked = false;
            }

            switch (timetableToShow_index)
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
        public static string curriculumsFileName = "Curriculums.json";

        private bool SaveCurriculum()
        {
            try
            {
                var draft = LocalJson.ReadOrNew<Curriculums>(curriculumEditPath);
                draft.Monday.Curriculums = textBoxMonday.Text;
                draft.Tuesday.Curriculums = textBoxTuesday.Text;
                draft.Wednesday.Curriculums = textBoxWednesday.Text;
                draft.Thursday.Curriculums = textBoxThursday.Text;
                draft.Friday.Curriculums = textBoxFriday.Text;
                draft.Saturday.Curriculums = textBoxSaturday.Text;
                draft.Sunday.Curriculums = textBoxSunday.Text;
                draft.Temp.Curriculums = textBoxTempCurriculums.Text;
                LocalJson.Write(curriculumEditPath, draft);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "课程列表保存失败", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void LoadCurriculum()
        {
            try
            {
                Curriculums = LocalJson.ReadOrNew<Curriculums>(Path.Combine(GetDataPath(), curriculumsFileName));
            }
            catch (Exception ex)
            {
                Curriculums = new Curriculums();
                ReportScheduleLoadError(ex);
            }

            if (ScrollViewerCurriculum.Visibility != Visibility.Visible)
            {
                textBoxMonday.Text = Curriculums.Monday.Curriculums ?? "";
                textBoxTuesday.Text = Curriculums.Tuesday.Curriculums ?? "";
                textBoxWednesday.Text = Curriculums.Wednesday.Curriculums ?? "";
                textBoxThursday.Text = Curriculums.Thursday.Curriculums ?? "";
                textBoxFriday.Text = Curriculums.Friday.Curriculums ?? "";
                textBoxSaturday.Text = Curriculums.Saturday.Curriculums ?? "";
                textBoxSunday.Text = Curriculums.Sunday.Curriculums ?? "";
                textBoxTempCurriculums.Text = Curriculums.Temp.Curriculums ?? "";
            }

            textBlockCurriculum.FontSize = Settings.TimetableSettings.FontSize;
            textBlockCurriculum.Text = Curriculums.GetDay(timetableToShow_index) ?? "";

            if (textBlockCurriculum.Text.Trim() == string.Empty)
            {
                LabelNoClass.Visibility = Visibility.Visible;
            }
            else
            {
                LabelNoClass.Visibility = Visibility.Collapsed;
            }
        }

        private bool isTimetableEditorOpen = false;
        private string curriculumEditPath;

        private void editCurriculumButton_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.TimetableSettings.IsTimetableEnabled)
            {
                if (!isTimetableEditorOpen)
                {
                    TimetableEditor timetableEditor = new();
                    timetableEditor.Closed += TimetableEditor_Closed;
                    isTimetableEditorOpen = true;
                    timetableEditor.Show();
                }
            }
            else
            {
                curriculumEditPath = Path.Combine(GetDataPath(), curriculumsFileName);
                LoadCurriculum();
                ScrollViewerShowCurriculum.Visibility = Visibility.Collapsed;
                editCurriculumButton.Visibility = Visibility.Collapsed;

                saveCurriculumButton.Visibility = Visibility.Visible;
                ScrollViewerCurriculum.Visibility = Visibility.Visible;
            }
        }

        private void TimetableEditor_Closed(object sender, EventArgs e)
        {
            LoadTimetableOrCurriculum();
            isTimetableEditorOpen = false;
        }

        private void saveCurriculumButton_Click(object sender, RoutedEventArgs e)
        {
            if (!SaveCurriculum()) return;
            curriculumEditPath = null;

            ScrollViewerShowCurriculum.Visibility = Visibility.Visible;
            editCurriculumButton.Visibility = Visibility.Visible;

            saveCurriculumButton.Visibility = Visibility.Collapsed;
            ScrollViewerCurriculum.Visibility = Visibility.Collapsed;

            LoadTimetableOrCurriculum();
        }
        #endregion

        #region Timetable
        public static Timetable Timetable = new Timetable();
        public static string timetableFileName = "Timetable.json";
        private DispatcherTimer timetableTimer;
        List<Lesson> timetableCurrent = new();

        private void LoadTimetable()
        {
            var classIslandConnectorService = App.ServiceManager.GetService<ClassIslandConnectorService>();

            if (classIslandConnectorService == null || classIslandConnectorService.IsConnected == false || !classIslandConnectorService.IsTimetableSyncEnabled) // 加载黑板贴本地课表
            {
                try
                {
                    Timetable = LocalJson.ReadOrNew<Timetable>(Path.Combine(GetDataPath(), timetableFileName));
                    Timetable.Normalize();
                }
                catch (Exception ex)
                {
                    Timetable = new Timetable();
                    ReportScheduleLoadError(ex);
                }

                timetableCurrent = Timetable.GetDay(timetableToShow_index);
                shownDutySource = timetableToShow_index;
                shownTemporaryDuty = Timetable.TemporaryDuty;
                MenuChooseTimetableToShow.IsEnabled = true;
            }
            else // 加载共享课表，来自 ClassIsland 连接器
            {
                timetableCurrent = classIslandConnectorService.TimetableShared;
                shownDutySource = null;
                shownTemporaryDuty = null;
                MenuChooseTimetableToShow.IsEnabled = false;
            }

            PresentTimetable(timetableCurrent);
            previousReminderTime = null;

            lessonIndex = -1;

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

        private int lessonIndex = -1; // 第几节课
        private bool isInClass = false; // 是否是上课时段
        private int lastLessonIndex = -1;
        private DateTime lastScheduleDate = DateTime.Today;
        private TimeSpan? previousReminderTime;
        private double previousTimeOffset;

        private void CheckTimetable(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            int? newDay = ScheduleClock.DayAfterDateChange(lastScheduleDate, now);
            if (newDay.HasValue)
            {
                lastScheduleDate = now.Date;
                timetableToShow_index = newDay.Value;
                LoadTimetableOrCurriculum();
            }
            if (!showingDetailedTimetable) return;

            double offset = Settings.TimetableSettings.TimeOffset;
            TimeSpan currentTime = now.TimeOfDay + TimeSpan.FromSeconds(offset);
            TimeSpan? previousTime = offset == previousTimeOffset ? previousReminderTime : null;
            previousReminderTime = currentTime;
            previousTimeOffset = offset;

            if (timetableCurrent != null && timetableCurrent.Count != 0)
            {
                // 获取上课状态 lessonIndex 和 isInClass
                foreach (var lesson in timetableCurrent)
                {
                    if (currentTime >= lesson.StartTime) // 在这节课开始后
                    {
                        if (timetableCurrent.IndexOf(lesson) + 1 < timetableCurrent.Count) // 不是最后一节课
                        {
                            if (currentTime < timetableCurrent[timetableCurrent.IndexOf(lesson) + 1].StartTime) // 在下一节课上课前
                            {
                                lessonIndex = timetableCurrent.IndexOf(lesson);
                                isInClass = currentTime < lesson.EndTime;
                                break;
                            }
                        }
                        else // 是最后一节课
                        {
                            lessonIndex = timetableCurrent.Count - 1;
                            isInClass = currentTime < lesson.EndTime;
                            break;
                        }
                    }
                    else if (timetableCurrent.IndexOf(lesson) == 0) // 在第一节课开始前
                    {
                        lessonIndex = -1;
                        isInClass = false;
                        break;
                    }
                }

                CheckLessonReminders(previousTime, currentTime);

                // 在界面中高亮当前课程或下一节课
                int lessonToHighlightIndex = -1; // lessonToHighlightIndex 为 -1 时，不高亮任何课程

                if (isInClass) // 上课时，高亮当前课程
                {
                    lessonToHighlightIndex = lessonIndex;
                }
                else if (lessonIndex + 1 < timetableCurrent.Count) // 在第一节课前或课间，高亮下一节课
                {
                    lessonToHighlightIndex = lessonIndex + 1;
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
                            TimeSpan second = new TimeSpan(now.Hour, now.Minute, now.Second) + TimeSpan.FromSeconds(offset);
                            TimeSpan timeLeft = timetableCurrent[lessonToHighlightIndex].EndTime - second;
                            if (isInClass)
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
                            timetableLesson.Time = timetableCurrent[StackPanelShowTimetable.Children.IndexOf(timetableLesson)].StartTime.ToString(@"hh\:mm");
                        }
                    }
                }

                // 自动滚动课程表
                if (lessonIndex != lastLessonIndex) ScrollToCurrentLesson();
                lastLessonIndex = lessonIndex;
            }

        }

        private void CheckLessonReminders(TimeSpan? previous, TimeSpan current)
        {
            for (int index = 0; index < timetableCurrent.Count; index++)
            {
                Lesson lesson = timetableCurrent[index];
                bool hasNext = index + 1 < timetableCurrent.Count;
                if ((!hasNext || lesson.EndTime < timetableCurrent[index + 1].StartTime) &&
                    ScheduleClock.CrossedReminder(previous, current, lesson.EndTime))
                {
                    if (hasNext) ShowClassOverNotification(timetableCurrent, index);
                    else ShowLastClassOverNotification(lesson.IsStrongClassOverNotificationEnabled);
                }

                TimeSpan advance = lesson.StartTime - TimeSpan.FromSeconds(Settings.TimetableSettings.BeginNotificationTime);
                if ((index == 0 || timetableCurrent[index - 1].EndTime <= advance) &&
                    ScheduleClock.CrossedReminder(previous, current, advance))
                    ShowClassBeginPreNotification(timetableCurrent, index - 1);
            }
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

                foreach (Lesson lesson in timetableCurrent)
                {
                    if (lesson.IsSplitBelow) extraMarginCount++;
                    if (timetableCurrent.IndexOf(lesson) >= lessonIndex) break;
                }

                double offset = (((lessonIndex + 1) * 48) + 8 * extraMarginCount - 48) * scale;
                if (offset > (StackPanelShowTimetable.ActualHeight * scale + 32 - ScrollViewerShowCurriculum.ActualHeight))
                    offset = (StackPanelShowTimetable.ActualHeight * scale + 32 - ScrollViewerShowCurriculum.ActualHeight);

                DoubleAnimation offsetAnimation = new()
                {
                    From = ScrollViewerShowCurriculum.VerticalOffset,
                    To = offset,
                    Duration = TimeSpan.FromMilliseconds(750),
                    EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut }
                };
                ScrollViewerShowCurriculum.BeginAnimation(ScrollViewerBehavior.VerticalOffsetProperty, offsetAnimation);
            }
        }

        private int scrollFreeTime = 0;

        private DispatcherTimer timetableScrollTimer = new()
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        private void TimetableScrollTimer_Tick(object sender, EventArgs e)
        {
            scrollFreeTime++;

            if (scrollFreeTime > 5)
            {
                ScrollToCurrentLesson();
            }
        }

        private void ScrollViewerShowCurriculum_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            scrollFreeTime = 0;
        }

        private void MenuItemTimetableAutoScroll_Click(object sender, RoutedEventArgs e)
        {
            if (MenuItemTimetableAutoScroll.IsChecked)
            {
                ScrollToCurrentLesson();
                timetableScrollTimer.Start();
            }
            else
            {
                timetableScrollTimer.Stop();
            }
        }
        #endregion
        #endregion

        #region Clock
        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            textBlockTime.Text = DateTime.Now.ToString("HH':'mm':'ss");
        }

        private DispatcherTimer clockTimer;

        private void iconShowBigClock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            new FullScreenClock(GetTargetWorkArea()).Show();
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

        private bool isLauncherEditorOpen = false;

        private void ButtonEditLauncher_Click(object sender, RoutedEventArgs e)
        {
            if (!isLauncherEditorOpen)
            {
                LauncherEditor launcherEditor = new();
                launcherEditor.Closed += LauncherEditor_Closed;
                isLauncherEditorOpen = true;
                launcherEditor.Show();
            }
        }

        private void LauncherEditor_Closed(object sender, EventArgs e)
        {
            ButtonReloadLauncher_Click(null, null);
            isLauncherEditorOpen = false;
        }

        #endregion

        #region Settings

        #region Panel Show & Hide

        private bool isSettingsWindowOpen = false;
        private string lastDataDirectory;

        private void iconShowSettingsPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            /*if (borderSettingsPanel.Visibility == Visibility.Collapsed) borderSettingsPanel.Visibility = Visibility.Visible;
            else btnHideSettingsPanel_Click(null, null);

            ButtonRefreshBNSStatus_Click(null, null);*/
            if (!isSettingsWindowOpen)
            {
                SettingsWindow settingsWindow = new();
                settingsWindow.Closed += SettingsWindow_Closed;
                settingsWindow.Show();
                isSettingsWindowOpen = true;
            }
        }

        private void SettingsWindow_Closed(object sender, EventArgs e)
        {
            isSettingsWindowOpen = false;
            string directory = GetDataPath();
            BlackboardCard.OpenStorage(directory);
            if (string.Equals(lastDataDirectory, directory, StringComparison.OrdinalIgnoreCase)) return;
            lastDataDirectory = directory;
            LoadTimetableOrCurriculum();
        }

        private void WelcomeWindow_Closed(object sender, EventArgs e)
        {
            iconShowSettingsPanel.Visibility = Visibility.Visible;
        }
        #endregion
        private void LoadSettings()
        {
            try
            {
                Settings = LocalJson.Parse<Settings>(AtomicTextFile.Read(SettingsPath));
            }
            catch (FileNotFoundException)
            {
                iconShowSettingsPanel.Visibility = Visibility.Collapsed;
                WelcomeWindow welcomeWindow = new WelcomeWindow();
                welcomeWindow.Closed += WelcomeWindow_Closed;
                welcomeWindow.Show();
            }
            catch (Exception ex)
            {
                settingsLoadFailed = true;
                MessageBox.Show(ex.Message, "设置读取失败，将使用默认设置", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            if (Settings.Automation.IsAutoHideHugoAssistantEnabled) timerHideSeewoServiceAssistant.Start();
        }

        public static Settings Settings = new Settings();
        public static string settingsFileName = "Settings.json";
        private static string SettingsPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, settingsFileName);
        private static bool settingsLoadFailed;

        public static void SaveSettings() => TrySaveSettings();

        internal static bool TrySaveSettings()
        {
            try
            {
                if (settingsLoadFailed)
                    throw new InvalidDataException("原设置文件读取失败。请修复该文件并重启后再保存设置。");
                LocalJson.Write(SettingsPath, Settings);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "设置保存失败", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
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
        public static bool isSeewoServiceAssistantHided = false;
        private void TimerHideSeewoServiceAssistant_Tick(object sender, EventArgs e)
        {
            if (WindowsHelper.MinimizeSeewoServiceAssistant())
            {
                timerHideSeewoServiceAssistant.Stop();
                isSeewoServiceAssistantHided = true;
            }
        }
        #endregion

        #endregion

        #region InfoBoard
        public List<Page> frameInfoPages = new();
        private int frameInfoPageIndex = 0;
        private DispatcherTimer frameInfoNavigationTimer = new DispatcherTimer();

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
            frameInfoNavigationTimer.Stop();
            if (frameInfoPages.Count == 0 || ColumnInfoBoard.Width.Value == 0) return;

            FrameInfo.NavigationService.RemoveBackEntry();

            frameInfoPageIndex++;
            if (frameInfoPageIndex >= frameInfoPages.Count) frameInfoPageIndex = 0;
            FrameInfo.Navigate(frameInfoPages[frameInfoPageIndex]);

            if (frameInfoPages.Count > 1) frameInfoNavigationTimer.Start();
        }

        public void LoadFrameInfoPagesList()
        {
            frameInfoNavigationTimer.Stop();
            frameInfoPageIndex = 0;
            frameInfoPages.Clear();

            if (Settings.InfoBoard.isDatePageEnabled) frameInfoPages.Add(new DatePage());
            if (Settings.InfoBoard.isCountdownPageEnabled) frameInfoPages.Add(new CountdownPage());
            if (Settings.InfoBoard.isWeatherPageEnabled) frameInfoPages.Add(new WeatherPage());
            if (Settings.InfoBoard.isWeatherForecastPageEnabled) frameInfoPages.Add(new WeatherForecastPage());

            FrameInfo.NavigationService.RemoveBackEntry();
            if (frameInfoPages.Count == 0 || ColumnInfoBoard.Width.Value == 0)
            {
                FrameInfo.Content = null;
                return;
            }

            FrameInfo.Navigate(frameInfoPages[0]);
            if (frameInfoPages.Count > 1) frameInfoNavigationTimer.Start();
        }
        #endregion

        #region Other Functions
        public static void Restart()
        {
            if (Application.Current.MainWindow is MainWindow window && !window.BlackboardCard.Flush())
            {
                MessageBox.Show("请先处理小黑板保存错误，或导出文本。", "小黑板", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Process.Start(System.Windows.Forms.Application.ExecutablePath, "-m");

            CloseIsFromButton = true;
            System.Windows.Application.Current.Shutdown();
        }

        public static string GetDataPath()
        {
            string path = Settings.Storage.DataPath;
            if (Settings.Storage.IsFilesSavingWithProgram || string.IsNullOrWhiteSpace(path) || !Path.IsPathRooted(path))
                path = AppDomain.CurrentDomain.BaseDirectory;

            try { Directory.CreateDirectory(path); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        }

        public static void SetWindowScaleTransform(double Multiplier)
        {
            MainWindow window = Application.Current.MainWindow as MainWindow;

            window.windowScale.ScaleX = Multiplier;
            window.windowScale.ScaleY = Multiplier;
            if (window.IsLoaded) window.ResizeForCurrentMode(window.GetTargetWorkArea());
        }

        public static void SetTheme()
        {
            MainWindow window = Application.Current.MainWindow as MainWindow;

            if (Settings.Look.Theme == 1 || (Settings.Look.Theme == 0 && IsSystemThemeLight())) // 浅色模式
            {
                ThemeManager.SetRequestedTheme(window, ElementTheme.Light);
                ResourceDictionary resourceDictionary = new ResourceDictionary() { Source = new Uri("Style/Light.xaml", UriKind.Relative) };
                Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);

            }
            else if (Settings.Look.Theme == 2 || (Settings.Look.Theme == 0 && (!IsSystemThemeLight()))) // 深色模式
            {
                ThemeManager.SetRequestedTheme(window, ElementTheme.Dark);
                ResourceDictionary resourceDictionary = new ResourceDictionary() { Source = new Uri("Style/Dark.xaml", UriKind.Relative) };
                Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);

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

            switch (mode)
            {
                case 0: // 默认
                    BorderMain.ClearValue(WidthProperty);
                    BorderMain.ClearValue(HorizontalAlignmentProperty);
                    iconSwitchLeft.Visibility = Visibility.Visible;
                    iconSwitchRight.Visibility = Visibility.Collapsed;

                    LeftCards.Visibility = Visibility.Visible;
                    ColumnCanvas.Width = new GridLength(1, GridUnitType.Star);
                    ColumnInfoBoard.Width = new GridLength(1, GridUnitType.Star);
                    ColumnClock.Width = GridLength.Auto;

                    SwitchFrameInfoPage();

                    break;

                case 1: // 简约（顶部为时钟）
                    BorderMain.HorizontalAlignment = HorizontalAlignment.Right;
                    iconSwitchLeft.Visibility = Visibility.Collapsed;
                    iconSwitchRight.Visibility = Visibility.Collapsed;

                    LeftCards.Visibility = Visibility.Collapsed;
                    ColumnCanvas.Width = new GridLength(0);
                    ColumnInfoBoard.Width = new GridLength(0);
                    ColumnClock.Width = new GridLength(1, GridUnitType.Star);

                    frameInfoNavigationTimer.Stop();
                    FrameInfo.Content = null;

                    break;

                case 2: // 简约（顶部为看板）
                    BorderMain.HorizontalAlignment = HorizontalAlignment.Right;
                    iconSwitchLeft.Visibility = Visibility.Collapsed;
                    iconSwitchRight.Visibility = Visibility.Collapsed;

                    LeftCards.Visibility = Visibility.Collapsed;
                    ColumnCanvas.Width = new GridLength(0);
                    ColumnClock.Width = new GridLength(0);
                    ColumnInfoBoard.Width = new GridLength(1, GridUnitType.Star);

                    SwitchFrameInfoPage();

                    break;
            }

            ResizeForCurrentMode(targetWorkArea);
        }

        private void ResizeForCurrentMode(Rect targetWorkArea)
        {
            double availableWidth = targetWorkArea.Width / windowScale.ScaleX;
            double fullModeWidth = Math.Min(availableWidth, Math.Max(availableWidth / 2, 365));
            double rightColumnWeight = ColumnLauncher.Width.Value;
            double liteModeWidth = Math.Min(Math.Max(0, availableWidth - 16), Math.Max(116,
                (fullModeWidth - 16) * rightColumnWeight / (1 + rightColumnWeight)));

            if (LeftCards.Visibility == Visibility.Visible)
                Width = fullModeWidth * windowScale.ScaleX;
            else
            {
                BorderMain.Width = liteModeWidth;
                Width = (liteModeWidth + 16) * windowScale.ScaleX;
            }
            bool dockLeft = iconSwitchRight.Visibility == Visibility.Visible;
            UpdateLayout();

            Dispatcher.BeginInvoke(new Action(() =>
            {
                Left = dockLeft ? targetWorkArea.Left : targetWorkArea.Left + (targetWorkArea.Width - ActualWidth);
                Top = targetWorkArea.Top;
                Height = targetWorkArea.Height;
            }), DispatcherPriority.Render);
        }

        public static bool GetIsLightTheme()
        {
            if (Settings.Look.Theme == 1 || (Settings.Look.Theme == 0 && IsSystemThemeLight())) return true;
            else return false;
        }

        #endregion
    }
}
