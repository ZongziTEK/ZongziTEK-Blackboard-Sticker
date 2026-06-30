using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
using ZongziTEK_Blackboard_Sticker.Helpers.Weather;
using static ZongziTEK_Blackboard_Sticker.Helpers.Weather.WeatherHelper;

namespace ZongziTEK_Blackboard_Sticker.Pages
{
    /// <summary>
    /// WeatherPage.xaml 的交互逻辑
    /// </summary>
    public partial class WeatherPage : Page
    {
        public WeatherPage()
        {
            InitializeComponent();

            timer.Interval = TimeSpan.FromMinutes(30);
            timer.Tick += Timer_Tick;

            Loaded += Page_Loaded;
            Unloaded += Page_Unloaded;
        }

        private DispatcherTimer timer = new DispatcherTimer();

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Timer_Tick(null, null);
            MainWindow.Settings.InfoBoard.PropertyChanged += InfoBoard_PropertyChanged;
            timer.Start();
        }

        private CurrentWeather currentWeather = new();

        private async void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            await Task.Run(() =>
            {
                if (!new DirectoryInfo("Weather/").Exists)
                {
                    try { new DirectoryInfo("Weather/").Create(); }
                    catch { }
                }

                if (File.Exists(xiaomiWeatherFilePath))
                {
                    DateTime currentWeatherFetchTime = new FileInfo(xiaomiWeatherFilePath).LastWriteTime;
                    if (DateTime.Now - currentWeatherFetchTime > TimeSpan.FromHours(1))
                    {
                        UpdateXiaomiWeather();
                    }
                    else
                    {
                        xiaomiWeather = JsonConvert.DeserializeObject<XiaomiWeather>(File.ReadAllText(xiaomiWeatherFilePath));

                        if (currentWeather != null)
                        {
                            if (lastCityCode != MainWindow.Settings.InfoBoard.WeatherCity)
                            {
                                UpdateXiaomiWeather();
                            }
                        }
                        else
                        {
                            UpdateXiaomiWeather();
                        }
                    }
                }
                else
                {
                    UpdateXiaomiWeather();
                }

                currentWeather = xiaomiWeather.Current;
                lastCityCode = MainWindow.Settings.InfoBoard.WeatherCity;

                Dispatcher.BeginInvoke(() =>
                {
                    ShowCurrentWeather();
                });
            });
            timer.Start();
        }

        private void ShowCurrentWeather()
        {
            if (currentWeather != null)
            {
                CityCodeToNameConverter cityCodeToNameConverter = new();

                string locationName = ((string)cityCodeToNameConverter.Convert(MainWindow.Settings.InfoBoard.WeatherCity, typeof(string), null, System.Globalization.CultureInfo.CurrentCulture))
                    .Replace('.', ' ');
                string weatherName = GetWeatherName(Convert.ToInt32(currentWeather.Weather));
                int aqiValue = Convert.ToInt32(xiaomiWeather.Aqi.Value);

                LabelWeatherInfo.Content = weatherName + " " + currentWeather.Temperature.Value + currentWeather.Temperature.Unit;
                LabelCity.Content = locationName;
                TextHumidity.Text = currentWeather.Humidity.Value + currentWeather.Humidity.Unit;
                TextAqi.Text = xiaomiWeather.Aqi.Value;

                // 加载天气图标
                string imagePath = "../../Resources/WeatherIcons/";
                if (weatherName.Contains("雷"))
                {
                    imagePath += "Thundery.png";
                }
                else if (weatherName.Contains("雨"))
                {
                    imagePath += "Rainy.png";
                }
                else if (weatherName.Contains("雪"))
                {
                    imagePath += "Snowy.png";
                }
                else if (weatherName.Contains("阴"))
                {
                    imagePath += "Cloudy.png";
                }
                else if (weatherName.Contains("晴"))
                {
                    imagePath += "Sunny.png";
                }
                else
                {
                    imagePath += "MostCloudy.png";
                }
                ImageWeather.Source = new BitmapImage(new Uri(imagePath, UriKind.Relative));

                // 判断空气质量等级
                if (aqiValue == -1)
                {
                    TextAqi.Text = "未知";
                    BorderAqi.Background = new SolidColorBrush(Colors.Gray);
                }
                else
                {
                    LinearGradientBrush aqiBackgroundBrush = new()
                    {
                        StartPoint = new Point(0.5, 1),
                        EndPoint = new Point(1, 0),
                    };

                    GradientStop startPointStop = new GradientStop();
                    GradientStop endPointStop = new GradientStop() { Offset = 1 };

                    aqiBackgroundBrush.GradientStops.Add(startPointStop);
                    aqiBackgroundBrush.GradientStops.Add(endPointStop);

                    switch (GetAqiLevel(aqiValue))
                    {
                        case 0:
                            TextAqi.Text += " 优";
                            startPointStop.Color = Color.FromRgb(0, 187, 54);
                            endPointStop.Color = Color.FromRgb(160, 210, 0);
                            break;
                        case 1:
                            TextAqi.Text += " 良";
                            startPointStop.Color = Color.FromRgb(255, 187, 0);
                            endPointStop.Color = Color.FromRgb(255, 225, 0);
                            break;
                        case 2:
                            TextAqi.Text += " 轻度污染";
                            startPointStop.Color = Colors.DarkOrange;
                            endPointStop.Color = Colors.OrangeRed;
                            break;
                        case 3:
                            TextAqi.Text += " 中度污染";
                            startPointStop.Color = Colors.OrangeRed;
                            endPointStop.Color = Colors.Red;
                            break;
                        case 4:
                            TextAqi.Text += " 重度污染";
                            startPointStop.Color = Color.FromRgb(111, 47, 160);
                            endPointStop.Color = Color.FromRgb(255, 0, 255);
                            break;
                        case 5:
                            TextAqi.Text += " 严重污染";
                            startPointStop.Color = Color.FromRgb(153, 1, 52);
                            endPointStop.Color = Color.FromRgb(105, 0, 35);
                            break;
                    }

                    BorderAqi.Background = aqiBackgroundBrush;
                }

                ImageWeather.Visibility = Visibility.Visible;
                if (MainWindow.Settings.Look.LookMode == 0)
                {
                    BorderHumidity.Visibility = Visibility.Visible;
                    BorderAqi.Visibility = Visibility.Visible;
                }
                else
                {
                    BorderHumidity.Visibility = Visibility.Collapsed;
                    BorderAqi.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                LabelWeatherInfo.Content = "暂无实况天气信息";
                LabelCity.Content = "天气";
                ImageWeather.Visibility = Visibility.Collapsed;
                BorderHumidity.Visibility = Visibility.Collapsed;
                BorderAqi.Visibility = Visibility.Collapsed;
            }
        }

        private void InfoBoard_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(MainWindow.Settings.InfoBoard.WeatherCity)) return;

            Timer_Tick(null, null);
        }

        private void Page_Unloaded(object sender, EventArgs e)
        {
            timer.Stop();
            MainWindow.Settings.InfoBoard.PropertyChanged -= InfoBoard_PropertyChanged;
            ImageWeather.Source = null;
        }
    }
}
