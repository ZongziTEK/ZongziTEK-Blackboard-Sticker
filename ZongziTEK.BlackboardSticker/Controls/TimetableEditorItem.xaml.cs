using System;
using System.Collections.Generic;
using System.Configuration;
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

namespace ZongziTEK.BlackboardSticker
{
    /// <summary>
    /// TimetableEditorItem.xaml 的交互逻辑
    /// </summary>
    public partial class TimetableEditorItem : UserControl
    {
        public TimetableEditorItem()
        {
            InitializeComponent();

            if (TextBoxSubject.Text == "")
            {
                TextBlockHintSubject.Visibility = Visibility.Visible;
            }
            else
            {
                TextBlockHintSubject.Visibility = Visibility.Hidden;
            }
        }

        public event EventHandler LessonInfoChanged;
        public event EventHandler LessonDeleting;
        private bool isLoaded = false;

        #region Properties
        public static readonly DependencyProperty SubjectProperty =
        DependencyProperty.Register("Subject", typeof(string), typeof(TimetableEditorItem), new PropertyMetadata(""));

        public string Subject
        {
            get { return (string)GetValue(SubjectProperty); }
            set { SetValue(SubjectProperty, value); }
        }

        public static readonly DependencyProperty StartTimeProperty =
            DependencyProperty.Register("StartTime", typeof(string), typeof(TimetableEditorItem), new PropertyMetadata("00:00"));

        public string StartTime
        {
            get { return (string)GetValue(StartTimeProperty); }
            set { SetValue(StartTimeProperty, value); }
        }

        public static readonly DependencyProperty EndTimeProperty =
            DependencyProperty.Register("EndTime", typeof(string), typeof(TimetableEditorItem), new PropertyMetadata("00:00"));

        public string EndTime
        {
            get { return (string)GetValue(EndTimeProperty); }
            set { SetValue(EndTimeProperty, value); }
        }

        public static readonly DependencyProperty IsSplitBelowProperty =
            DependencyProperty.Register("IsSplitBelow", typeof(bool), typeof(TimetableEditorItem), new PropertyMetadata(false));

        public bool IsSplitBelow
        {
            get { return (bool)GetValue(IsSplitBelowProperty); }
            set { SetValue(IsSplitBelowProperty, value); }
        }

        public static readonly DependencyProperty IsStrongClassOverNotificationEnabledProperty =
            DependencyProperty.Register("IsStrongClassOverNotificationEnabled", typeof(bool), typeof(TimetableEditorItem), new PropertyMetadata(false));

        public bool IsStrongClassOverNotificationEnabled
        {
            get { return (bool)GetValue(IsStrongClassOverNotificationEnabledProperty); }
            set { SetValue(IsStrongClassOverNotificationEnabledProperty, value); }
        }
        #endregion

        #region TextBlockHint
        private void TextBoxSubject_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBlockHintSubject.Visibility = Visibility.Hidden;
        }

        private void TextBoxSubject_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TextBoxSubject.Text == "")
            {
                TextBlockHintSubject.Visibility = Visibility.Visible;
            }
        }
        #endregion

        #region Events
        private void OnLessonInfoChanged()
        {
            EventHandler handler = LessonInfoChanged;
            handler?.Invoke(this, EventArgs.Empty);
        }

        private void OnLessonDeleting()
        {
            EventHandler handler = LessonDeleting;
            handler?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Controls

        private void TextBoxSubject_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBoxSubject.Text == "")
            {
                TextBlockHintSubject.Visibility = Visibility.Visible;
            }
            else
            {
                TextBlockHintSubject.Visibility = Visibility.Hidden;
            }

            if (!isLoaded) return;

            Subject = TextBoxSubject.Text;
            OnLessonInfoChanged();
        }

        private void StartTimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isLoaded) return;

            StartTime = StartTimeTextBox.Text;
            OnLessonInfoChanged();
        }

        private void EndTimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isLoaded) return;

            EndTime = EndTimeTextBox.Text;
            OnLessonInfoChanged();
        }

        private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            OnLessonDeleting();
        }

        private void MenuItemSplit_Click(object sender, RoutedEventArgs e)
        {
            IsSplitBelow = MenuItemSplit.IsChecked;
            OnLessonInfoChanged();
        }

        private void MenuItemStrongNotification_Click(object sender, RoutedEventArgs e)
        {
            IsStrongClassOverNotificationEnabled = MenuItemStrongNotification.IsChecked;
            OnLessonInfoChanged();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            MenuItemSplit.IsChecked = IsSplitBelow;
            MenuItemStrongNotification.IsChecked = IsStrongClassOverNotificationEnabled;
            isLoaded = true;
        }
        #endregion
    }
}
