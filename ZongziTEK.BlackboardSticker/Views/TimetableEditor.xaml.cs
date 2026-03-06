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
using ZongziTEK.BlackboardSticker.Services;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace ZongziTEK.BlackboardSticker.Views
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

            AutoSelectTimetableToEdit();
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

        private async void LoadTimetable()
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

                    await Task.Delay(25);
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
            switch (ComboBoxDay.SelectedIndex)
            {
                case 0:
                    return Timetable.Monday;
                case 1:
                    return Timetable.Tuesday;
                case 2:
                    return Timetable.Wednesday;
                case 3:
                    return Timetable.Thursday;
                case 4:
                    return Timetable.Friday;
                case 5:
                    return Timetable.Saturday;
                case 6:
                    return Timetable.Sunday;
                case 7:
                    return Timetable.Temp;
            }
            return Timetable.Monday;
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
                            LoadTimetable();
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
                            LoadTimetable();
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
