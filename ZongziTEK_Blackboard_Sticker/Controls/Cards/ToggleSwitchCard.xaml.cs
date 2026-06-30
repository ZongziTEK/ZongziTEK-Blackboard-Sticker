using iNKORE.UI.WPF.Modern.Common.IconKeys;
using iNKORE.UI.WPF.Modern.Controls;
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

namespace ZongziTEK_Blackboard_Sticker.Controls.Cards
{
    /// <summary>
    /// ToggleSwitchCard.xaml 的交互逻辑
    /// </summary>
    public partial class ToggleSwitchCard : UserControl
    {
        public ToggleSwitchCard()
        {
            InitializeComponent();
        }

        public bool IsOn
        {
            get { return (bool)GetValue(IsOnProperty); }
            set { SetValue(IsOnProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsOn.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsOnProperty =
            DependencyProperty.Register("IsOn", typeof(bool), typeof(ToggleSwitchCard), new PropertyMetadata(false));


        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(ToggleSwitchCard), new PropertyMetadata(""));


        public string Tip
        {
            get { return (string)GetValue(TipProperty); }
            set { SetValue(TipProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Tip.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TipProperty =
            DependencyProperty.Register("Tip", typeof(string), typeof(ToggleSwitchCard), new PropertyMetadata(""));


        public string OnContent
        {
            get { return (string)GetValue(OnContentProperty); }
            set { SetValue(OnContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OnContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OnContentProperty =
            DependencyProperty.Register("OnContent", typeof(string), typeof(ToggleSwitchCard), new PropertyMetadata("开"));


        public string OffContent
        {
            get { return (string)GetValue(OffContentProperty); }
            set { SetValue(OffContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OffContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OffContentProperty =
            DependencyProperty.Register("OffContent", typeof(string), typeof(ToggleSwitchCard), new PropertyMetadata("关"));


        public FontIconData Icon
        {
            get { return (FontIconData)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(FontIconData), typeof(ToggleSwitchCard), new PropertyMetadata(FluentSystemIcons.EmojiLaugh_20_Regular));

        public bool IsResetButtonVisible
        {
            get { return (bool)GetValue(IsResetButtonVisibleProperty); }
            set { SetValue(IsResetButtonVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsResetButtonVisibleProperty =
            DependencyProperty.Register("IsResetButtonVisible", typeof(bool), typeof(ToggleSwitchCard), new PropertyMetadata(false));

        public bool IsResetEnabled
        {
            get { return (bool)GetValue(IsResetEnabledProperty); }
            set { SetValue(IsResetEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsResetEnabledProperty =
            DependencyProperty.Register("IsResetEnabled", typeof(bool), typeof(ToggleSwitchCard), new PropertyMetadata(true));


        // Toggled Event
        public static readonly RoutedEvent ToggledEvent = EventManager.RegisterRoutedEvent("Toggled", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ToggleSwitchCard));

        public event RoutedEventHandler Toggled
        {
            add { AddHandler(ToggledEvent, value); }
            remove { RemoveHandler(ToggledEvent, value); }
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (SettingsResetItem.IsResetting) return;

            RoutedEventArgs routedEventArgs = new RoutedEventArgs(ToggledEvent, this);
            RaiseEvent(routedEventArgs);
        }

        public static readonly RoutedEvent ResetClickedEvent = EventManager.RegisterRoutedEvent("ResetClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ToggleSwitchCard));

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
