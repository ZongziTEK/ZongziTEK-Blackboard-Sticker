using Edge_tts_sharp;
using Edge_tts_sharp.Model;
using System.Net.NetworkInformation;
using System.Speech.Synthesis;
using System.Threading.Tasks;

namespace ZongziTEK_Blackboard_Sticker.Helpers
{
    public static class TTSHelper
    {
        public static void PlayText(string text)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                Task.Run(() => EdgeTTSPlayText(text));
            }
            else
            {
                SysTTSPlayText(text);
            }
        }

        private static async Task EdgeTTSPlayText(string text)
        {
            var voices = Edge_tts.GetVoice();
            if (voices.Count == 0)
            {
                SysTTSPlayText(text);
                return;
            }

            int voiceIndex = MainWindow.Settings.TimetableSettings.Voice;

            if (voiceIndex < 0 || voiceIndex >= voices.Count)
            {
                voiceIndex = 0;
                for (int i = 0; i < voices.Count; i++)
                {
                    if ((voices[i].Locale ?? string.Empty).Contains("zh"))
                    {
                        voiceIndex = i;
                        break;
                    }
                }

                MainWindow.Settings.TimetableSettings.Voice = voiceIndex;
            }

            var voice = voices[voiceIndex];

            PlayOption option = new()
            {
                Rate = 0,
                Text = text
            };

            await Edge_tts.PlayTextAsync(option, voice);
        }

        private static void SysTTSPlayText(string text)
        {
            SpeechSynthesizer synthesizer = null;

            try
            {
                synthesizer = new SpeechSynthesizer();
            }
            catch
            {
                // 系统 TTS 不存在
            }

            if (synthesizer != null)
            {
                synthesizer.Volume = 100;
                synthesizer.SpeakAsync(text);
            }
        }
    }
}
