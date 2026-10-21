using NAudio.Wave;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZongziTEK.BlackboardSticker.Models;

namespace ZongziTEK.BlackboardSticker.Helpers
{
    internal static class EdgeTtsClient
    {
        private const string ChromiumVersion = "143.0.3650.75";
        private const string ChromiumMajorVersion = "143";
        private const string TrustedClientToken = "6A5AA1D4EAFF4E9FB37E23D68491D6F4";

        private static readonly Dictionary<string, string> Headers = new Dictionary<string, string>
        {
            { "User-Agent", $"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/{ChromiumMajorVersion}.0.0.0 Safari/537.36 Edg/{ChromiumMajorVersion}.0.0.0" },
            { "Accept-Language", "en-US,en;q=0.9" },
            { "Pragma", "no-cache" },
            { "Cache-Control", "no-cache" },
            { "Origin", "chrome-extension://jdiccldimpdaibmpdkjnbmckianbfold" },
            { "Accept", "*/*" }
        };

        public static async Task PlayTextAsync(string text, EdgeTtsVoice voice, int rate = 0, float volume = 1f)
        {
            List<byte> audio = await InvokeAsync(text, voice, rate, volume);

            if (audio == null || audio.Count == 0)
            {
                throw new InvalidOperationException("Edge 在线语音未返回音频数据。");
            }

            await PlayAudioAsync(audio.ToArray(), volume);
        }

        private static async Task<List<byte>> InvokeAsync(string text, EdgeTtsVoice voice, int rate, float volume)
        {
            RemoveRestrictedHeaders();

            var audio = new List<byte>();
            string requestId = Guid.NewGuid().ToString("N");
            string token = EdgeTtsTokenHelper.Generate();

            var uri = new Uri(
                "wss://speech.platform.bing.com/consumer/speech/synthesize/readaloud/edge/v1" +
                $"?TrustedClientToken={TrustedClientToken}" +
                $"&ConnectionId={requestId}" +
                $"&Sec-MS-GEC={token}" +
                $"&Sec-MS-GEC-Version=2-{ChromiumVersion}");

            var socket = new ClientWebSocket();
            socket.Options.Cookies = new CookieContainer();

            foreach (KeyValuePair<string, string> header in Headers)
            {
                socket.Options.SetRequestHeader(header.Key, header.Value);
            }

            socket.Options.Cookies.SetCookies(
                new Uri("https://speech.platform.bing.com"),
                $"muid={Guid.NewGuid().ToString("N").ToUpperInvariant()};");

            using (var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20)))
            {
                try
                {
                    await ConnectAsync(socket, uri, timeout.Token, TimeSpan.FromSeconds(8));

                    string outputFormat = string.IsNullOrEmpty(voice.SuggestedCodec)
                        ? "audio-24khz-48kbitrate-mono-mp3"
                        : voice.SuggestedCodec;
                    await SendAsync(socket, CreateSpeechConfigMessage(outputFormat), timeout.Token);
                    await SendAsync(socket, CreateSsmlMessage(requestId, voice, rate, volume, text), timeout.Token);

                    var buffer = new byte[32 * 1024];
                    var messageBuffer = new List<byte>();

                    while (socket.State == WebSocketState.Open || socket.State == WebSocketState.Connecting)
                    {
                        if (socket.State == WebSocketState.Connecting)
                        {
                            await Task.Delay(10, timeout.Token);
                            continue;
                        }

                        WebSocketReceiveResult result;
                        try
                        {
                            result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), timeout.Token);
                        }
                        catch (WebSocketException)
                        {
                            break;
                        }

                        messageBuffer.AddRange(buffer.Take(result.Count));

                        if (!result.EndOfMessage)
                        {
                            continue;
                        }

                        byte[] message = messageBuffer.ToArray();
                        messageBuffer.Clear();

                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            if (Encoding.UTF8.GetString(message).Contains("Path:turn.end"))
                            {
                                break;
                            }
                        }
                        else if (result.MessageType == WebSocketMessageType.Binary)
                        {
                            AppendAudio(message, audio);
                        }
                        else if (result.MessageType == WebSocketMessageType.Close)
                        {
                            break;
                        }
                    }
                }
                finally
                {
                    if (socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseReceived)
                    {
                        try
                        {
                            using (var closeTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(2)))
                            {
                                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "NormalClosure", closeTimeout.Token);
                            }
                        }
                        catch { }
                    }

                    socket.Dispose();
                }
            }

            return audio;
        }

        private static async Task ConnectAsync(ClientWebSocket socket, Uri uri, CancellationToken cancellationToken, TimeSpan timeout)
        {
            Task connectTask = socket.ConnectAsync(uri, cancellationToken);
            Task completedTask = await Task.WhenAny(connectTask, Task.Delay(timeout));

            if (completedTask != connectTask)
            {
                socket.Abort();

                try
                {
                    await connectTask;
                }
                catch
                {
                }

                throw new TimeoutException("连接 Edge 语音服务器超时。");
            }

            await connectTask;
        }

        private static Task PlayAudioAsync(byte[] audio, float volume)
        {
            return Task.Run(() =>
            {
                using (var stream = new MemoryStream(audio))
                using (var reader = new Mp3FileReader(stream))
                using (var output = new WaveOutEvent())
                {
                    var completion = new TaskCompletionSource<bool>();

                    output.PlaybackStopped += (sender, e) =>
                    {
                        if (e.Exception != null)
                        {
                            completion.TrySetException(e.Exception);
                        }
                        else
                        {
                            completion.TrySetResult(true);
                        }
                    };

                    output.Init(reader);
                    output.Volume = volume;
                    output.Play();

                    completion.Task.GetAwaiter().GetResult();
                }
            });
        }

        private static async Task SendAsync(ClientWebSocket socket, string message, CancellationToken cancellationToken)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            await socket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, cancellationToken);
        }

        private static void AppendAudio(byte[] message, List<byte> audio)
        {
            if (message.Length < 2)
            {
                return;
            }

            int headerLength = (message[0] << 8) | message[1];
            int audioStart = 2 + headerLength;

            if (message.Length <= audioStart)
            {
                return;
            }

            byte[] audioData = new byte[message.Length - audioStart];
            Array.Copy(message, audioStart, audioData, 0, audioData.Length);
            audio.AddRange(audioData);
        }

        private static string CreateSpeechConfigMessage(string outputFormat)
        {
            return "Content-Type:application/json; charset=utf-8\r\n" +
                   "Path:speech.config\r\n\r\n" +
                   "{\"context\":{\"synthesis\":{\"audio\":{\"metadataoptions\":{" +
                   "\"sentenceBoundaryEnabled\":\"false\",\"wordBoundaryEnabled\":\"false\"}," +
                   "\"outputFormat\":\"" + outputFormat + "\"}}}}";
        }

        private static string CreateSsmlMessage(string requestId, EdgeTtsVoice voice, int rate, float volume, string text)
        {
            string timestamp = DateTimeOffset.UtcNow.ToString(
                "ddd MMM dd yyyy HH:mm:ss 'GMT+0000 (Coordinated Universal Time)'",
                CultureInfo.InvariantCulture);

            string ssml =
                $"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='{voice.Locale}'>" +
                $"<voice name='{voice.Name}'><prosody pitch='+0Hz' rate='{FormatPercentage(rate)}' volume='{(int)(volume * 100)}'>" +
                $"{SecurityElement.Escape(text)}</prosody></voice></speak>";

            return $"X-RequestId:{requestId}\r\n" +
                   "Content-Type:application/ssml+xml\r\n" +
                   $"X-Timestamp:{timestamp}Z\r\n" +
                   "Path:ssml\r\n\r\n" +
                   ssml;
        }

        private static string FormatPercentage(int value)
        {
            return value.ToString("+#;-#;0", CultureInfo.InvariantCulture) + "%";
        }

        private static void RemoveRestrictedHeaders()
        {
            Assembly assembly = typeof(HttpWebRequest).Assembly;
            Type headerInfoTableType = assembly.GetType("System.Net.HeaderInfoTable");
            FieldInfo headerHashTableField = headerInfoTableType?.GetField(
                "HeaderHashTable", BindingFlags.Static | BindingFlags.NonPublic);

            if (headerHashTableField?.GetValue(null) is Hashtable headerHashTable)
            {
                FieldInfo isRequestRestrictedField = assembly.GetType("System.Net.HeaderInfo")?.GetField(
                    "IsRequestRestricted", BindingFlags.Instance | BindingFlags.NonPublic);

                if (isRequestRestrictedField != null)
                {
                    foreach (DictionaryEntry entry in headerHashTable)
                    {
                        isRequestRestrictedField.SetValue(entry.Value, false);
                    }
                }
            }
        }
    }
}
