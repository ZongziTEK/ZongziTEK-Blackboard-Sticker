using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Resources;
using RestSharp;
using iNKORE.UI.WPF.Modern.Common.IconKeys;
using Microsoft.Xaml.Behaviors.Layout;

namespace ZongziTEK.BlackboardSticker.Helpers.Weather
{
    public class WeatherHelper
    {
        public static string xiaomiWeatherFilePath = "Weather/XiaomiWeather.json";
        public static XiaomiWeather xiaomiWeather = new();

        public static WeatherCityData GetWeatherCityData()
        {
            string jsonContent;
            WeatherCityData weatherCityInfo;

            Uri uri = new Uri("Resources/XiaomiWeather/xiaomi_weather.json", UriKind.Relative);
            StreamResourceInfo streamInfo = Application.GetResourceStream(uri);
            using (StreamReader reader = new StreamReader(streamInfo.Stream))
            {
                jsonContent = reader.ReadToEnd();
            }

            weatherCityInfo = JsonConvert.DeserializeObject<WeatherCityData>(jsonContent);
            return weatherCityInfo;
        }

        public static XiaomiWeather GetXiaomiWeather(string cityCode)
        {
            var weather = new XiaomiWeather();
            try
            {
                var locationKey = "weathercn:" + cityCode;
                var request_url = "https://weatherapi.market.xiaomi.com/wtr-v3/weather/all?latitude=0&longitude=0&days=5&appKey=weather20151024&sign=zUFJoAR2ZVrDy1vF3D07&isGlobal=false&locale=zh_cn" + "&locationKey=" + locationKey;

                var clientOptions = new RestClientOptions(request_url);
                var client = new RestClient(clientOptions);

                var request = new RestRequest()
                {
                    Method = Method.Get
                };

                request.AddHeader("Accept", "*/*");

                var jsonObject = JObject.Parse(client.Execute(request).Content);
                var weatherJson = "";
                weatherJson = jsonObject.ToString();
                weather = JsonConvert.DeserializeObject<XiaomiWeather>(weatherJson);
            }
            catch { }
            return weather;
        }

        public static void UpdateXiaomiWeather()
        {
            xiaomiWeather = GetXiaomiWeather(MainWindow.Settings.InfoBoard.WeatherCity);
            try
            {
                File.WriteAllText(xiaomiWeatherFilePath, JsonConvert.SerializeObject(xiaomiWeather, Formatting.Indented));
            }
            catch (Exception ex)
            {
                LogHelper.NewLog(ex.Message);
            }
        }

        public static string GetWeatherName(int weatherCode)
        {
            string jsonContent;

            Uri uri = new Uri("Resources/XiaomiWeather/xiaomi_weather_status.json", UriKind.Relative);
            StreamResourceInfo streamInfo = Application.GetResourceStream(uri);
            using (StreamReader reader = new StreamReader(streamInfo.Stream))
            {
                jsonContent = reader.ReadToEnd();
            }

            var jObject = JObject.Parse(jsonContent);

            List<WeatherInfo> weatherInfos = JsonConvert.DeserializeObject<List<WeatherInfo>>(jObject["weatherinfo"].ToString());
            WeatherInfo weatherInfo = weatherInfos.Find(w => w.Code == weatherCode);

            return weatherInfo.Name;
        }

        public static FontIconData GetWeatherIcon(int weatherCode, bool isNight)
        {
            FontIconData fontIconData = new();

            switch (weatherCode)
            {
                case 0:
                    if (!isNight) fontIconData = FluentSystemIcons.WeatherSunny_20_Regular;
                    else fontIconData = FluentSystemIcons.WeatherMoon_20_Regular;
                    break;
                case 1:
                    if (!isNight) fontIconData = FluentSystemIcons.WeatherPartlyCloudyDay_20_Regular;
                    else fontIconData = FluentSystemIcons.WeatherPartlyCloudyNight_20_Regular;
                    break;
                case 2:
                    fontIconData = FluentSystemIcons.WeatherCloudy_20_Regular;
                    break;
                case 3:
                case int n when (n >= 7 && n <= 12):
                case 19:
                case int m when (m >= 21 && m <= 25): // 全是雨
                    fontIconData = FluentSystemIcons.WeatherRain_20_Regular;
                    break;
                case 4:
                case 5: // 全是雷
                    fontIconData = FluentSystemIcons.WeatherThunderstorm_20_Regular;
                    break;
                case 6:
                    fontIconData = FluentSystemIcons.WeatherRainSnow_20_Regular;
                    break;
                case int n when (n >= 13 && n <= 17):
                case int m when (m >= 26 && m <= 28): // 全是雪
                    fontIconData = FluentSystemIcons.WeatherSnow_20_Regular;
                    break;
                case 18:
                case 35:
                    fontIconData = FluentSystemIcons.WeatherFog_20_Regular;
                    break;
                case 20:
                case int n when (n >= 29 && n <= 31):
                    fontIconData = FluentSystemIcons.WeatherDuststorm_20_Regular;
                    break;
                case 32:
                case 33:
                    fontIconData = FluentSystemIcons.WeatherSqualls_20_Regular;
                    break;
                case 34:
                    fontIconData = FluentSystemIcons.WeatherBlowingSnow_20_Regular;
                    break;
                case 53:
                    if (!isNight) fontIconData = FluentSystemIcons.WeatherHaze_20_Regular;
                    else fontIconData = FluentSystemIcons.WeatherFog_20_Regular;
                    break;
                default:
                    fontIconData = FluentSystemIcons.WeatherCloudy_20_Regular;
                    break;
            }

            return fontIconData;
        }

        public static int GetAqiLevel(int AqiValue)
        {
            if (AqiValue <= 50) // 优
            {
                return 0;
            }
            if (AqiValue <= 100) // 良
            {
                return 1;
            }
            if (AqiValue <= 150) // 轻度污染
            {
                return 2;
            }
            if (AqiValue <= 200) // 中度污染
            {
                return 3;
            }
            if (AqiValue <= 300) // 重度污染
            {
                return 4;
            }
            return 5; // 严重污染
        }

        public static string lastCityCode;
    }

    #region City
    public class WeatherCityData
    {
        [JsonProperty("provinces")] public Province[] Provinces { get; set; }
        [JsonProperty("citys")] public City[] Cities { get; set; }
    }

    public class Province
    {
        [JsonProperty("_id")] public int Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
    }

    public class City
    {
        [JsonProperty("_id")] public int Id { get; set; }
        [JsonProperty("province_id")] public int ProvinceId { get; set; } // ProvinceId = Province.Id - 1
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("city_num")] public string CityCode { get; set; }
    }
    #endregion

    public class XiaomiWeather
    {
        [JsonProperty("current")] public CurrentWeather Current { get; set; }
        [JsonProperty("forecastDaily")] public ForecastDaily ForecastDaily { get; set; }
        [JsonProperty("aqi")] public Aqi Aqi { get; set; }
    }

    #region current
    public class CurrentWeather
    {
        [JsonProperty("weather")] public string Weather { get; set; }
        [JsonProperty("uvIndex")] public string UvIndex { get; set; }
        [JsonProperty("pubTime")] public string PubTime { get; set; }
        [JsonProperty("feelsLike")] public FeelsLike FeelsLike { get; set; }
        [JsonProperty("humidity")] public Humidity Humidity { get; set; }
        [JsonProperty("pressure")] public Pressure Pressure { get; set; }
        [JsonProperty("temperature")] public Temperature Temperature { get; set; }
        [JsonProperty("visibility")] public AirVisibility Visibility { get; set; }
        [JsonProperty("wind")] public Wind Wind { get; set; }
    }

    public class FeelsLike
    {
        [JsonProperty("unit")] public string Unit { get; set; }
        [JsonProperty("value")] public string Value { get; set; }
    }
    public class Humidity
    {
        [JsonProperty("unit")] public string Unit { get; set; }
        [JsonProperty("value")] public string Value { get; set; }
    }
    public class Pressure
    {
        [JsonProperty("unit")] public string Unit { get; set; }
        [JsonProperty("value")] public string Value { get; set; }
    }
    public class Temperature
    {
        [JsonProperty("unit")] public string Unit { get; set; }
        [JsonProperty("value")] public string Value { get; set; }
    }
    public class AirVisibility
    {
        [JsonProperty("unit")] public string Unit { get; set; }
        [JsonProperty("value")] public string Value { get; set; }
    }
    public class Wind
    {
        [JsonProperty("direction")] public Direction Direction { get; set; }
        [JsonProperty("speed")] public Speed Speed { get; set; }
    }
    public class Direction
    {
        [JsonProperty("unit")] public string Unit { get; set; }
        [JsonProperty("value")] public string Value { get; set; }
    }
    public class Speed
    {
        [JsonProperty("unit")] public string Unit { get; set; }
        [JsonProperty("value")] public string Value { get; set; }
    }
    #endregion

    #region forecastDaily
    public class ForecastDaily
    {
        [JsonProperty("weather")] public ForecastDailyWeather Weather { get; set; }
        [JsonProperty("temperature")] public ForecastDailyTemperature Temperature { get; set; }
    }

    public class ForecastDailyTemperature
    {
        [JsonProperty("status")] public int Status { get; set; }
        [JsonProperty("unit")] public string Unit { get; set; }
        [JsonProperty("value")] public FromToValue[] Values { get; set; }
    }

    public class ForecastDailyWeather
    {
        [JsonProperty("status")] public int Status { get; set; }
        [JsonProperty("value")] public FromToValue[] Values { get; set; }
    }

    public class FromToValue
    {
        [JsonProperty("from")] public string From { get; set; }
        [JsonProperty("to")] public string To { get; set; }
    }
    #endregion

    #region aqi
    public class Aqi
    {
        [JsonProperty("aqi")] public string Value { get; set; }
    }
    #endregion

    #region ForecastWeatherItemData
    public class ForecastWeatherItemData
    {
        public string DayName { get; set; }
        public FontIconData WeatherFromIcon { get; set; }
        public FontIconData WeatherToIcon { get; set; }
        public string TemperatureFrom { get; set; }
        public string TemperatureTo { get; set; }
        public bool IsRainy { get; set; }
    }
    #endregion

    #region WeatherInfo
    public class WeatherInfo
    {
        [JsonProperty("code")] public int Code { get; set; }
        [JsonProperty("wea")] public string Name { get; set; }
    }
    #endregion

    #region Converter
    public class CityCodeToNameConverter : IValueConverter
    {
        private static readonly List<City> Cities = WeatherHelper.GetWeatherCityData().Cities.ToList();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string cityCode)
            {
                var city = Cities.Find(c => c.CityCode == cityCode);
                return city?.Name ?? "未知";
            }
            return "Invalid input";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}
