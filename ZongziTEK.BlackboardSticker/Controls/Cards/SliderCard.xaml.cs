using iNKORE.UI.WPF.Modern.Common.IconKeys;
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

namespace ZongziTEK.BlackboardSticker.Controls.Cards
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

        bool isLoaded = false;

        private void SliderCard_Loaded(object sender, RoutedEventArgs e)
        {
            MainSlider.Minimum = Minimum;
            MainSlider.Maximum = Maximum;
            MainSlider.TickFrequency = TickFrequency;
            MainSlider.Value = Value;

            isLoaded = true;
        }

        public void SetValue(double value)
        {
            Value = value;
            MainSlider.Value = value;
        }

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(SliderCard), new PropertyMetadata(""));


        public string Tip
        {
            get { return (string)GetValue(TipProperty); }
            set { SetValue(TipProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Tip.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TipProperty =
            DependencyProperty.Register("Tip", typeof(string), typeof(SliderCard), new PropertyMetadata(""));

        public FontIconData Icon
        {
            get { return (FontIconData)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(FontIconData), typeof(SliderCard), new PropertyMetadata(FluentSystemIcons.EmojiLaugh_20_Regular));

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(SliderCard), new PropertyMetadata((double)0));

        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Minimum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(SliderCard), new PropertyMetadata((double)0));

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Maximum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(SliderCard), new PropertyMetadata((double)1));

        public double TickFrequency
        {
            get { return (double)GetValue(TickFrequencyProperty); }
            set { SetValue(TickFrequencyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TickFrequency.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TickFrequencyProperty =
            DependencyProperty.Register("TickFrequency", typeof(double), typeof(SliderCard), new PropertyMetadata(0.1));


        // ValueChanged Event
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SliderCard));

        public event RoutedEventHandler ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        private void MainSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isLoaded) return;
            
            Value = MainSlider.Value;

            RoutedEventArgs routedEventArgs = new RoutedEventArgs(ValueChangedEvent, this);
            RaiseEvent(routedEventArgs);
        }

        // ValueChangeStart Event
        public static readonly RoutedEvent ValueChangeStartEvent = EventManager.RegisterRoutedEvent("ValueChangeStart", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SliderCard));

        public event RoutedEventHandler ValueChangeStart
        {
            add { AddHandler(ValueChangeStartEvent, value); }
            remove { RemoveHandler(ValueChangeStartEvent, value); }
        }

        private void MainSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            RoutedEventArgs routedEventArgs = new RoutedEventArgs(ValueChangeStartEvent, this);
            RaiseEvent(routedEventArgs);
        }

        // ValueChangeEnd Event
        public static readonly RoutedEvent ValueChangeEndEvent = EventManager.RegisterRoutedEvent("ValueChangeEnd", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SliderCard));

        public event RoutedEventHandler ValueChangeEnd
        {
            add { AddHandler(ValueChangeEndEvent, value); }
            remove { RemoveHandler(ValueChangeEndEvent, value); }
        }

        private void MainSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            RoutedEventArgs routedEventArgs = new RoutedEventArgs(ValueChangeEndEvent, this);
            RaiseEvent(routedEventArgs);
        }
    }
}
