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

namespace ZongziTEK.BlackboardSticker.Pages.SettingsPages.InfoBoardSettingsPages
{
    /// <summary>
    /// CountdownSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class CountdownSettingsPage : Page
    {
        public CountdownSettingsPage()
        {
            InitializeComponent();

            DataContext = MainWindow.Settings.InfoBoard;
        }

        private void TextBoxName_TextChanged(object sender, RoutedEventArgs e)
        {
            MainWindow.SaveSettings();
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            MainWindow.SaveSettings();
        }

        private void SliderWarnThreshold_ValueChanged(object sender, RoutedEventArgs e)
        {
            MainWindow.SaveSettings();
        }
    }
}
