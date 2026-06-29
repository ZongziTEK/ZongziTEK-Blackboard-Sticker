using iNKORE.UI.WPF.Modern.Common.IconKeys;
using System.Windows;
using System.Windows.Controls;

namespace ZongziTEK_Blackboard_Sticker.Controls.Cards
{
    /// <summary>
    /// SliderCard.xaml 的交互逻辑
    /// </summary>
    public partial class SliderCard : UserControl
    {
        public SliderCard()
        {
            InitializeComponent();
        }

        public void SetValue(double value)
        {
            Value = value;
            if (MainEditor != null)
            {
                MainEditor.Value = value;
            }
        }

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(SliderCard), new PropertyMetadata(""));

        public string Tip
        {
            get { return (string)GetValue(TipProperty); }
            set { SetValue(TipProperty, value); }
        }

        public static readonly DependencyProperty TipProperty =
            DependencyProperty.Register("Tip", typeof(string), typeof(SliderCard), new PropertyMetadata(""));

        public FontIconData Icon
        {
            get { return (FontIconData)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(FontIconData), typeof(SliderCard), new PropertyMetadata(FluentSystemIcons.EmojiLaugh_20_Regular));

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(SliderCard), new PropertyMetadata((double)0));

        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(SliderCard), new PropertyMetadata((double)0));

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(SliderCard), new PropertyMetadata((double)1));

        public double TickFrequency
        {
            get { return (double)GetValue(TickFrequencyProperty); }
            set { SetValue(TickFrequencyProperty, value); }
        }

        public static readonly DependencyProperty TickFrequencyProperty =
            DependencyProperty.Register("TickFrequency", typeof(double), typeof(SliderCard), new PropertyMetadata(0.1));

        public string Unit
        {
            get { return (string)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }

        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register("Unit", typeof(string), typeof(SliderCard), new PropertyMetadata(""));

        public string NumberFormat
        {
            get { return (string)GetValue(NumberFormatProperty); }
            set { SetValue(NumberFormatProperty, value); }
        }

        public static readonly DependencyProperty NumberFormatProperty =
            DependencyProperty.Register("NumberFormat", typeof(string), typeof(SliderCard), new PropertyMetadata("0.##"));

        public double InputWidth
        {
            get { return (double)GetValue(InputWidthProperty); }
            set { SetValue(InputWidthProperty, value); }
        }

        public static readonly DependencyProperty InputWidthProperty =
            DependencyProperty.Register("InputWidth", typeof(double), typeof(SliderCard), new PropertyMetadata((double)80));

        public bool IsResetButtonVisible
        {
            get { return (bool)GetValue(IsResetButtonVisibleProperty); }
            set { SetValue(IsResetButtonVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsResetButtonVisibleProperty =
            DependencyProperty.Register("IsResetButtonVisible", typeof(bool), typeof(SliderCard), new PropertyMetadata(false));

        public bool IsResetEnabled
        {
            get { return (bool)GetValue(IsResetEnabledProperty); }
            set { SetValue(IsResetEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsResetEnabledProperty =
            DependencyProperty.Register("IsResetEnabled", typeof(bool), typeof(SliderCard), new PropertyMetadata(true));

        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SliderCard));

        public event RoutedEventHandler ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        private void MainEditor_ValueChanged(object sender, RoutedEventArgs e)
        {
            Value = MainEditor.Value;
            RaiseEvent(new RoutedEventArgs(ValueChangedEvent, this));
        }

        public static readonly RoutedEvent ValueChangeStartEvent = EventManager.RegisterRoutedEvent("ValueChangeStart", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SliderCard));

        public event RoutedEventHandler ValueChangeStart
        {
            add { AddHandler(ValueChangeStartEvent, value); }
            remove { RemoveHandler(ValueChangeStartEvent, value); }
        }

        private void MainEditor_ValueChangeStart(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ValueChangeStartEvent, this));
        }

        public static readonly RoutedEvent ValueChangeEndEvent = EventManager.RegisterRoutedEvent("ValueChangeEnd", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SliderCard));

        public event RoutedEventHandler ValueChangeEnd
        {
            add { AddHandler(ValueChangeEndEvent, value); }
            remove { RemoveHandler(ValueChangeEndEvent, value); }
        }

        private void MainEditor_ValueChangeEnd(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ValueChangeEndEvent, this));
        }

        public static readonly RoutedEvent ResetClickedEvent = EventManager.RegisterRoutedEvent("ResetClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SliderCard));

        public event RoutedEventHandler ResetClicked
        {
            add { AddHandler(ResetClickedEvent, value); }
            remove { RemoveHandler(ResetClickedEvent, value); }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ResetClickedEvent, this));
        }
    }
}
