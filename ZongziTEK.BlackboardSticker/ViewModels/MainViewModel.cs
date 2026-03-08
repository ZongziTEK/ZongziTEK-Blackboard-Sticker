using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ZongziTEK.BlackboardSticker.Models;

namespace ZongziTEK.BlackboardSticker.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        public Settings Settings => MainWindow.Settings;

        [ObservableProperty]
        private string _currentTime;

        public MainViewModel()
        {
            var clockTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(1000 - DateTime.Now.Millisecond)
            };
            clockTimer.Tick += (sender, e) =>
            {
                var now = DateTime.Now;
                CurrentTime = now.ToString("HH:mm:ss");
                clockTimer.Interval = TimeSpan.FromMilliseconds(1000 - DateTime.Now.Millisecond);
            };
            clockTimer.Start();
        }
    }
}
