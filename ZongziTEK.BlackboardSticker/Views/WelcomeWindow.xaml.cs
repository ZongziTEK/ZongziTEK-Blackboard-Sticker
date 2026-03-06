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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ZongziTEK.BlackboardSticker.Pages.WelcomePages;

namespace ZongziTEK.BlackboardSticker.Views
{
    /// <summary>
    /// WelcomeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WelcomeWindow : Window
    {
        public WelcomeWindow()
        {
            InitializeComponent();

            MainWindow.SaveSettings();

            NavigationViewRoot.SelectedItem = NavigationViewRoot.MenuItems[0];
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BeginSplashScreenAnimation();
        }

        private int lastPageIndex = 0;
        private int currentPageIndex = 0;

        private List<Type> pages = new()
        {
            typeof(WelcomePage0),
            typeof(WelcomePage1),
            typeof(WelcomePage2),
            typeof(WelcomePage3),
            typeof(WelcomePage4)
        };

        private void NavigationViewRoot_SelectionChanged(iNKORE.UI.WPF.Modern.Controls.NavigationView sender, iNKORE.UI.WPF.Modern.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            currentPageIndex = NavigationViewRoot.MenuItems.IndexOf(NavigationViewRoot.SelectedItem);

            CheckButtonState();

            if (currentPageIndex > lastPageIndex) FrameTransitionEffect.Effect = SlideNavigationTransitionEffect.FromRight;
            else FrameTransitionEffect.Effect = SlideNavigationTransitionEffect.FromLeft;

            FrameRoot.Navigate(pages[currentPageIndex]);

            lastPageIndex = currentPageIndex;
        }

        private async void CheckButtonState()
        {
            if (currentPageIndex == 0)
            {
                HideElement(ButtonPrevious);
            }
            else
            {
                if (ButtonPrevious.Visibility != Visibility.Visible) ShowElement(ButtonPrevious);
            }
            if (currentPageIndex == pages.Count - 1)
            {
                HideElement(ButtonNext);
                await Task.Delay(250);
                ShowElement(ButtonFinish);
            }
            else
            {
                if (ButtonFinish.Visibility == Visibility.Visible)
                {
                    HideElement(ButtonFinish);
                    await Task.Delay(250);
                }
                if (ButtonNext.Visibility != Visibility.Visible) ShowElement(ButtonNext);
            }
        }

        private void SwitchToNextPage()
        {
            if (currentPageIndex < pages.Count - 1)
                NavigationViewRoot.SelectedItem = NavigationViewRoot.MenuItems[NavigationViewRoot.MenuItems.IndexOf(NavigationViewRoot.SelectedItem) + 1];
        }

        private void SwitchToPreviousPage()
        {
            if (currentPageIndex > 0)
                NavigationViewRoot.SelectedItem = NavigationViewRoot.MenuItems[NavigationViewRoot.MenuItems.IndexOf(NavigationViewRoot.SelectedItem) - 1];
        }

        private async void HideElement(UIElement element)
        {
            DoubleAnimation opacityAnimaion = new()
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.25),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseIn }
            };
            element.BeginAnimation(OpacityProperty, opacityAnimaion);

            await Task.Delay(250);

            element.Visibility = Visibility.Collapsed;
        }

        private void ShowElement(UIElement element)
        {
            element.Visibility = Visibility.Visible;

            DoubleAnimation opacityAnimaion = new()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.25),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };
            element.BeginAnimation(OpacityProperty, opacityAnimaion);
        }

        private async void BeginSplashScreenAnimation()
        {
            DoubleAnimation opacityAnimationIn = new()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(1000),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };

            ThicknessAnimation marginAnimationIn = new()
            {
                From = new Thickness(200, 288, 200, 112),
                To = new Thickness(200),
                Duration = TimeSpan.FromMilliseconds(1000),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };

            DoubleAnimation opacityAnimationOut = new()
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };

            ThicknessAnimation marginAnimationOut = new()
            {
                From = new Thickness(200),
                To = new Thickness(144),
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };

            ViewboxSplashScreen.BeginAnimation(OpacityProperty, opacityAnimationIn);
            ViewboxSplashScreen.BeginAnimation(MarginProperty, marginAnimationIn);

            await Task.Delay(2000);

            ViewboxSplashScreen.BeginAnimation(MarginProperty, marginAnimationOut);
            GridSplashScreen.BeginAnimation(OpacityProperty, opacityAnimationOut);

            await Task.Delay(500);

            GridSplashScreen.Visibility = Visibility.Collapsed;
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            SwitchToNextPage();
        }

        private void ButtonPrevious_Click(object sender, RoutedEventArgs e)
        {
            SwitchToPreviousPage();
        }

        private void ButtonFinish_Click(object sender, RoutedEventArgs e)
        {
            if (currentPageIndex == pages.Count - 1) Close();
        }
    }
}
