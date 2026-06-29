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

namespace ZongziTEK_Blackboard_Sticker.Controls.Cards
{
    /// <summary>
    /// GenericSettingsCard.xaml 的交互逻辑
    /// </summary>
    public partial class GenericSettingsCard : UserControl
    {
        public GenericSettingsCard()
        {
            InitializeComponent();
        }

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(GenericSettingsCard), new PropertyMetadata(""));


        public string Tip
        {
            get { return (string)GetValue(TipProperty); }
            set { SetValue(TipProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Tip.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TipProperty =
            DependencyProperty.Register("Tip", typeof(string), typeof(GenericSettingsCard), new PropertyMetadata(""));

        public FontIconData Icon
        {
            get { return (FontIconData)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(FontIconData), typeof(GenericSettingsCard), new PropertyMetadata(FluentSystemIcons.EmojiLaugh_20_Regular));

        public UIElement CardContent
        {
            get { return (UIElement)GetValue(CardContentProperty); }
            set { SetValue(CardContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CardContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CardContentProperty =
            DependencyProperty.Register("CardContent", typeof(UIElement), typeof(GenericSettingsCard));

        public bool IsResetButtonVisible
        {
            get { return (bool)GetValue(IsResetButtonVisibleProperty); }
            set { SetValue(IsResetButtonVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsResetButtonVisibleProperty =
            DependencyProperty.Register("IsResetButtonVisible", typeof(bool), typeof(GenericSettingsCard), new PropertyMetadata(false));

        public bool IsResetEnabled
        {
            get { return (bool)GetValue(IsResetEnabledProperty); }
            set { SetValue(IsResetEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsResetEnabledProperty =
            DependencyProperty.Register("IsResetEnabled", typeof(bool), typeof(GenericSettingsCard), new PropertyMetadata(true));

        public static readonly RoutedEvent ResetClickedEvent = EventManager.RegisterRoutedEvent("ResetClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(GenericSettingsCard));

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
