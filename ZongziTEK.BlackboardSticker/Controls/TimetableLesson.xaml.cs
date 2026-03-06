using iNKORE.UI.WPF.Modern;
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
using ZongziTEK.BlackboardSticker.Helpers;

namespace ZongziTEK.BlackboardSticker
{
    /// <summary>
    /// TimetableLesson.xaml 的交互逻辑
    /// </summary>
    public partial class TimetableLesson : UserControl
    {
        public TimetableLesson()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsActive) Activate();
            else Deactivate();
        }

        public void Activate()
        {
            ControlsHelper.SetDynamicResource(BorderActiveIndicator, Border.BackgroundProperty, ThemeKeys.AccentButtonBackgroundKey);
            if (!IsActive) IsActive = true;
        }

        public void Deactivate()
        {
            BorderActiveIndicator.Background = new SolidColorBrush(Color.FromArgb(153, 85, 85, 85));
            if (IsActive) IsActive = false;
        }

        public string Subject
        {
            get { return (string)GetValue(SubjectProperty); }
            set { SetValue(SubjectProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Subject.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SubjectProperty =
            DependencyProperty.Register("Subject", typeof(string), typeof(TimetableLesson), new PropertyMetadata(""));

        public string Time
        {
            get { return (string)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Time.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register("Time", typeof(string), typeof(TimetableLesson), new PropertyMetadata(""));

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsActive.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(TimetableLesson), new PropertyMetadata(false));
    }
}
