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

namespace ZongziTEK_Blackboard_Sticker.Pages.SettingsPages.InfoBoardSettingsPages
{
    /// <summary>
    /// InfoBoardGenericSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class InfoBoardGenericSettingsPage : Page
    {
        public InfoBoardGenericSettingsPage()
        {
            InitializeComponent();

            DataContext = MainWindow.Settings.InfoBoard;

            CheckBoxes = new List<CheckBox>
            {
                CheckBoxDate,
                CheckBoxCountdown,
                CheckBoxLiveWeather,
                CheckBoxCastWeather
            };
        }

        private List<CheckBox> CheckBoxes = new List<CheckBox>();

        private void SliderSwitchInterval_ValueChanged(object sender, RoutedEventArgs e)
        {
            MainWindow.SaveSettings();
            (Application.Current.MainWindow as MainWindow)?.ApplyInfoBoardSwitchInterval();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.SaveSettings();

            bool HasNoCheckBoxSelected = true;

            foreach (CheckBox checkBox in CheckBoxes)
            {
                if (checkBox.IsChecked == true)
                {
                    HasNoCheckBoxSelected = false;
                    break;
                }
            }

            if (HasNoCheckBoxSelected)
            {
                CheckBoxDate.IsChecked = true;
                return;
            }

            (Application.Current.MainWindow as MainWindow).LoadFrameInfoPagesList();
        }
    }
}
