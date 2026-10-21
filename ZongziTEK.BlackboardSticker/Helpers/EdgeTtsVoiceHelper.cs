using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ZongziTEK.BlackboardSticker.Models;

namespace ZongziTEK.BlackboardSticker.Helpers
{
    internal static class EdgeTtsVoiceHelper
    {
        private const string VoiceListResource = "ZongziTEK.BlackboardSticker.Resources.EdgeTts.VoiceList.json";

        private static List<EdgeTtsVoice> voices;

        public static List<EdgeTtsVoice> GetVoices()
        {
            if (voices != null)
            {
                return voices;
            }

            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(VoiceListResource))
            using (var reader = new StreamReader(stream))
            {
                voices = JsonConvert.DeserializeObject<List<EdgeTtsVoice>>(reader.ReadToEnd());
            }

            return voices;
        }
    }
}
