using iNKORE.UI.WPF.Controls;
using iNKORE.UI.WPF.Modern;
using iNKORE.UI.WPF.Modern.Common.IconKeys;
using iNKORE.UI.WPF.Modern.Controls;
using IWshRuntimeLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Drawing = System.Drawing;
using File = System.IO.File;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace ZongziTEK.BlackboardSticker
{
    /// <summary>
    /// LauncherEditor.xaml 的交互逻辑
    /// </summary>
    public partial class LauncherEditor : Window
    {
        public LauncherEditor()
        {
            InitializeComponent();
            Task.Run(() =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    LoadList();
                });
            });
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        }

        private bool isAnimationDisabledOnce = false;

        private async void LoadList()
        {
            //ListScrollViewer.Visibility = Visibility.Collapsed;
            //ListProgressBar.Visibility = Visibility.Visible;

            Dictionary<string, Drawing.Bitmap> fileInfo = new();
            string LinkPath = AppDomain.CurrentDomain.BaseDirectory + @"\LauncherLinks\";
            if (!new DirectoryInfo(LinkPath).Exists)
            {
                try { new DirectoryInfo(LinkPath).Create(); }
                catch { }
            }
            try
            {
                string[] files = Directory.GetFiles(LinkPath);
                foreach (string filePath in files)
                {
                    if (!filePath.EndsWith(".lnk"))
                    {
                        continue;
                    }
                    WshShell shell = new();
                    IWshShortcut wshShortcut = (IWshShortcut)shell.CreateShortcut(filePath);
                    /*if (!File.Exists(wshShortcut.TargetPath))
                    {
                        continue;
                    }*/
                    try
                    {
#pragma warning disable CS8602 // 解引用可能出现空引用。
                        Drawing.Bitmap icon = Drawing.Icon.ExtractAssociatedIcon(wshShortcut.TargetPath).ToBitmap();
#pragma warning restore CS8602 // 解引用可能出现空引用。
                        //fileInfo.Add(wshShortcut.TargetPath, icon);
                        fileInfo.Add(filePath, icon);
                    }
                    catch { }
                }

                foreach (KeyValuePair<string, Drawing.Bitmap> file in fileInfo)
                {
                    // 列表项
                    Border BorderItem = new()
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        Height = 48,
                        Background = (Brush)FindResource(ThemeKeys.CardBackgroundFillColorDefaultBrushKey),
                        CornerRadius = new CornerRadius(4),
                        BorderBrush = (Brush)FindResource("BorderBrush"),
                        BorderThickness = new Thickness(1)
                    };

                    // 按钮里面的布局
                    SimpleStackPanel ContentStackPanel = new()
                    {
                        Spacing = 8,
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    Grid ContentGrid = new()
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        Margin = new(16, 8, 16, 8)
                    };

                    // 图标
                    Image image = new()
                    {
                        Height = 19
                    };
                    BitmapSource bitmapSource = System.Windows.Interop.Imaging.
                        CreateBitmapSourceFromHBitmap(file.Value.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    image.Source = bitmapSource;

                    // 名字
                    TextBlock textBlockFileName = new()
                    {
                        Text = Path.GetFileName(file.Key),
                        Visibility = Visibility.Collapsed
                    };

                    TextBlock textBlockLinkName = new()
                    {
                        Text = Path.GetFileName(file.Key).Remove(Path.GetFileName(file.Key).LastIndexOf("."), 4)
                    };

                    // 删除按钮
                    Button DeleteButton = new()
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Width = 28,
                        Height = 28,
                        Background = Brushes.Transparent,
                        Padding = new Thickness(0),
                        BorderThickness = new Thickness(0),
                        Margin = new(0, 0, -6, 0)
                    };

                    FontIcon DeleteIcon = new()
                    {
                        Icon = FluentSystemIcons.Delete_20_Regular
                    };

                    DeleteButton.Content = DeleteIcon;


                    // 开始组装按钮
                    ContentStackPanel.Children.Add(image);
                    ContentStackPanel.Children.Add(textBlockFileName);
                    ContentStackPanel.Children.Add(textBlockLinkName);
                    BorderItem.Child = ContentGrid;
                    ContentGrid.Children.Add(ContentStackPanel);
                    ContentGrid.Children.Add(DeleteButton);
                    DeleteButton.Click += DeleteButton_Click;

                    // 往编辑器里面添加按钮
                    ListStackPanel.Children.Add(BorderItem);

                    if (!isAnimationDisabledOnce && MainWindow.Settings.Look.IsAnimationEnhanced)
                    {
                        DoubleAnimation opacityAnimation = new()
                        {
                            From = 0,
                            To = 1,
                            Duration = TimeSpan.FromMilliseconds(500),
                            EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
                        };
                        BorderItem.BeginAnimation(OpacityProperty, opacityAnimation);

                        ThicknessAnimation marginAnimation = new()
                        {
                            From = new Thickness(0, 24, 0, BorderItem.Margin.Bottom),
                            To = new Thickness(0, 0, 0, BorderItem.Margin.Bottom),
                            Duration = TimeSpan.FromMilliseconds(500),
                            EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
                        };
                        BorderItem.BeginAnimation(MarginProperty, marginAnimation);

                        await Task.Delay(25);
                    }
                }

                ListScrollViewer.Visibility = Visibility.Visible;
                //ListProgressBar.Visibility = Visibility.Collapsed;
            }
            catch (Exception e)
            {
                MessageBox.Show("加载列表时出现错误：\r\n" + e.Message);
            }
            isAnimationDisabledOnce = false;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("确认删除吗", "编辑启动台", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                string LinkPath = AppDomain.CurrentDomain.BaseDirectory + @"LauncherLinks\";
                string filePath = LinkPath + ((TextBlock)((SimpleStackPanel)((Grid)((Button)sender).Parent).Children[0]).Children[2]).Text + ".lnk";
                try
                {
                    File.Delete(filePath);
                    isAnimationDisabledOnce = true;
                    ButtonRefresh_Click(null, null);
                }
                catch (Exception ex) { MessageBox.Show("删除该项时出现错误：\r\n" + ex.Message); }
            }
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            ListStackPanel.Children.Clear();
            Task.Run(() =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    LoadList();
                });
            });
        }

        //New

        private void ButtonNew_Click(object sender, RoutedEventArgs e)
        {
            TextBoxFilePath.Text = "";
            TextBoxLinkName.Text = "";
            ListScrollViewer.Visibility = Visibility.Collapsed;

            ThicknessAnimation gridInsertMarginAnimation = new()
            {
                From = new Thickness(0, 72, 0, -72),
                To = new Thickness(0),
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };
            GridInsert.BeginAnimation(MarginProperty, gridInsertMarginAnimation);

            DoubleAnimation gridInsertOpacityAnimation = new()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };
            GridInsert.BeginAnimation(OpacityProperty, gridInsertOpacityAnimation);

            GridInsert.Visibility = Visibility.Visible;
        }
        private void ButtonCancelNew_Click(object sender, RoutedEventArgs e)
        {
            HideInsertPanel();
        }

        private void ButtonSaveNew_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(AppDomain.CurrentDomain.BaseDirectory + @"LauncherLinks\" + TextBoxLinkName.Text + ".lnk");
                shortcut.TargetPath = TextBoxFilePath.Text;
                shortcut.WorkingDirectory = Path.GetFullPath(TextBoxFilePath.Text);
                shortcut.Description = "ZongziTEK";
                shortcut.Save();
                HideInsertPanel();
                ButtonRefresh_Click(null, null);
            }
            catch (Exception ex) { MessageBox.Show("新建项时出现错误：\r\n" + ex.Message); }
        }

        private void HideInsertPanel()
        {
            GridInsert.Visibility = Visibility.Collapsed;
            ListScrollViewer.Visibility = Visibility.Visible;
        }

        private void ButtonBrowsePath_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog fileBrowser = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "选择文件",
                Multiselect = false,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };
            fileBrowser.ShowDialog();
            TextBoxFilePath.Text = fileBrowser.FileName;
        }

        private void TextBoxFilePath_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                TextBoxLinkName.Text = Path.GetFileNameWithoutExtension(TextBoxFilePath.Text);
            }
            catch (Exception) { }
        }

        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            isAnimationDisabledOnce = true;
            ButtonRefresh_Click(null, null);
        }
    }
}
