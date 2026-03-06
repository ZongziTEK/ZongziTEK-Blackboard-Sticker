using Edge_tts_sharp;
using Edge_tts_sharp.Model;
using System.Net.NetworkInformation;
using System.Speech.Synthesis;
using System.Threading.Tasks;

namespace ZongziTEK.BlackboardSticker.Helpers
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
            var voice = Edge_tts.GetVoice()[MainWindow.Settings.TimetableSettings.Voice];

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
