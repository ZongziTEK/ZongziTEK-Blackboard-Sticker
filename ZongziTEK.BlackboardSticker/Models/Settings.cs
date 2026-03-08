using System;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace ZongziTEK.BlackboardSticker.Models
{
    public partial class Settings : ObservableObject
    {
        [ObservableProperty]
        private Storage _storage = new();

        [ObservableProperty]
        private Look _look = new();

        [ObservableProperty]
        private TimetableSettings _timetableSettings = new();

        [ObservableProperty]
        private Blackboard _blackboard = new();

        [ObservableProperty]
        private InfoBoard _infoBoard = new();

        [ObservableProperty]
        private Automation _automation = new();

        [ObservableProperty]
        private Update _update = new();

        [ObservableProperty]
        private Interactions _interactions = new();
    }

    public partial class Storage : ObservableObject
    {
        [ObservableProperty]
        private bool _isFilesSavingWithProgram = true;

        [ObservableProperty]
        private string _dataPath = "D:\\ZongziTEK_Blackboard_Sticker_Data";
    }

    public partial class Look : ObservableObject
    {
        [ObservableProperty]
        private double _windowScaleMultiplier = 1;

        [ObservableProperty]
        private int _theme = 0;

        [ObservableProperty]
        private bool _isAnimationEnhanced = true;

        [ObservableProperty]
        private int _lookMode = 0;

        [ObservableProperty]
        private bool _isWindowChromeDisabled = false;

        [ObservableProperty]
        private int _targetMonitor = 0;
    }

    public partial class TimetableSettings : ObservableObject
    {
        [ObservableProperty]
        private bool _isTimetableEnabled = true;

        [ObservableProperty]
        private bool _isTimetableNotificationEnabled = true;

        [ObservableProperty]
        private double _fontSize = 24;

        [ObservableProperty]
        private double _beginNotificationTime = 60;

        [ObservableProperty]
        private bool _isBeginSpeechEnabled = false;

        [ObservableProperty]
        private double _overNotificationTime = 10;

        [ObservableProperty]
        private bool _isOverSpeechEnabled = false;

        [ObservableProperty]
        private int _voice = 55;

        [ObservableProperty]
        private double _timeOffset = 0;

        [ObservableProperty]
        private bool _isClickToHideNotificationEnabled = true;
    }

    public partial class Blackboard : ObservableObject
    {
        [ObservableProperty]
        private bool _isLocked = false;
    }

    public partial class InfoBoard : ObservableObject
    {
        [ObservableProperty]
        [property: JsonProperty("isCountdownPageEnabled")]
        private bool _isCountdownPageEnabled = true;

        [ObservableProperty]
        [property: JsonProperty("isDatePageEnabled")]
        private bool _isDatePageEnabled = true;

        [ObservableProperty]
        [property: JsonProperty("isWeatherPageEnabled")]
        private bool _isWeatherPageEnabled = true;

        [ObservableProperty]
        [property: JsonProperty("isWeatherForecastPageEnabled")]
        private bool _isWeatherForecastPageEnabled = true;

        [ObservableProperty]
        private string _countdownName = "高考";

        [ObservableProperty]
        private DateTime _countdownDate = DateTime.Parse("2025/6/7");

        [ObservableProperty]
        private int _countdownWarnDays = 30;

        [ObservableProperty]
        private string _weatherCity = "101010100";

        [ObservableProperty]
        private bool _isRainForecastOnly = false;
    }

    public partial class Automation : ObservableObject
    {
        [ObservableProperty]
        private bool _isAutoHideHugoAssistantEnabled = false;

        [ObservableProperty]
        private bool _isBottomMost = true;
    }

    public partial class Update : ObservableObject
    {
        [ObservableProperty]
        private bool _isUpdateAutomatic = true;

        [ObservableProperty]
        private int _updateChannel = 0;
    }

    public partial class Interactions : ObservableObject
    {
        [ObservableProperty]
        private bool _isClassIslandConnectorEnabled = false;
    }
}