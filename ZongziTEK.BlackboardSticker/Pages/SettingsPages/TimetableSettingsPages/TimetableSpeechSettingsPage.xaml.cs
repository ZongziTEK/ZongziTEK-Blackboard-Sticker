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
using ZongziTEK.BlackboardSticker.Helpers;

namespace ZongziTEK.BlackboardSticker.Pages.SettingsPages.TimetableSettingsPages
{
    /// <summary>
    /// TimetableSpeechSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class TimetableSpeechSettingsPage : Page
    {
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

                if(voiceItem.Index == MainWindow.Settings.TimetableSettings.Voice)
                {
                    ComboBoxVoice.SelectedItem = item;
                }
            }
            
            isLoaded = true;
        }

        private List<VoiceItem> voiceItems = new List<VoiceItem>();
        private bool isLoaded = false;

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.SaveSettings();
        }

        private class VoiceItem
        {
            public eVoice Voice { get; set; }
            public int Index { get; set; }
        }

        private void ComboBoxVoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isLoaded) return;
            
            MainWindow.Settings.TimetableSettings.Voice = voiceItems[ComboBoxVoice.SelectedIndex].Index;
            MainWindow.SaveSettings();
        }

        private void ButtonResetVoice_Click(object sender, RoutedEventArgs e)
        {
            foreach (ComboBoxItem item in ComboBoxVoice.Items)
            {
                if (item.Content.ToString().ToLower().Contains("xiaoxiao"))
                {
                    ComboBoxVoice.SelectedItem = item;
                }
            }
        }

        private void ButtonPreviewVoice_Click(object sender, RoutedEventArgs e)
        {
            TTSHelper.PlayText("试听语音。距上课还有3分钟。准备上课，自习课即将开始。下课。下一节是自习课。");
        }
    }
}
