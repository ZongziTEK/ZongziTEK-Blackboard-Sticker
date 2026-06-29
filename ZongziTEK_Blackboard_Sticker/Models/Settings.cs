using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ZongziTEK_Blackboard_Sticker.Models
{
    public class Settings : INotifyPropertyChanged
    {
        private Storage _storage = new Storage();
        public Storage Storage
        {
            get => _storage;
            set
            {
                if (_storage != value)
                {
                    _storage = value;
                    OnPropertyChanged();
                }
            }
        }

        private Look _look = new Look();
        public Look Look
        {
            get => _look;
            set
            {
                if (_look != value)
                {
                    _look = value;
                    OnPropertyChanged();
                }
            }
        }

        private TimetableSettings _timetableSettings = new TimetableSettings();
        public TimetableSettings TimetableSettings
        {
            get => _timetableSettings;
            set
            {
                if (_timetableSettings != value)
                {
                    _timetableSettings = value;
                    OnPropertyChanged();
                }
            }
        }

        private Blackboard _blackboard = new Blackboard();
        public Blackboard Blackboard
        {
            get => _blackboard;
            set
            {
                if (_blackboard != value)
                {
                    _blackboard = value;
                    OnPropertyChanged();
                }
            }
        }

        private InfoBoard _infoBoard = new InfoBoard();
        public InfoBoard InfoBoard
        {
            get => _infoBoard;
            set
            {
                if (_infoBoard != value)
                {
                    _infoBoard = value;
                    OnPropertyChanged();
                }
            }
        }

        private Automation _automation = new Automation();
        public Automation Automation
        {
            get => _automation;
            set
            {
                if (_automation != value)
                {
                    _automation = value;
                    OnPropertyChanged();
                }
            }
        }

        private Update _update = new Update();
        public Update Update
        {
            get => _update;
            set
            {
                if (_update != value)
                {
                    _update = value;
                    OnPropertyChanged();
                }
            }
        }

        private Interactions _interactions = new Interactions();
        public Interactions Interactions
        {
            get => _interactions;
            set
            {
                if (_interactions != value)
                {
                    _interactions = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Storage : INotifyPropertyChanged
    {
        private bool _isFilesSavingWithProgram = true;
        public bool IsFilesSavingWithProgram
        {
            get => _isFilesSavingWithProgram;
            set
            {
                if (_isFilesSavingWithProgram != value)
                {
                    _isFilesSavingWithProgram = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _dataPath = "D:\\ZongziTEK_Blackboard_Sticker_Data";
        public string DataPath
        {
            get => _dataPath;
            set
            {
                if (_dataPath != value)
                {
                    _dataPath = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Look : INotifyPropertyChanged
    {
        private double _windowScaleMultiplier = 1;
        public double WindowScaleMultiplier
        {
            get => _windowScaleMultiplier;
            set
            {
                if (_windowScaleMultiplier != value)
                {
                    _windowScaleMultiplier = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _theme = 0;
        public int Theme
        {
            get => _theme;
            set
            {
                if (_theme != value)
                {
                    _theme = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isAnimationEnhanced = true;
        public bool IsAnimationEnhanced
        {
            get => _isAnimationEnhanced;
            set
            {
                if (_isAnimationEnhanced != value)
                {
                    _isAnimationEnhanced = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _lookMode = 0;
        public int LookMode
        {
            get => _lookMode;
            set
            {
                if (_lookMode != value)
                {
                    _lookMode = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isLauncherEnabled = true;
        public bool IsLauncherEnabled
        {
            get => _isLauncherEnabled;
            set
            {
                if (_isLauncherEnabled != value)
                {
                    _isLauncherEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isWindowHeightAdjustmentEnabled = false;
        public bool IsWindowHeightAdjustmentEnabled
        {
            get => _isWindowHeightAdjustmentEnabled;
            set
            {
                if (_isWindowHeightAdjustmentEnabled != value)
                {
                    _isWindowHeightAdjustmentEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _windowHeightPercent = 100;
        public double WindowHeightPercent
        {
            get => _windowHeightPercent;
            set
            {
                if (_windowHeightPercent != value)
                {
                    _windowHeightPercent = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _windowVerticalAlignment = 0;
        public int WindowVerticalAlignment
        {
            get => _windowVerticalAlignment;
            set
            {
                if (_windowVerticalAlignment != value)
                {
                    _windowVerticalAlignment = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isWindowChromeDisabled = false;
        public bool IsWindowChromeDisabled
        {
            get => _isWindowChromeDisabled;
            set
            {
                if (_isWindowChromeDisabled != value)
                {
                    _isWindowChromeDisabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _backgroundStyle = 0;
        public int BackgroundStyle
        {
            get => _backgroundStyle;
            set
            {
                if (_backgroundStyle != value)
                {
                    _backgroundStyle = value;
                    OnPropertyChanged();
                }
            }
        }

        private CustomBackgroundStyle _customBackgroundStyle = new CustomBackgroundStyle();
        public CustomBackgroundStyle CustomBackgroundStyle
        {
            get
            {
                if (_customBackgroundStyle == null) _customBackgroundStyle = new CustomBackgroundStyle();
                return _customBackgroundStyle;
            }
            set
            {
                if (_customBackgroundStyle != value)
                {
                    _customBackgroundStyle = value ?? new CustomBackgroundStyle();
                    OnPropertyChanged();
                }
            }
        }

        private bool _isComponentTitleTextHidden = false;
        public bool IsComponentTitleTextHidden
        {
            get => _isComponentTitleTextHidden;
            set
            {
                if (_isComponentTitleTextHidden != value)
                {
                    _isComponentTitleTextHidden = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _targetMonitor = 0;
        public int TargetMonitor
        {
            get => _targetMonitor;
            set
            {
                if (_targetMonitor != value)
                {
                    _targetMonitor = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CustomBackgroundStyle : INotifyPropertyChanged
    {
        private BackgroundElementStyle _mainPanel = new BackgroundElementStyle("#FEFEFE", 60);
        public BackgroundElementStyle MainPanel
        {
            get => _mainPanel;
            set
            {
                if (_mainPanel != value)
                {
                    _mainPanel = value ?? new BackgroundElementStyle("#FEFEFE", 60);
                    OnPropertyChanged();
                }
            }
        }

        private BackgroundElementStyle _topPanel = new BackgroundElementStyle("#FEFEFE", 60);
        public BackgroundElementStyle TopPanel
        {
            get => _topPanel;
            set
            {
                if (_topPanel != value)
                {
                    _topPanel = value ?? new BackgroundElementStyle("#FEFEFE", 60);
                    OnPropertyChanged();
                }
            }
        }

        private BackgroundElementStyle _blackboardPanel = new BackgroundElementStyle("#FEFEFE", 60);
        public BackgroundElementStyle BlackboardPanel
        {
            get => _blackboardPanel;
            set
            {
                if (_blackboardPanel != value)
                {
                    _blackboardPanel = value ?? new BackgroundElementStyle("#FEFEFE", 60);
                    OnPropertyChanged();
                }
            }
        }

        private BackgroundElementStyle _launcherPanel = new BackgroundElementStyle("#FEFEFE", 60);
        public BackgroundElementStyle LauncherPanel
        {
            get => _launcherPanel;
            set
            {
                if (_launcherPanel != value)
                {
                    _launcherPanel = value ?? new BackgroundElementStyle("#FEFEFE", 60);
                    OnPropertyChanged();
                }
            }
        }

        private BackgroundElementStyle _timetablePanel = new BackgroundElementStyle("#FEFEFE", 60);
        public BackgroundElementStyle TimetablePanel
        {
            get => _timetablePanel;
            set
            {
                if (_timetablePanel != value)
                {
                    _timetablePanel = value ?? new BackgroundElementStyle("#FEFEFE", 60);
                    OnPropertyChanged();
                }
            }
        }

        private BackgroundElementStyle _functionMenu = new BackgroundElementStyle("#FEFEFE", 60);
        public BackgroundElementStyle FunctionMenu
        {
            get => _functionMenu;
            set
            {
                if (_functionMenu != value)
                {
                    _functionMenu = value ?? new BackgroundElementStyle("#FEFEFE", 60);
                    OnPropertyChanged();
                }
            }
        }

        private BackgroundElementStyle _blackboardTitleBar = new BackgroundElementStyle("#FFFFFF", 80);
        public BackgroundElementStyle BlackboardTitleBar
        {
            get => _blackboardTitleBar;
            set
            {
                if (_blackboardTitleBar != value)
                {
                    _blackboardTitleBar = value ?? new BackgroundElementStyle("#FFFFFF", 80);
                    OnPropertyChanged();
                }
            }
        }

        private BackgroundElementStyle _launcherTitleBar = new BackgroundElementStyle("#FFFFFF", 80);
        public BackgroundElementStyle LauncherTitleBar
        {
            get => _launcherTitleBar;
            set
            {
                if (_launcherTitleBar != value)
                {
                    _launcherTitleBar = value ?? new BackgroundElementStyle("#FFFFFF", 80);
                    OnPropertyChanged();
                }
            }
        }

        private BackgroundElementStyle _timetableTitleBar = new BackgroundElementStyle("#FFFFFF", 80);
        public BackgroundElementStyle TimetableTitleBar
        {
            get => _timetableTitleBar;
            set
            {
                if (_timetableTitleBar != value)
                {
                    _timetableTitleBar = value ?? new BackgroundElementStyle("#FFFFFF", 80);
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class BackgroundElementStyle : INotifyPropertyChanged
    {
        public BackgroundElementStyle()
        {
        }

        public BackgroundElementStyle(string color, double opacity)
        {
            _color = color;
            _opacity = opacity;
        }

        private string _color = "#FEFEFE";
        public string Color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    _color = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _opacity = 60;
        public double Opacity
        {
            get => _opacity;
            set
            {
                if (_opacity != value)
                {
                    _opacity = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TimetableSettings : INotifyPropertyChanged
    {
        private bool _isTimetableEnabled = true;
        public bool IsTimetableEnabled
        {
            get => _isTimetableEnabled;
            set
            {
                if (_isTimetableEnabled != value)
                {
                    _isTimetableEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isTimetableNotificationEnabled = true;
        public bool IsTimetableNotificationEnabled
        {
            get => _isTimetableNotificationEnabled;
            set
            {
                if (_isTimetableNotificationEnabled != value)
                {
                    _isTimetableNotificationEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _fontSize = 24;
        public double FontSize
        {
            get => _fontSize;
            set
            {
                if (_fontSize != value)
                {
                    _fontSize = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _beginNotificationTime = 60;
        public double BeginNotificationTime
        {
            get => _beginNotificationTime;
            set
            {
                if (_beginNotificationTime != value)
                {
                    _beginNotificationTime = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isBeginSpeechEnabled = false;
        public bool IsBeginSpeechEnabled
        {
            get => _isBeginSpeechEnabled;
            set
            {
                if (_isBeginSpeechEnabled != value)
                {
                    _isBeginSpeechEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _overNotificationTime = 10;
        public double OverNotificationTime
        {
            get => _overNotificationTime;
            set
            {
                if (_overNotificationTime != value)
                {
                    _overNotificationTime = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isOverSpeechEnabled = false;
        public bool IsOverSpeechEnabled
        {
            get => _isOverSpeechEnabled;
            set
            {
                if (_isOverSpeechEnabled != value)
                {
                    _isOverSpeechEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _voice = 55;
        public int Voice
        {
            get => _voice;
            set
            {
                if (_voice != value)
                {
                    _voice = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _timeOffset = 0;
        public double TimeOffset
        {
            get => _timeOffset;
            set
            {
                if (_timeOffset != value)
                {
                    _timeOffset = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isClickToHideNotificationEnabled = true;
        public bool IsClickToHideNotificationEnabled
        {
            get => _isClickToHideNotificationEnabled;
            set
            {
                if (_isClickToHideNotificationEnabled != value)
                {
                    _isClickToHideNotificationEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Blackboard : INotifyPropertyChanged
    {
        private bool _isLocked = false;
        public bool IsLocked
        {
            get => _isLocked;
            set
            {
                if (_isLocked != value)
                {
                    _isLocked = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class InfoBoard : INotifyPropertyChanged
    {
        private bool _isCountdownPageEnabled = true;
        public bool isCountdownPageEnabled
        {
            get => _isCountdownPageEnabled;
            set
            {
                if (_isCountdownPageEnabled != value)
                {
                    _isCountdownPageEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isDatePageEnabled = true;
        public bool isDatePageEnabled
        {
            get => _isDatePageEnabled;
            set
            {
                if (_isDatePageEnabled != value)
                {
                    _isDatePageEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isWeatherPageEnabled = true;
        public bool isWeatherPageEnabled
        {
            get => _isWeatherPageEnabled;
            set
            {
                if (_isWeatherPageEnabled != value)
                {
                    _isWeatherPageEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isWeatherForecastPageEnabled = true;
        public bool isWeatherForecastPageEnabled
        {
            get => _isWeatherForecastPageEnabled;
            set
            {
                if (_isWeatherForecastPageEnabled != value)
                {
                    _isWeatherForecastPageEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _countdownName = "高考";
        public string CountdownName
        {
            get => _countdownName;
            set
            {
                if (_countdownName != value)
                {
                    _countdownName = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime _countdownDate = DateTime.Parse("2025/6/7");
        public DateTime CountdownDate
        {
            get => _countdownDate;
            set
            {
                if (_countdownDate != value)
                {
                    _countdownDate = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _countdownWarnDays = 30;
        public int CountdownWarnDays
        {
            get => _countdownWarnDays;
            set
            {
                if (_countdownWarnDays != value)
                {
                    _countdownWarnDays = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _weatherCity = "101010100";
        public string WeatherCity
        {
            get => _weatherCity;
            set
            {
                if (_weatherCity != value)
                {
                    _weatherCity = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isRainForecastOnly = false;
        public bool IsRainForecastOnly
        {
            get => _isRainForecastOnly;
            set
            {
                if (_isRainForecastOnly != value)
                {
                    _isRainForecastOnly = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _switchIntervalSeconds = 4;
        public double SwitchIntervalSeconds
        {
            get => _switchIntervalSeconds;
            set
            {
                if (_switchIntervalSeconds != value)
                {
                    _switchIntervalSeconds = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Automation : INotifyPropertyChanged
    {
        private bool _isAutoHideHugoAssistantEnabled = false;
        public bool IsAutoHideHugoAssistantEnabled
        {
            get => _isAutoHideHugoAssistantEnabled;
            set
            {
                if (_isAutoHideHugoAssistantEnabled != value)
                {
                    _isAutoHideHugoAssistantEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isBottomMost = true;
        public bool IsBottomMost
        {
            get => _isBottomMost;
            set
            {
                if (_isBottomMost != value)
                {
                    _isBottomMost = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Update : INotifyPropertyChanged
    {
        private bool _isUpdateAutomatic = true;
        public bool IsUpdateAutomatic
        {
            get => _isUpdateAutomatic;
            set
            {
                if (_isUpdateAutomatic != value)
                {
                    _isUpdateAutomatic = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _updateChannel = 0;
        public int UpdateChannel
        {
            get => _updateChannel;
            set
            {
                if (_updateChannel != value)
                {
                    _updateChannel = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Interactions : INotifyPropertyChanged
    {
        private bool _isClassIslandConnectorEnabled = false;

        public bool IsClassIslandConnectorEnabled
        {
            get => _isClassIslandConnectorEnabled;
            set
            {
                if (_isClassIslandConnectorEnabled != value)
                {
                    _isClassIslandConnectorEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
