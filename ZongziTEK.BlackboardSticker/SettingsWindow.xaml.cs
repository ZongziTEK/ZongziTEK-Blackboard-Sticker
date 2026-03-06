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
using System.Windows.Shapes;
using ZongziTEK.BlackboardSticker.Pages.SettingsPages;

namespace ZongziTEK.BlackboardSticker
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

            NavigationViewRoot.SelectedItem = NavigationViewRoot.MenuItems[0];
        }

        private List<Type> pages = new()
        {
            typeof(BehaviorSettingsPage),
            typeof(LookSettingsPage),
            typeof(StorageSettingsPage),
            typeof(TimetableSettingsPage),
            typeof(InfoBoardSettingsPage),
            typeof(AutomationSettingsPage),
            typeof(InteractionsSettingsPage),
            typeof(AboutPage)
        };

        private void NavigationViewRoot_SelectionChanged(iNKORE.UI.WPF.Modern.Controls.NavigationView sender, iNKORE.UI.WPF.Modern.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            int currentPageIndex = NavigationViewRoot.MenuItems.IndexOf(NavigationViewRoot.SelectedItem);
            if (currentPageIndex == -1) currentPageIndex = 6;

            FrameRoot.Navigate(pages[currentPageIndex]);
        }
    }
}
