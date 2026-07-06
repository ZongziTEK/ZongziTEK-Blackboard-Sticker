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
using ZongziTEK_Blackboard_Sticker.Helpers;

namespace ZongziTEK_Blackboard_Sticker
{
    /// <summary>
    /// TimetableLesson.xaml 的交互逻辑
    /// </summary>
    public partial class TimetableLesson : UserControl
    {
        private bool isCustomCardBackgroundApplied = false;
        private Brush customCardBackground;
        private object customCardBorderBrushResourceKey;
        private Thickness customCardBorderThickness;

        public TimetableLesson()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyCardBackgroundVisibility();

            if (IsActive) Activate();
            else Deactivate();
        }

        private void ApplyCardBackgroundVisibility()
        {
            if (BorderLessonCard == null) return;

            if (isCustomCardBackgroundApplied)
            {
                BorderLessonCard.Background = customCardBackground ?? Brushes.Transparent;
                if (customCardBorderBrushResourceKey != null)
                {
                    BorderLessonCard.SetResourceReference(Border.BorderBrushProperty, customCardBorderBrushResourceKey);
                }
                else
                {
                    BorderLessonCard.BorderBrush = Brushes.Transparent;
                }
                BorderLessonCard.BorderThickness = customCardBorderThickness;
                return;
            }

            if (IsCardBackgroundVisible)
            {
                BorderLessonCard.SetResourceReference(Border.BackgroundProperty, ThemeKeys.CardBackgroundFillColorDefaultBrushKey);
                BorderLessonCard.SetResourceReference(Border.BorderBrushProperty, ThemeKeys.CardStrokeColorDefaultBrushKey);
                BorderLessonCard.BorderThickness = new Thickness(1);
            }
            else
            {
                BorderLessonCard.Background = Brushes.Transparent;
                BorderLessonCard.BorderBrush = Brushes.Transparent;
                BorderLessonCard.BorderThickness = new Thickness(0);
            }
        }

        public void RestoreCardBackground()
        {
            isCustomCardBackgroundApplied = false;
            customCardBackground = null;
            customCardBorderBrushResourceKey = null;
            customCardBorderThickness = new Thickness(0);
            if (IsCardBackgroundVisible) ApplyCardBackgroundVisibility();
            else IsCardBackgroundVisible = true;
        }

        public void MakeCardBackgroundTransparent()
        {
            isCustomCardBackgroundApplied = false;
            customCardBackground = null;
            customCardBorderBrushResourceKey = null;
            customCardBorderThickness = new Thickness(0);
            if (!IsCardBackgroundVisible) ApplyCardBackgroundVisibility();
            else IsCardBackgroundVisible = false;
        }

        public void ApplyCustomCardBackground(Brush background, object borderBrushResourceKey, Thickness borderThickness)
        {
            isCustomCardBackgroundApplied = true;
            customCardBackground = background;
            customCardBorderBrushResourceKey = borderBrushResourceKey;
            customCardBorderThickness = borderThickness;
            ApplyCardBackgroundVisibility();
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

        public bool IsCardBackgroundVisible
        {
            get { return (bool)GetValue(IsCardBackgroundVisibleProperty); }
            set { SetValue(IsCardBackgroundVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsCardBackgroundVisibleProperty =
            DependencyProperty.Register("IsCardBackgroundVisible", typeof(bool), typeof(TimetableLesson),
                new PropertyMetadata(true, OnIsCardBackgroundVisiblePropertyChanged));

        private static void OnIsCardBackgroundVisiblePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TimetableLesson)d).ApplyCardBackgroundVisibility();
        }
    }
}
