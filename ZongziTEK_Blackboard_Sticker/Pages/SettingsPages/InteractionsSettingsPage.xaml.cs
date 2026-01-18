using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using ZongziTEK_Blackboard_Sticker.Services;

namespace ZongziTEK_Blackboard_Sticker.Pages.SettingsPages
{
    /// <summary>
    /// InteractionsSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class InteractionsSettingsPage : Page
    {
        public Interactions InteractionsSettings { get; set; }

        public InteractionsSettingsPage()
        {
            InitializeComponent();

            InteractionsSettings = MainWindow.Settings.Interactions;
            InteractionsSettings.PropertyChanged += InteractionsSettings_PropertyChanged;

            DataContext = this;
        }

        private void InteractionsSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Interactions.IsClassIslandConnectorEnabled) && App.ServiceManager != null)
            {
                if (InteractionsSettings.IsClassIslandConnectorEnabled)
                {
                    App.ServiceManager.RegisterService<ClassIslandConnectorService>();
                }
                else
                {
                    App.ServiceManager.RemoveService<ClassIslandConnectorService>();
                }
            }

            MainWindow.SaveSettings();
        }

        private void InteractionsSettingsPage_OnUnloaded(object sender, RoutedEventArgs e)
        {
            InteractionsSettings.PropertyChanged -= InteractionsSettings_PropertyChanged;
        }
    }
}
