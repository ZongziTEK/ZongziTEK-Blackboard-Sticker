using System.Windows;
using System.Windows.Controls;
using ZongziTEK_Blackboard_Sticker.Models;

namespace ZongziTEK_Blackboard_Sticker.Controls
{
    public partial class DutyPanel : UserControl
    {
        public DutyPanel() => InitializeComponent();

        public event RoutedEventHandler EditRequested;

        public void Present(DutyPresentation presentation, string error)
        {
            Heading.Text = "值日生 · " + presentation.Title;
            Assignments.ItemsSource = error == null ? presentation.Rows : null;
            EmptyState.Text = error ?? presentation.EmptyText;
            EmptyState.Visibility = error == null && presentation.Rows.Count != 0
                ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Edit_Click(object sender, RoutedEventArgs e) => EditRequested?.Invoke(this, e);
    }
}
