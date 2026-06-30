using Edge_tts_sharp;
using Edge_tts_sharp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZongziTEK_Blackboard_Sticker.Helpers;
using ZongziTEK_Blackboard_Sticker.Models;

namespace ZongziTEK_Blackboard_Sticker.Pages.SettingsPages.TimetableSettingsPages
{
    /// <summary>
    /// TimetableSpeechSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class TimetableSpeechSettingsPage : Page
    {
        private readonly Settings defaultSettings = new Settings();
        private readonly List<SettingsResetItem> resetItems = new List<SettingsResetItem>();
        private SettingsResetItem resetSpeechSelection;
        private SettingsResetItem resetVoice;

        public TimetableSpeechSettingsPage()
        {
            InitializeComponent();

            DataContext = MainWindow.Settings.TimetableSettings;

            if (!MainWindow.Settings.TimetableSettings.IsTimetableEnabled)
            {
                ScrollViewerRoot.Visibility = Visibility.Collapsed;
                LabelTimetableDisabledHint.Visibility = Visibility.Visible;
            }
            var voices = Edge_tts.GetVoice();
            foreach (eVoice voice in voices)
            {
                if (voice.Locale.Contains("zh"))
                {
                    voiceItems.Add(new VoiceItem() { Voice = voice, Index = voices.IndexOf(voice) });
                }
            }

            foreach (var voiceItem in voiceItems)
            {
                ComboBoxItem item = new()
                {
                    Content = voiceItem.Voice.FriendlyName
                };

                ComboBoxVoice.Items.Add(item);
            }

            int selectedVoice = SetSelectedVoice(MainWindow.Settings.TimetableSettings.Voice);
            MainWindow.Settings.TimetableSettings.Voice = selectedVoice;
            
            isLoaded = true;
            InitializeResetItems();
            UpdateResetButtons();
        }

        private List<VoiceItem> voiceItems = new List<VoiceItem>();
        private bool isLoaded = false;

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ApplySpeechSettings();
        }

        private class VoiceItem
        {
            public eVoice Voice { get; set; }
            public int Index { get; set; }
        }

        private void ComboBoxVoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isLoaded) return;
            if (ComboBoxVoice.SelectedIndex < 0 || ComboBoxVoice.SelectedIndex >= voiceItems.Count) return;
            
            MainWindow.Settings.TimetableSettings.Voice = voiceItems[ComboBoxVoice.SelectedIndex].Index;
            ApplySpeechSettings();
        }

        private void InitializeResetItems()
        {
            resetSpeechSelection = SettingsResetItem.Register(
                resetItems,
                enabled => ButtonResetSpeechSelection.IsEnabled = enabled,
                HasSpeechSelectionChanged,
                ResetSpeechSelection,
                ApplySpeechSettings);

            resetVoice = SettingsResetItem.Register(
                resetItems,
                enabled => ButtonResetVoice.IsEnabled = enabled,
                () => MainWindow.Settings.TimetableSettings.Voice,
                () => ResolveVoiceIndex(defaultSettings.TimetableSettings.Voice),
                value =>
                {
                    MainWindow.Settings.TimetableSettings.Voice = SetSelectedVoice(value);
                },
                ApplySpeechSettings);
        }

        private void ApplySpeechSettings()
        {
            if (SettingsResetItem.IsResetting) return;

            MainWindow.SaveSettings();
            UpdateResetButtons();
        }

        private void ButtonResetVoice_Click(object sender, RoutedEventArgs e)
        {
            resetVoice.Reset();
            e.Handled = true;
        }

        private void ButtonPreviewVoice_Click(object sender, RoutedEventArgs e)
        {
            TTSHelper.PlayText("试听语音。距上课还有3分钟。准备上课，自习课即将开始。下课。下一节是自习课。");
        }

        private void ButtonResetSpeechSelection_Click(object sender, RoutedEventArgs e)
        {
            resetSpeechSelection.Reset();
            e.Handled = true;
        }

        private bool HasSpeechSelectionChanged()
        {
            TimetableSettings settings = MainWindow.Settings.TimetableSettings;
            TimetableSettings defaults = defaultSettings.TimetableSettings;

            return settings.IsBeginSpeechEnabled != defaults.IsBeginSpeechEnabled ||
                   settings.IsOverSpeechEnabled != defaults.IsOverSpeechEnabled;
        }

        private void ResetSpeechSelection()
        {
            MainWindow.Settings.TimetableSettings.IsBeginSpeechEnabled = defaultSettings.TimetableSettings.IsBeginSpeechEnabled;
            MainWindow.Settings.TimetableSettings.IsOverSpeechEnabled = defaultSettings.TimetableSettings.IsOverSpeechEnabled;
        }

        private int ResolveVoiceIndex(int voiceIndex)
        {
            foreach (VoiceItem voiceItem in voiceItems)
            {
                if (voiceItem.Index == voiceIndex)
                {
                    return voiceIndex;
                }
            }

            return voiceItems.Count > 0 ? voiceItems[0].Index : voiceIndex;
        }

        private int SetSelectedVoice(int voiceIndex)
        {
            int resolvedVoiceIndex = ResolveVoiceIndex(voiceIndex);

            for (int i = 0; i < voiceItems.Count; i++)
            {
                if (voiceItems[i].Index == resolvedVoiceIndex)
                {
                    ComboBoxVoice.SelectedIndex = i;
                    return resolvedVoiceIndex;
                }
            }

            ComboBoxVoice.SelectedIndex = -1;
            return resolvedVoiceIndex;
        }

        private void UpdateResetButtons()
        {
            SettingsResetItem.UpdateAll(resetItems);
        }
    }
}
