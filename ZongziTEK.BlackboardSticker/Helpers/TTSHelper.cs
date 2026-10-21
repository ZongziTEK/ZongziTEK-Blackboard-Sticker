using System;
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
                Task.Run(() => SysTTSPlayText(text));
            }
        }

        private static async Task EdgeTTSPlayText(string text)
        {
            try
            {
                var voices = EdgeTtsVoiceHelper.GetVoices();
                int index = MainWindow.Settings.TimetableSettings.Voice;

                if (index < 0 || index >= voices.Count)
                {
                    throw new IndexOutOfRangeException($"语音序号 {index} 无效。");
                }

                await EdgeTtsClient.PlayTextAsync(text, voices[index]);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile("在线语音播放失败：" + ex.Message, LogHelper.LogType.Error);
                SysTTSPlayText(text);
            }
        }

        private static void SysTTSPlayText(string text)
        {
            try
            {
                using (var synthesizer = new SpeechSynthesizer())
                {
                    synthesizer.Volume = 100;
                    synthesizer.Speak(text);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile("本地语音播放失败：" + ex.Message, LogHelper.LogType.Error);
            }
        }
    }
}
