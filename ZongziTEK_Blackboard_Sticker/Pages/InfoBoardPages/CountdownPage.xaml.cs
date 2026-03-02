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
using System.Windows.Threading;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace ZongziTEK_Blackboard_Sticker.Pages
{
    /// <summary>
    /// CountdownPage.xaml 的交互逻辑
    /// </summary>
    public partial class CountdownPage : Page
    {
        public CountdownPage()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;

            Loaded += Page_Loaded;
            Unloaded += Page_Unloaded;
        }

        private DispatcherTimer timer;

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Timer_Tick(null, null);
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            TimeSpan timeSpan = MainWindow.Settings.InfoBoard.CountdownDate - DateTime.Now;
            string countdownName = MainWindow.Settings.InfoBoard.CountdownName;
            if (MainWindow.Settings.InfoBoard.CountdownName != null && MainWindow.Settings.InfoBoard.CountdownName.Length == 0) countdownName = "倒数日";
            if (timeSpan.TotalDays < 0)
            {
                LabelDays.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 204, 0));
                LabelName.Text = "距离" + countdownName + "开始已过去";
                timeSpan = -timeSpan;
            }
            else
            {
                if (timeSpan.Days < MainWindow.Settings.InfoBoard.CountdownWarnDays) LabelDays.Foreground = Brushes.Red;
                else LabelDays.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 204, 0));
                LabelName.Text = "距离" + countdownName + "还有";
            }
            LabelDays.Text = timeSpan.Days.ToString();
            LabelDaysDetail.Text = "." + Math.Truncate((timeSpan.TotalDays - timeSpan.Days) * 1000).ToString("000");
        }

        private void Page_Unloaded(object sender, EventArgs e)
        {
            timer.Stop();
        }
    }
}
