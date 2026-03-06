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

namespace ZongziTEK.BlackboardSticker.Pages
{
    /// <summary>
    /// DatePage.xaml 的交互逻辑
    /// </summary>
    public partial class DatePage : Page
    {
        public DatePage()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
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
            LabelDate.Content = DateTime.Now.ToString("yyyy 年 M 月 d 日");
            LabelDayOfWeek.Content = DateTime.Now.ToString("dddd");
        }

        private void Page_Unloaded(object sender, EventArgs e)
        {
            timer.Stop();
        }
    }
}
