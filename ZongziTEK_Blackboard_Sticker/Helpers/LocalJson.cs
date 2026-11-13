using System.IO;
using Newtonsoft.Json;

namespace ZongziTEK_Blackboard_Sticker.Helpers
{
    internal static class LocalJson
    {
        public static T ReadOrNew<T>(string path) where T : class, new()
        {
            try { return Parse<T>(AtomicTextFile.Read(path)); }
            catch (FileNotFoundException) { return new T(); }
            catch (DirectoryNotFoundException) { return new T(); }
        }

        public static T Parse<T>(string text) where T : class
        {
            return JsonConvert.DeserializeObject<T>(text,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })
                ?? throw new InvalidDataException("JSON 文件必须包含有效的数据对象。");
        }

        public static void Write<T>(string path, T value)
        {
            AtomicTextFile.Write(path, JsonConvert.SerializeObject(value, Formatting.Indented));
        }
    }
}
