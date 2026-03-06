using iNKORE.UI.WPF.Modern.Media.Animation;
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
using ZongziTEK.BlackboardSticker.Pages.WelcomePages;
using ZongziTEK.BlackboardSticker.Pages.SettingsPages.InfoBoardSettingsPages;

namespace ZongziTEK.BlackboardSticker.Pages.SettingsPages
{
    /// <summary>
    /// InfoBoardSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class InfoBoardSettingsPage : Page
    {
        public InfoBoardSettingsPage()
        {
            InitializeComponent();

            NavigationViewRoot.SelectedItem = NavigationViewRoot.MenuItems[0];
        }

        private int lastPageIndex = 0;

        private List<Type> pages = new()
        {
            typeof(InfoBoardGenericSettingsPage),
            typeof(CountdownSettingsPage),
            typeof(WeatherSettingsPage)
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
