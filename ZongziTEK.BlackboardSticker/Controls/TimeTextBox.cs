using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Controls;
using System;
using System.Windows;

namespace ZongziTEK.BlackboardSticker
{
    public class TimeTextBox : TextBox
    {
        public TimeTextBox()
        {
            PreviewTextInput += TimeTextBox_PreviewTextInput;
            TextChanged += TimeTextBox_TextChanged;
            SelectionChanged += TimeTextBox_SelectionChanged;
            GotFocus += TimeTextBox_GotFocus;
            PreviewMouseDown += TimeTextBox_PreviewMouseDown;

            Text = "00:00";
            TextAlignment = System.Windows.TextAlignment.Center;
        }

        private void TimeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 只允许输入数字和冒号，并禁止删除冒号
            if (!IsValidInput(e.Text) || (e.Text == ":" && Text.Contains(":")))
            {
                e.Handled = true;
            }
        }

        private void TimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int caretIndex = CaretIndex; // 记住当前光标位置

            string currentValue = Text;
            currentValue = currentValue.Replace(":", ""); // 去除冒号

            if (string.IsNullOrEmpty(currentValue))
            {
                Text = "00:00";
            }
            else if (currentValue.Length == 1 && currentValue != "0")
            {
                Text = $"0{currentValue}:00";
            }
            else if (currentValue.Length == 2 && currentValue != "00")
            {
                Text = $"{currentValue}:00";
            }
            else if (currentValue.Length > 5)
            {
                Text = currentValue.Substring(0, 5);
            }
            else
            {
                // 检查 hh 和 mm 是否超过了限制值
                string hhStr = currentValue.Substring(0, Math.Min(currentValue.Length, 2));
                string mmStr = currentValue.Substring(2, Math.Min(currentValue.Length - 2, 2));

                int hh;
                int mm;

                if (!int.TryParse(hhStr, out hh))
                {
                    hh = 0;
                }

                if (!int.TryParse(mmStr, out mm))
                {
                    mm = 0;
                }

                if (hh > 23)
                {
                    hh = 23;
                }

                if (mm > 59)
                {
                    mm = 59;
                }

                Text = $"{hh:D2}:{mm:D2}";
            }

            int textLength = Text.Length;

            // 当光标在最后一个数字的后面时，将光标移到第一个数字前面
            if (caretIndex >= textLength)
            {
                CaretIndex = 0;
            }
            // 当光标在冒号前面并且在小时数后面时，将光标移到冒号后面
            else if (caretIndex == 2)
            {
                CaretIndex = 3;
            }
            else
            {
                CaretIndex = caretIndex; // 将光标移动到原先的位置
            }
        }

        private bool IsValidInput(string text)
        {
            // 不允许输入除数字和冒号以外的字符
            foreach (char c in text)
            {
                if (!char.IsDigit(c) && c != ':')
                {
                    return false;
                }
            }

            return true;
        }

        private void TimeTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (SelectionLength != 0) SelectionLength = 0;    //防止文本被选中
        }

        private void TimeTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            CaretIndex = 0;
            CaretIndex = 0;
            CaretIndex = 0;
        }

        private void TimeTextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsKeyboardFocusWithin)
            {
                e.Handled = true;
                Focus();
            }
        }
    }
}
