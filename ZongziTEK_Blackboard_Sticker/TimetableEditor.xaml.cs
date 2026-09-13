using iNKORE.UI.WPF.Helpers;
using Microsoft.Win32;
using Newtonsoft.Json;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ZongziTEK.BlackboardSticker.Models;
using ZongziTEK_Blackboard_Sticker.Models;
using ZongziTEK_Blackboard_Sticker.Services;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace ZongziTEK_Blackboard_Sticker
{
    /// <summary>
    /// TimetableEditor.xaml 的交互逻辑
    /// </summary>
    public partial class TimetableEditor : Window
    {
        public TimetableEditor()
        {
            InitializeComponent();

            var classIslandConnectorService = App.ServiceManager.GetService<ClassIslandConnectorService>();
            if (classIslandConnectorService != null && classIslandConnectorService.IsConnected && classIslandConnectorService.IsTimetableSyncEnabled)
            {
                InfoBarSharedTimetableEnabled.IsOpen = true;
            }

            if (File.Exists(MainWindow.GetDataPath() + MainWindow.timetableFileName))
            {
                try
                {
                    string text = File.ReadAllText(MainWindow.GetDataPath() + MainWindow.timetableFileName);
                    Timetable = JsonConvert.DeserializeObject<Timetable>(text);
                }
                catch { }
            }

            Timetable ??= new Timetable();
            Timetable.EnsureMigrated();
            LoadScheduleList();
            AutoSelectTimetableToEdit();
            LoadSelectedScheduleEditor();
        }

        #region Window & Controls
        private bool isCloseWithoutWarning = false;

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!isCloseWithoutWarning && isEdited)
            {
                e.Cancel = true;
                if (MessageBox.Show("部分修改仍未保存，是否保存这些更改？", "是否保存更改", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    ButtonSave_Click(null, null);
                    e.Cancel = false;
                }
                else
                {
                    if (MessageBox.Show("此操作将导致刚才的更改丢失，确定不要保存课表吗？", "关闭而不保存", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        e.Cancel = false;
                    }
                }
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            Timetable.Sort(Timetable);

            string text = JsonConvert.SerializeObject(Timetable, Formatting.Indented);
            try
            {
                File.WriteAllText(MainWindow.GetDataPath() + MainWindow.timetableFileName, text);
            }
            catch { }
            isEdited = false;

            LoadTimetable();

            //isCloseWithoutWarning = true;
            //Close();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void ComboBoxDay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadTimetable();
            if (ComboBoxDay.SelectedIndex == 7)
            {
                InfoBarTempTimetable.IsOpen = true;

                DoubleAnimation heightAnimation = new()
                {
                    From = 0,
                    To = 50,
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
                };
                InfoBarTempTimetable.BeginAnimation(HeightProperty, heightAnimation);
            }
            else if (InfoBarTempTimetable.IsOpen)
            {
                DoubleAnimation heightAnimation = new()
                {
                    From = 50,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseIn }
                };
                InfoBarTempTimetable.BeginAnimation(HeightProperty, heightAnimation);

                await Task.Delay(500);
                InfoBarTempTimetable.IsOpen = false;
            }
        }

        private void Item_LessonInfoChanged(object sender, EventArgs e)
        {
            if (sender is TimetableEditorItem)
            {
                TimetableEditorItem changedItem = sender as TimetableEditorItem;
                int index = ListStackPanel.Children.IndexOf(changedItem);

                try
                {
                    Lesson lesson = new Lesson(changedItem.Subject, TimeSpan.Parse(changedItem.StartTime), TimeSpan.Parse(changedItem.EndTime), changedItem.IsSplitBelow, changedItem.IsStrongClassOverNotificationEnabled);
                    GetSelectedDay()[index] = lesson;
                    changedItem.BeginAnimation(MarginProperty, null);
                    if (changedItem.IsSplitBelow)
                    {
                        changedItem.Margin = new Thickness(0, 0, 0, 8);
                    }
                    else
                    {
                        changedItem.Margin = new Thickness(0);
                    }
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }

            isEdited = true;
        }

        private void Item_LessonDeleting(object sender, EventArgs e)
        {
            if (sender is TimetableEditorItem)
            {
                TimetableEditorItem itemToDelete = sender as TimetableEditorItem;
                int index = ListStackPanel.Children.IndexOf(itemToDelete);

                ListStackPanel.Children.RemoveAt(index);
                GetSelectedDay().RemoveAt(index);
            }

            isEdited = true;

            SetButtonCopyIsEnabled();
            TextBlockLessonCount.Text = GetSelectedDay().Count.ToString();
        }

        private void ButtonInsertLesson_Click(object sender, RoutedEventArgs e)
        {
            TimetableEditorItem item = new TimetableEditorItem()
            {
                Subject = "",
                StartTime = "00:00",
                EndTime = "00:00",
                IsSplitBelow = false,
                IsStrongClassOverNotificationEnabled = false
            };
            item.LessonInfoChanged += Item_LessonInfoChanged;
            item.LessonDeleting += Item_LessonDeleting;
            GetSelectedDay().Add(new Lesson("", new TimeSpan(0, 0, 0), new TimeSpan(0, 0, 0), false, false));
            ListStackPanel.Children.Add(item);

            isEdited = true;

            SetButtonCopyIsEnabled();
            TextBlockLessonCount.Text = GetSelectedDay().Count.ToString();
        }

        private void ScrollViewerTimetable_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) ScrollViewerTimetable.LineUp();
            else ScrollViewerTimetable.LineDown();
        }
        #endregion

        #region Load & Save
        private Timetable Timetable = new Timetable();
        private bool isEdited = false;
        private bool isLoadingConditionEditor = false;

        private void LoadTimetable()
        {
            TextBlockLessonCount.Text = GetSelectedDay().Count.ToString();

            ListStackPanel.Children.Clear();

            ScrollViewerTimetable.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            ScrollViewerTimetable.ScrollToTop();

            foreach (Lesson lesson in GetSelectedDay())
            {
                TimetableEditorItem item = new TimetableEditorItem()
                {
                    Subject = lesson.Subject,
                    StartTime = lesson.StartTime.ToString(@"hh\:mm"),
                    EndTime = lesson.EndTime.ToString(@"hh\:mm"),
                    IsSplitBelow = lesson.IsSplitBelow,
                    IsStrongClassOverNotificationEnabled = lesson.IsStrongClassOverNotificationEnabled
                };
                if (lesson.IsSplitBelow)
                {
                    item.Margin = new Thickness(0, 0, 0, 8);
                }
                item.LessonInfoChanged += Item_LessonInfoChanged;
                item.LessonDeleting += Item_LessonDeleting;
                ListStackPanel.Children.Add(item);

                if (MainWindow.Settings.Look.IsAnimationEnhanced)
                {
                    DoubleAnimation opacityAnimation = new()
                    {
                        From = 0,
                        To = 1,
                        Duration = TimeSpan.FromMilliseconds(500),
                        EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
                    };
                    item.BeginAnimation(OpacityProperty, opacityAnimation);

                    ThicknessAnimation marginAnimation = new()
                    {
                        From = new Thickness(0, 24, 0, item.Margin.Bottom),
                        To = new Thickness(0, 0, 0, item.Margin.Bottom),
                        Duration = TimeSpan.FromMilliseconds(500),
                        EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
                    };
                    item.BeginAnimation(MarginProperty, marginAnimation);

                }
            }

            ScrollViewerTimetable.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            SetButtonCopyIsEnabled();
        }
        #endregion

        #region ClipBoard
        private List<Lesson> ClipBoard = new List<Lesson>();

        private async void ButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            ClipBoard = GetSelectedDay().ToList();

            if (ButtonCopy.ActualWidth > 32)
            {
                DoubleAnimation buttonCopyWidthAnimation = new()
                {
                    From = ButtonCopy.ActualWidth,
                    To = 32,
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut }
                };
                ButtonCopy.BeginAnimation(WidthProperty, buttonCopyWidthAnimation);

                ThicknessAnimation buttonCopyPaddingAnimation = new()
                {
                    From = new Thickness(10, 0, 10, 0),
                    To = new Thickness(7, 0, 0, 0),
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut }
                };
                ButtonCopy.BeginAnimation(PaddingProperty, buttonCopyPaddingAnimation);

                DoubleAnimation buttonPasteWidthAnimation = new()
                {
                    From = 0,
                    To = 32,
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut }
                };
                ButtonPaste.BeginAnimation(WidthProperty, buttonPasteWidthAnimation);
                ButtonPaste.Visibility = Visibility.Visible;

                DoubleAnimation buttonPasteOpacityAnimation = new()
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut }
                };
                ButtonPaste.BeginAnimation(OpacityProperty, buttonPasteOpacityAnimation);

                await Task.Delay(500);
                LabelCopy.Visibility = Visibility.Collapsed;
            }
        }

        private void ButtonPaste_Click(object sender, RoutedEventArgs e)
        {
            if (GetSelectedDay().Count > 0)
            {
                if (MessageBox.Show("这里原有的课程将被覆盖", "确定要粘贴吗？", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            GetSelectedDay().Clear();
            foreach (Lesson lesson in ClipBoard)
            {
                GetSelectedDay().Add(lesson);
            }
            LoadTimetable();

            isEdited = true;
        }

        #endregion

        #region Other Functions
        private List<Lesson> GetSelectedDay()
        {
            Timetable.EnsureMigrated();
            TimetableDefinition schedule = GetSelectedSchedule();
            if (schedule == null)
            {
                schedule = new TimetableDefinition { Name = "新课表" };
                Timetable.Schedules.Add(schedule);
            }
            switch (ComboBoxDay.SelectedIndex)
            {
                case 0:
                    return schedule.Monday;
                case 1:
                    return schedule.Tuesday;
                case 2:
                    return schedule.Wednesday;
                case 3:
                    return schedule.Thursday;
                case 4:
                    return schedule.Friday;
                case 5:
                    return schedule.Saturday;
                case 6:
                    return schedule.Sunday;
                case 7:
                    return schedule.Monday;
            }
            return schedule.Monday;
        }

        private TimetableDefinition GetSelectedSchedule()
        {
            if (ComboBoxSchedule.SelectedIndex >= 0 &&
                ComboBoxSchedule.SelectedIndex < Timetable.Schedules.Count)
                return Timetable.Schedules[ComboBoxSchedule.SelectedIndex];
            return Timetable.Schedules.FirstOrDefault();
        }

        private void LoadScheduleList(string preferredId = null)
        {
            string currentId = preferredId;
            if (string.IsNullOrEmpty(currentId) &&
                ComboBoxSchedule.SelectedIndex >= 0 &&
                ComboBoxSchedule.SelectedIndex < Timetable.Schedules.Count)
            {
                currentId = Timetable.Schedules[ComboBoxSchedule.SelectedIndex].Id;
            }
            ComboBoxSchedule.SelectionChanged -= ComboBoxSchedule_SelectionChanged;
            ComboBoxSchedule.Items.Clear();
            foreach (var schedule in Timetable.Schedules)
                ComboBoxSchedule.Items.Add(schedule.Name);
            if (ComboBoxSchedule.Items.Count > 0)
            {
                int selectedIndex = 0;
                if (!string.IsNullOrEmpty(currentId))
                {
                    int index = Timetable.Schedules.FindIndex(x => x.Id == currentId);
                    if (index >= 0) selectedIndex = index;
                }
                else
                {
                    string selectedId = MainWindow.TimetableSelection?.TimetableId;
                    if (MainWindow.TimetableSelection != null &&
                        !MainWindow.TimetableSelection.IsAutomatic &&
                        !string.IsNullOrEmpty(selectedId))
                    {
                        int index = Timetable.Schedules.FindIndex(x => x.Id == selectedId);
                        if (index >= 0) selectedIndex = index;
                    }
                    else
                    {
                        TimetableDefinition current = Timetable.ResolveAutomatic(
                            DateTime.Now,
                            MainWindow.Settings.TimetableSettings.SchoolStartDate);
                        int index = current == null ? -1 : Timetable.Schedules.IndexOf(current);
                        if (index >= 0) selectedIndex = index;
                    }
                }
                ComboBoxSchedule.SelectedIndex = selectedIndex;
            }
            ComboBoxSchedule.SelectionChanged += ComboBoxSchedule_SelectionChanged;
        }

        private void ComboBoxSchedule_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxSchedule.SelectedIndex >= 0)
                LoadSelectedScheduleEditor();
        }

        private void LoadSelectedScheduleEditor()
        {
            var schedule = GetSelectedSchedule();
            if (schedule == null) return;

            TextBoxScheduleName.TextChanged -= TextBoxScheduleName_TextChanged;
            TextBoxScheduleName.Text = schedule.Name;
            TextBoxScheduleName.TextChanged += TextBoxScheduleName_TextChanged;
            LoadConditionEditor();
            LoadTimetable();
        }

        private void TextBoxScheduleName_TextChanged(object sender, TextChangedEventArgs e)
        {
            var schedule = GetSelectedSchedule();
            if (schedule == null || string.IsNullOrWhiteSpace(TextBoxScheduleName.Text)) return;
            schedule.Name = TextBoxScheduleName.Text.Trim();
            int index = ComboBoxSchedule.SelectedIndex;
            if (index >= 0 && index < ComboBoxSchedule.Items.Count)
            {
                ComboBoxSchedule.SelectionChanged -= ComboBoxSchedule_SelectionChanged;
                ComboBoxSchedule.Items[index] = schedule.Name;
                ComboBoxSchedule.SelectionChanged += ComboBoxSchedule_SelectionChanged;
            }
            isEdited = true;
        }

        private void ComboBoxCondition_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isLoadingConditionEditor) return;
            var schedule = GetSelectedSchedule();
            if (schedule == null || ComboBoxCondition.SelectedIndex < 0) return;
            schedule.ConditionType = (TimetableConditionType)ComboBoxCondition.SelectedIndex;
            LoadConditionEditor();
            isEdited = true;
        }

        private void TextBoxConditionValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isLoadingConditionEditor) return;
            if (ComboBoxCondition.SelectedIndex < 0) return;
            var schedule = GetSelectedSchedule();
            if (schedule == null) return;
            try
            {
                if (ComboBoxCondition.SelectedIndex == 0 ||
                    ComboBoxCondition.SelectedIndex == (int)TimetableConditionType.WeekParity)
                {
                    schedule.Weekdays = TextBoxConditionValue.Text.Split(',')
                        .Select(ParseChineseDay).Where(x => x.HasValue).Select(x => x.Value).Distinct().ToList();
                }
                else if (ComboBoxCondition.SelectedIndex == 1)
                {
                    schedule.Dates = TextBoxConditionValue.Text.Split(',')
                        .Select(x => DateTime.TryParse(x.Trim(), out var date) ? (DateTime?)date.Date : null)
                        .Where(x => x.HasValue).Select(x => x.Value).ToList();
                }
                isEdited = true;
            }
            catch { }
        }

        private void DatePickerConditionDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isLoadingConditionEditor || DatePickerConditionDate.SelectedDate == null) return;
            var schedule = GetSelectedSchedule();
            if (schedule == null || schedule.ConditionType != TimetableConditionType.Dates) return;

            DateTime selectedDate = DatePickerConditionDate.SelectedDate.Value.Date;
            schedule.Dates ??= new List<DateTime>();
            if (!schedule.Dates.Any(x => x.Date == selectedDate))
            {
                schedule.Dates.Add(selectedDate);
                LoadConditionEditor();
                isEdited = true;
            }
        }

        private void ComboBoxWeekParity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isLoadingConditionEditor || ComboBoxWeekParity.SelectedIndex < 0) return;
            var schedule = GetSelectedSchedule();
            if (schedule == null || schedule.ConditionType != TimetableConditionType.WeekParity) return;
            schedule.WeekParity = ComboBoxWeekParity.SelectedIndex == 0 ? WeekParity.Odd : WeekParity.Even;
            isEdited = true;
        }

        private static DayOfWeek? ParseChineseDay(string value) => value.Trim() switch
        {
            "一" or "周一" or "星期一" => DayOfWeek.Monday,
            "二" or "周二" or "星期二" => DayOfWeek.Tuesday,
            "三" or "周三" or "星期三" => DayOfWeek.Wednesday,
            "四" or "周四" or "星期四" => DayOfWeek.Thursday,
            "五" or "周五" or "星期五" => DayOfWeek.Friday,
            "六" or "周六" or "星期六" => DayOfWeek.Saturday,
            "日" or "天" or "周日" or "星期日" => DayOfWeek.Sunday,
            _ => null
        };

        private void LoadConditionEditor()
        {
            var schedule = GetSelectedSchedule();
            if (schedule == null) return;
            isLoadingConditionEditor = true;
            try
            {
                ComboBoxCondition.SelectionChanged -= ComboBoxCondition_SelectionChanged;
                TextBoxConditionValue.TextChanged -= TextBoxConditionValue_TextChanged;
                DatePickerConditionDate.SelectedDateChanged -= DatePickerConditionDate_SelectedDateChanged;
                ComboBoxWeekParity.SelectionChanged -= ComboBoxWeekParity_SelectionChanged;
                ComboBoxCondition.SelectedIndex = (int)schedule.ConditionType;
                TextBoxConditionValue.Text = schedule.ConditionType switch
                {
                    TimetableConditionType.Weekdays or TimetableConditionType.WeekParity =>
                        string.Join(",", schedule.Weekdays.Select(x => x switch
                        {
                            DayOfWeek.Monday => "一", DayOfWeek.Tuesday => "二", DayOfWeek.Wednesday => "三",
                            DayOfWeek.Thursday => "四", DayOfWeek.Friday => "五", DayOfWeek.Saturday => "六", _ => "日"
                        })),
                    TimetableConditionType.Dates => string.Join(",", schedule.Dates.Select(x => x.ToString("yyyy-MM-dd"))),
                    _ => ""
                };
                bool hasWeekdayValue = schedule.ConditionType == TimetableConditionType.Weekdays ||
                                       schedule.ConditionType == TimetableConditionType.WeekParity;
                TextBoxConditionValue.IsEnabled = hasWeekdayValue ||
                                                   schedule.ConditionType == TimetableConditionType.Dates;
                TextBoxConditionValue.Visibility = hasWeekdayValue ||
                                                   schedule.ConditionType == TimetableConditionType.Dates
                    ? Visibility.Visible : Visibility.Collapsed;
                DatePickerConditionDate.Visibility =
                    schedule.ConditionType == TimetableConditionType.Dates
                        ? Visibility.Visible : Visibility.Collapsed;
                DatePickerConditionDate.SelectedDate = null;
                ComboBoxWeekParity.Visibility =
                    schedule.ConditionType == TimetableConditionType.WeekParity
                        ? Visibility.Visible : Visibility.Collapsed;
                ComboBoxWeekParity.SelectedIndex = schedule.WeekParity == WeekParity.Odd ? 0 : 1;
            }
            finally
            {
                ComboBoxCondition.SelectionChanged += ComboBoxCondition_SelectionChanged;
                TextBoxConditionValue.TextChanged += TextBoxConditionValue_TextChanged;
                DatePickerConditionDate.SelectedDateChanged += DatePickerConditionDate_SelectedDateChanged;
                ComboBoxWeekParity.SelectionChanged += ComboBoxWeekParity_SelectionChanged;
                isLoadingConditionEditor = false;
            }
        }

        private void ButtonAddSchedule_Click(object sender, RoutedEventArgs e)
        {
            var schedule = new TimetableDefinition
            {
                Name = $"新课表 {Timetable.Schedules.Count + 1}",
                ConditionType = TimetableConditionType.Weekdays,
                Weekdays = new List<DayOfWeek> { DayOfWeek.Monday }
            };
            Timetable.Schedules.Add(schedule);
            LoadScheduleList(schedule.Id);
            LoadSelectedScheduleEditor();
            isEdited = true;
        }

        private void ButtonDuplicateSchedule_Click(object sender, RoutedEventArgs e)
        {
            var source = GetSelectedSchedule();
            if (source == null) return;
            var json = JsonConvert.SerializeObject(source);
            var copy = JsonConvert.DeserializeObject<TimetableDefinition>(json);
            copy.Id = Guid.NewGuid().ToString("N");
            copy.Name = source.Name + " 副本";
            int targetIndex = Math.Max(0, ComboBoxSchedule.SelectedIndex + 1);
            Timetable.Schedules.Insert(targetIndex, copy);
            LoadScheduleList(copy.Id);
            LoadSelectedScheduleEditor();
            isEdited = true;
        }

        private void ButtonDeleteSchedule_Click(object sender, RoutedEventArgs e)
        {
            if (Timetable.Schedules.Count <= 1) return;
            int index = ComboBoxSchedule.SelectedIndex;
            if (index < 0) return;
            if (MessageBox.Show("确定删除当前课表吗？", "删除课表", MessageBoxButton.OKCancel) != MessageBoxResult.OK) return;
            Timetable.Schedules.RemoveAt(index);
            string nextId = Timetable.Schedules[Math.Min(index, Timetable.Schedules.Count - 1)].Id;
            LoadScheduleList(nextId);
            LoadSelectedScheduleEditor();
            isEdited = true;
        }

        private void ButtonMoveScheduleUp_Click(object sender, RoutedEventArgs e)
        {
            MoveSchedule(-1);
        }

        private void ButtonMoveScheduleDown_Click(object sender, RoutedEventArgs e)
        {
            MoveSchedule(1);
        }

        private void MoveSchedule(int offset)
        {
            int index = ComboBoxSchedule.SelectedIndex;
            int target = index + offset;
            if (index < 0 || target < 0 || target >= Timetable.Schedules.Count) return;
            var schedule = Timetable.Schedules[index];
            Timetable.Schedules.RemoveAt(index);
            Timetable.Schedules.Insert(target, schedule);
            LoadScheduleList(schedule.Id);
            LoadSelectedScheduleEditor();
            isEdited = true;
        }

        private void AutoSelectTimetableToEdit()
        {

            switch (MainWindow.timetableToShow_index)
            {
                case 1: // 周一
                    ComboBoxDay.SelectedIndex = 0;
                    break;
                case 2: // 周二
                    ComboBoxDay.SelectedIndex = 1;
                    break;
                case 3: // 周三
                    ComboBoxDay.SelectedIndex = 2;
                    break;
                case 4: // 周四
                    ComboBoxDay.SelectedIndex = 3;
                    break;
                case 5: // 周五
                    ComboBoxDay.SelectedIndex = 4;
                    break;
                case 6: // 周六
                    ComboBoxDay.SelectedIndex = 5;
                    break;
                case 0: // 周日
                    ComboBoxDay.SelectedIndex = 6;
                    break;
                case 7: // 临时
                    ComboBoxDay.SelectedIndex = 7;
                    break;
            }
        }

        private void SetButtonCopyIsEnabled()
        {
            if (GetSelectedDay().Count == 0)
            {
                ButtonCopy.IsEnabled = false;
            }
            else
            {
                ButtonCopy.IsEnabled = true;
            }
        }
        #endregion

        #region Import & Export
        private void ButtonExport_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new()
            {
                Title = "导出课程表",
                FileName = "Timetable.json",
                Filter = "JSON 课程表文件 (*.json)|*.json"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string text = JsonConvert.SerializeObject(Timetable, Formatting.Indented);
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, text);
                }
                catch { }
            }
        }

        private void MenuItemImportFromTimetable_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Title = "导入课程表",
                FileName = "Timetable.json",
                Filter = "JSON 课程表文件 (*.json)|*.json|所有文件 (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                if (File.Exists(openFileDialog.FileName))
                {
                    try
                    {
                        if (MessageBox.Show("这里原有的课程将被覆盖", "确定要导入吗？", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {
                            string text = File.ReadAllText(openFileDialog.FileName);
                            Timetable = JsonConvert.DeserializeObject<Timetable>(text);
                            Timetable ??= new Timetable();
                            Timetable.EnsureMigrated();
                            ComboBoxSchedule.SelectedIndex = -1;
                            LoadScheduleList();
                            LoadSelectedScheduleEditor();
                            isEdited = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        private void MenuItemImportFromCses_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Title = "导入课程表",
                FileName = "Timetable.yaml",
                Filter = "CSES 档案文件 (*.yaml;*.yml)|*.yaml;*.yml|所有文件 (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                if (File.Exists(openFileDialog.FileName))
                {
                    try
                    {
                        if (MessageBox.Show("这里原有的课程将被覆盖", "确定要导入吗？", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {
                            string text = File.ReadAllText(openFileDialog.FileName);
                            Timetable = Timetable.ConvertFromCses(text);
                            Timetable.EnsureMigrated();
                            ComboBoxSchedule.SelectedIndex = -1;
                            LoadScheduleList();
                            LoadSelectedScheduleEditor();
                            isEdited = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }
        #endregion
    }
}
