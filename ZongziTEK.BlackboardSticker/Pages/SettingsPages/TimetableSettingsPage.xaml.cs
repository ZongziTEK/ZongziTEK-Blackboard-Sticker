using iNKORE.UI.WPF.Modern.Media.Animation;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using ZongziTEK.BlackboardSticker.Pages.SettingsPages.TimetableSettingsPages;

namespace ZongziTEK.BlackboardSticker.Pages.SettingsPages
{
    /// <summary>
    /// TimetableSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class TimetableSettingsPage : Page
    {
        public TimetableSettingsPage()
        {
            InitializeComponent();

            NavigationViewRoot.SelectedItem = NavigationViewRoot.MenuItems[0];
        }

        private int lastPageIndex = 0;

        private List<Type> pages = new()
        {
            typeof(TimetableGenericSettingsPage),
            typeof(TimetableSpeechSettingsPage)
        };

        private void NavigationViewRoot_SelectionChanged(iNKORE.UI.WPF.Modern.Controls.NavigationView sender, iNKORE.UI.WPF.Modern.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            int currentPageIndex = NavigationViewRoot.MenuItems.IndexOf(NavigationViewRoot.SelectedItem);

            if (currentPageIndex > lastPageIndex) FrameTransitionEffect.Effect = SlideNavigationTransitionEffect.FromRight;
            else FrameTransitionEffect.Effect = SlideNavigationTransitionEffect.FromLeft;

            FrameRoot.Navigate(pages[currentPageIndex]);

            lastPageIndex = currentPageIndex;
        }
    }
}
