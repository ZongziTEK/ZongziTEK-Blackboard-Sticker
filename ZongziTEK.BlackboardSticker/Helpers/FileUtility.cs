using System;
using System.IO;

namespace ZongziTEK.BlackboardSticker
{
    public static class FileUtility
    {
        /// <summary>
        /// 复制文件夹及文件
        /// </summary>
        /// <param name="sourceFolder">原文件路径</param>
        /// <param name="destFolder">目标文件路径</param>
        /// <returns></returns>
        public static void CopyFolder(string sourceFolder, string destFolder)
        {
            try
            {
                //如果目标路径不存在,则创建目标路径
                if (!Directory.Exists(destFolder))
                {
                    Directory.CreateDirectory(destFolder);
                }
                //得到原文件根目录下的所有文件
                string[] files = Directory.GetFiles(sourceFolder);
                foreach (string file in files)
                {
                    string name = Path.GetFileName(file);
                    string dest = Path.Combine(destFolder, name);
                    // 复制文件
                    File.Copy(file, dest);
                }
                //得到原文件根目录下的所有文件夹
                string[] folders = Directory.GetDirectories(sourceFolder);
                foreach (string folder in folders)
                {
                    string dirName = folder.Split('\\')[folder.Split('\\').Length - 1];
                    string destfolder = Path.Combine(destFolder, dirName);
                    // 递归调用
                    CopyFolder(folder, destfolder);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"copy file Error:{ex.Message}\r\n source:{ex.StackTrace}");
            }
        }
    }
}
