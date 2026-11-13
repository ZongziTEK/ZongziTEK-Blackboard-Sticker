using System;
using System.IO;
using System.Text;

namespace ZongziTEK_Blackboard_Sticker.Helpers
{
    internal static class AtomicTextFile
    {
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false, true);

        public static string Read(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            Encoding encoding = Utf8;
            int offset = 0;
            if (HasPrefix(bytes, 0xFF, 0xFE, 0, 0))
            {
                encoding = new UTF32Encoding(false, false, true);
                offset = 4;
            }
            else if (HasPrefix(bytes, 0, 0, 0xFE, 0xFF))
            {
                encoding = new UTF32Encoding(true, false, true);
                offset = 4;
            }
            else if (HasPrefix(bytes, 0xFF, 0xFE))
            {
                encoding = new UnicodeEncoding(false, false, true);
                offset = 2;
            }
            else if (HasPrefix(bytes, 0xFE, 0xFF))
            {
                encoding = new UnicodeEncoding(true, false, true);
                offset = 2;
            }
            else if (HasPrefix(bytes, 0xEF, 0xBB, 0xBF))
            {
                offset = 3;
            }

            string text = encoding.GetString(bytes, offset, bytes.Length - offset);
            if (text.IndexOf('\0') >= 0)
                throw new InvalidDataException("文本包含 NUL 字符。请使用 UTF-8 或带 BOM 的 Unicode 文本。");
            return text;
        }

        private static bool HasPrefix(byte[] bytes, params byte[] prefix)
        {
            if (bytes.Length < prefix.Length) return false;
            for (int index = 0; index < prefix.Length; index++)
                if (bytes[index] != prefix[index]) return false;
            return true;
        }

        public static void Write(string path, string text)
        {
            path = Path.GetFullPath(path);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                byte[] bytes = Utf8.GetBytes(text);
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }
                if (File.Exists(path))
                    File.Replace(temporary, path, path + ".bak");
                else
                    File.Move(temporary, path);
            }
            finally
            {
                if (File.Exists(temporary)) File.Delete(temporary);
            }
        }
    }
}
