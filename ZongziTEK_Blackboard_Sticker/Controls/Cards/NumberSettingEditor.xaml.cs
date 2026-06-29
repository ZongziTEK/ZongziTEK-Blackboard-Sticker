using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ZongziTEK_Blackboard_Sticker.Controls.Cards
{
    /// <summary>
    /// NumberSettingEditor.xaml 的交互逻辑
    /// </summary>
    public partial class NumberSettingEditor : UserControl
    {
        private bool isLoaded = false;
        private bool isUpdating = false;

        public NumberSettingEditor()
        {
            InitializeComponent();
        }

        private void NumberSettingEditor_Loaded(object sender, RoutedEventArgs e)
        {
            isLoaded = true;
            UpdateSliderRange();
            UpdateInterfaceFromValue();
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(NumberSettingEditor),
                new FrameworkPropertyMetadata((double)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValuePropertyChanged));

        private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumberSettingEditor editor = (NumberSettingEditor)d;
            if (editor.isUpdating) return;

            editor.UpdateInterfaceFromValue();
        }

        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(NumberSettingEditor),
                new PropertyMetadata((double)0, OnRangePropertyChanged));

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(NumberSettingEditor),
                new PropertyMetadata((double)1, OnRangePropertyChanged));

        public double TickFrequency
        {
            get { return (double)GetValue(TickFrequencyProperty); }
            set { SetValue(TickFrequencyProperty, value); }
        }

        public static readonly DependencyProperty TickFrequencyProperty =
            DependencyProperty.Register("TickFrequency", typeof(double), typeof(NumberSettingEditor),
                new PropertyMetadata(0.1, OnRangePropertyChanged));

        private static void OnRangePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumberSettingEditor editor = (NumberSettingEditor)d;
            editor.UpdateSliderRange();
            editor.UpdateInterfaceFromValue();
        }

        public string Unit
        {
            get { return (string)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }

        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register("Unit", typeof(string), typeof(NumberSettingEditor), new PropertyMetadata(""));

        public string NumberFormat
        {
            get { return (string)GetValue(NumberFormatProperty); }
            set { SetValue(NumberFormatProperty, value); }
        }

        public static readonly DependencyProperty NumberFormatProperty =
            DependencyProperty.Register("NumberFormat", typeof(string), typeof(NumberSettingEditor),
                new PropertyMetadata("0.##", OnNumberFormatPropertyChanged));

        private static void OnNumberFormatPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((NumberSettingEditor)d).UpdateInterfaceFromValue();
        }

        public double InputWidth
        {
            get { return (double)GetValue(InputWidthProperty); }
            set { SetValue(InputWidthProperty, value); }
        }

        public static readonly DependencyProperty InputWidthProperty =
            DependencyProperty.Register("InputWidth", typeof(double), typeof(NumberSettingEditor), new PropertyMetadata((double)80));

        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NumberSettingEditor));

        public event RoutedEventHandler ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        public static readonly RoutedEvent ValueChangeStartEvent = EventManager.RegisterRoutedEvent("ValueChangeStart", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NumberSettingEditor));

        public event RoutedEventHandler ValueChangeStart
        {
            add { AddHandler(ValueChangeStartEvent, value); }
            remove { RemoveHandler(ValueChangeStartEvent, value); }
        }

        public static readonly RoutedEvent ValueChangeEndEvent = EventManager.RegisterRoutedEvent("ValueChangeEnd", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NumberSettingEditor));

        public event RoutedEventHandler ValueChangeEnd
        {
            add { AddHandler(ValueChangeEndEvent, value); }
            remove { RemoveHandler(ValueChangeEndEvent, value); }
        }

        private void ValueTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            CommitTextValue();
        }

        private void ValueTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            CommitTextValue();
            Keyboard.ClearFocus();
            e.Handled = true;
        }

        private void MainSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isLoaded || isUpdating) return;

            ApplyValue(MainSlider.Value, true);
        }

        private void MainSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ValueChangeStartEvent, this));
        }

        private void MainSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ValueChangeEndEvent, this));
        }

        private void MainSlider_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ValueChangeStartEvent, this));
        }

        private void MainSlider_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ValueChangeEndEvent, this));
        }

        private void CommitTextValue()
        {
            if (isUpdating) return;

            if (!TryParseNumber(ValueTextBox.Text, out double parsedValue))
            {
                UpdateTextBox(Value);
                return;
            }

            ApplyValue(parsedValue, true);
        }

        private void ApplyValue(double value, bool raiseChanged)
        {
            double normalizedValue = NormalizeValue(value);
            bool hasChanged = !AreClose(Value, normalizedValue);

            isUpdating = true;
            SetCurrentValue(ValueProperty, normalizedValue);
            isUpdating = false;

            UpdateInterfaceFromValue();

            if (raiseChanged && hasChanged)
            {
                RaiseEvent(new RoutedEventArgs(ValueChangedEvent, this));
            }
        }

        private double NormalizeValue(double value)
        {
            double minimum = Math.Min(Minimum, Maximum);
            double maximum = Math.Max(Minimum, Maximum);
            double result = Math.Max(minimum, Math.Min(maximum, value));

            if (TickFrequency > 0)
            {
                double steps = Math.Round((result - minimum) / TickFrequency, MidpointRounding.AwayFromZero);
                result = minimum + steps * TickFrequency;
                result = Math.Max(minimum, Math.Min(maximum, result));
            }

            return Math.Round(result, 10);
        }

        private void UpdateSliderRange()
        {
            if (MainSlider == null) return;

            MainSlider.Minimum = Math.Min(Minimum, Maximum);
            MainSlider.Maximum = Math.Max(Minimum, Maximum);
            MainSlider.TickFrequency = TickFrequency;
        }

        private void UpdateInterfaceFromValue()
        {
            if (!isLoaded) return;

            isUpdating = true;
            UpdateSliderRange();
            MainSlider.Value = Math.Max(MainSlider.Minimum, Math.Min(MainSlider.Maximum, Value));
            SliderValueTextBlock.Text = FormatValue(MainSlider.Value);

            if (!ValueTextBox.IsKeyboardFocusWithin)
            {
                UpdateTextBox(Value);
            }

            isUpdating = false;
        }

        private void UpdateTextBox(double value)
        {
            ValueTextBox.Text = FormatValue(value);
        }

        private string FormatValue(double value)
        {
            string format = string.IsNullOrWhiteSpace(NumberFormat) ? "0.##" : NumberFormat;
            return value.ToString(format, CultureInfo.CurrentCulture);
        }

        private bool TryParseNumber(string text, out double value)
        {
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
            {
                return true;
            }

            return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        private static bool AreClose(double a, double b)
        {
            return Math.Abs(a - b) < 0.0000001;
        }
    }
}
