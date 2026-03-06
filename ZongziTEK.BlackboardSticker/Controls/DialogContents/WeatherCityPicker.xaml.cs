using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using iNKORE.UI.WPF.Controls;
using ZongziTEK.BlackboardSticker.Helpers.Weather;

namespace ZongziTEK.BlackboardSticker.Controls.DialogContents
{
    /// <summary>
    /// WeatherCityPicker.xaml 的交互逻辑
    /// </summary>
    public partial class WeatherCityPicker : UserControl
    {
        public WeatherCityPicker()
        {
            InitializeComponent();

            LoadCityData();
            FilterCitiesAsync();
        }

        private WeatherCityData weatherCityData = new();
        private List<City> filteredCities = new();
        private System.Threading.Timer searchDelayTimer;

        private void LoadCityData()
        {
            weatherCityData = WeatherHelper.GetWeatherCityData();
        }

        private void TextBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            searchDelayTimer?.Dispose();
            searchDelayTimer = new System.Threading.Timer(_ =>
            {
                Dispatcher.Invoke(() => FilterCitiesAsync());
            }, null, 300, System.Threading.Timeout.Infinite);
        }

        private void FilterCities()
        {
            StackPanelSearchResult.Children.Clear();
            string searchText = TextBoxSearch.Text;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                filteredCities = weatherCityData.Cities.ToList();
            }
            else
            {
                filteredCities = weatherCityData.Cities.Where(c => c.Name.Contains(searchText)).ToList();
            }

            var filteredProvinceIds = filteredCities
                .Select(c => c.ProvinceId)
                .Distinct()
                .ToList();

            var filteredProvinces = weatherCityData.Provinces
                .Where(p => filteredProvinceIds.Contains(p.Id - 1))
                .ToList();

            if (filteredCities.Count == 0)
            {
                TextNoResult.Visibility = Visibility.Visible;
                return;
            }

            foreach (Province province in filteredProvinces)
            {
                var citiesInProvince = filteredCities
                    .Where(c => c.ProvinceId == province.Id - 1)
                    .ToList();

                Expander expander = new()
                {
                    Header = province.Name,
                    Padding = new Thickness(8)
                };

                SimpleStackPanel stackPanel = new() { Spacing = 4 };

                if (citiesInProvince.Count != 0)
                {
                    foreach (City city in citiesInProvince)
                    {
                        RadioButton radioButton = new()
                        {
                            Content = city.Name,
                            GroupName = "WeatherCity"
                        };

                        if (city.CityCode == MainWindow.Settings.InfoBoard.WeatherCity)
                        {
                            radioButton.IsChecked = true;
                            expander.IsExpanded = true;
                        }

                        radioButton.Checked += RadioButton_Checked;

                        stackPanel.Children.Add(radioButton);
                    }
                    if (!string.IsNullOrWhiteSpace(searchText)) expander.IsExpanded = true;
                }

                expander.Content = stackPanel;
                StackPanelSearchResult.Children.Add(expander);
            }
        }

        private async void FilterCitiesAsync()
        {

            try
            {
                StackPanelSearchResult.Visibility = Visibility.Collapsed;
                ProgressRing.Visibility = Visibility.Visible;
                TextNoResult.Visibility = Visibility.Collapsed;

                await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);
                await Dispatcher.BeginInvoke(FilterCities);
            }
            finally
            {
                ProgressRing.Visibility = Visibility.Collapsed;
                StackPanelSearchResult.Visibility = Visibility.Visible;
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            string cityName = (sender as RadioButton).Content.ToString();
            City city = weatherCityData.Cities
                .FirstOrDefault(c => c.Name == cityName);
            string cityCode = city.CityCode;

            MainWindow.Settings.InfoBoard.WeatherCity = cityCode;
            MainWindow.SaveSettings();
        }
    }
}
