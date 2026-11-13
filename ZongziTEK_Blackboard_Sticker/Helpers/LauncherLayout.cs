using System.Windows;
using System.Windows.Controls;

namespace ZongziTEK_Blackboard_Sticker.Helpers
{
    internal static class LauncherLayout
    {
        public static void Apply(UIElement timetable, UIElement launcher, bool showLauncher)
        {
            launcher.Visibility = showLauncher ? Visibility.Visible : Visibility.Collapsed;
            Grid.SetRowSpan(timetable, showLauncher ? 1 : 2);
        }
    }
}
