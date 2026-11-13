using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32;
using ZongziTEK_Blackboard_Sticker.Helpers;
using FluentMessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace ZongziTEK_Blackboard_Sticker.Controls
{
    public partial class TextBlackboard : UserControl
    {
        private readonly DispatcherTimer saveTimer = new() { Interval = TimeSpan.FromMilliseconds(550) };
        private string filePath;
        private bool loading;
        private bool readFailed;
        private bool dirty;

        public TextBlackboard()
        {
            InitializeComponent();
            saveTimer.Tick += (sender, args) => Flush();
        }

        internal void OpenStorage(string directory)
        {
            string nextPath = Path.Combine(directory, "Blackboard.txt");
            if (string.Equals(filePath, nextPath, StringComparison.OrdinalIgnoreCase)) return;
            if (!Flush()) return;

            loading = true;
            filePath = nextPath;
            readFailed = false;
            try
            {
                Editor.Text = AtomicTextFile.Read(filePath);
                SetStatus(null);
            }
            catch (FileNotFoundException) { LoadEmpty(); }
            catch (DirectoryNotFoundException) { LoadEmpty(); }
            catch (Exception ex)
            {
                Editor.Text = "";
                readFailed = true;
                SetStatus("读取失败，原文件已保留。", ex.Message);
            }
            finally
            {
                loading = false;
                ApplyLock();
            }
        }

        private void LoadEmpty()
        {
            Editor.Text = "";
            SetStatus(null);
        }

        internal bool Flush()
        {
            saveTimer.Stop();
            if (!dirty) return true;
            try
            {
                AtomicTextFile.Write(filePath, Editor.Text);
                dirty = false;
                SetStatus(null);
                return true;
            }
            catch (Exception ex)
            {
                SetStatus("保存失败，文本仍保留在编辑区。", ex.Message);
                return false;
            }
        }

        private void SetStatus(string message, string detail = null)
        {
            Status.Text = message;
            Status.ToolTip = detail;
            Status.Visibility = message == null ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (loading || filePath == null || readFailed) return;
            dirty = true;
            saveTimer.Stop();
            saveTimer.Start();
        }

        private void ApplyLock()
        {
            bool locked = MainWindow.Settings.Blackboard.IsLocked;
            LockMenu.IsChecked = locked;
            Heading.Text = locked ? "小黑板 · 已锁定" : "小黑板";
            Editor.IsReadOnly = locked || readFailed;
            ImportMenu.IsEnabled = ClearMenu.IsEnabled = !Editor.IsReadOnly;
        }

        private void Lock_Click(object sender, RoutedEventArgs e)
        {
            bool previous = MainWindow.Settings.Blackboard.IsLocked;
            MainWindow.Settings.Blackboard.IsLocked = LockMenu.IsChecked;
            if (!MainWindow.TrySaveSettings()) MainWindow.Settings.Blackboard.IsLocked = previous;
            ApplyLock();
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            if (Editor.IsReadOnly) return;
            var dialog = new OpenFileDialog { Title = "导入小黑板", Filter = "文本文件 (*.txt)|*.txt|所有文件 (*.*)|*.*" };
            if (dialog.ShowDialog(Window.GetWindow(this)) != true) return;
            try
            {
                string replacement = AtomicTextFile.Read(dialog.FileName);
                if (Editor.Text.Length != 0 && FluentMessageBox.Show("替换当前小黑板内容？", "导入 TXT", MessageBoxButton.OKCancel)
                    != MessageBoxResult.OK) return;
                Editor.Text = replacement;
            }
            catch (Exception ex) { ShowFileError("导入失败", ex); }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog { Title = "导出小黑板", FileName = "Blackboard.txt", Filter = "文本文件 (*.txt)|*.txt" };
            if (dialog.ShowDialog(Window.GetWindow(this)) != true) return;
            try
            {
                string target = Path.GetFullPath(dialog.FileName);
                if (string.Equals(target, Path.GetFullPath(filePath), StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(target, Path.GetFullPath(filePath + ".bak"), StringComparison.OrdinalIgnoreCase))
                    throw new IOException("请选择其他导出位置，以保留小黑板数据文件及备份。");
                AtomicTextFile.Write(target, Editor.Text);
            }
            catch (Exception ex) { ShowFileError("导出失败", ex); }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            if (Editor.IsReadOnly || Editor.Text.Length == 0) return;
            if (FluentMessageBox.Show("清空当前小黑板内容？", "小黑板", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                Editor.Clear();
        }

        private static void ShowFileError(string title, Exception error) =>
            FluentMessageBox.Show(error.Message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
