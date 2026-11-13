using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using iNKORE.UI.WPF.Modern;
using ZongziTEK_Blackboard_Sticker.Models;
using ZongziTEK_Blackboard_Sticker.Services;
using FluentMessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace ZongziTEK_Blackboard_Sticker.Controls
{
    public partial class DutyEditor : Window
    {
        private readonly DutyRosterStore store;
        private readonly DutyRoster draft;
        private readonly int[] days = { 1, 2, 3, 4, 5, 6, 0 };

        internal DutyEditor(DutyRosterStore store, int? source)
        {
            this.store = store;
            draft = store.Load();
            InitializeComponent();
            ThemeManager.SetRequestedTheme(this, MainWindow.GetIsLightTheme() ? ElementTheme.Light : ElementTheme.Dark);
            foreach (int day in days) DayPicker.Items.Add(DutyRoster.SourceNames[day]);
            int initialDay = Array.IndexOf(days, source ?? -1);
            DayPicker.SelectedIndex = initialDay < 0 ? 0 : initialDay;
            EditWeek.SelectedIndex = CurrentWeek.SelectedIndex = draft.IsEvenWeek ? 1 : 0;
        }

        private void Selection_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (RoleRows != null && DayPicker?.SelectedIndex >= 0 && EditWeek?.SelectedIndex >= 0)
                RenderRows();
        }

        private void RenderRows()
        {
            var day = draft.Day(days[DayPicker.SelectedIndex]);
            Dictionary<Guid, string> names = EditWeek.SelectedIndex == 1 ? day.Even : day.Odd;
            RoleRows.Children.Clear();
            foreach (var role in draft.Roles)
            {
                var row = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
                var title = new DockPanel();
                var actions = new StackPanel { Orientation = Orientation.Horizontal };
                DockPanel.SetDock(actions, Dock.Right);
                int index = draft.Roles.IndexOf(role);
                actions.Children.Add(ActionButton("上移", () => MoveRole(role, index - 1), index > 0));
                actions.Children.Add(ActionButton("下移", () => MoveRole(role, index + 1), index + 1 < draft.Roles.Count));
                actions.Children.Add(ActionButton("删除", () => DeleteRole(role)));
                title.Children.Add(actions);
                title.Children.Add(EditorField("职位名称", role.Name, text => role.Name = text));
                row.Children.Add(title);
                var people = EditorField("学生姓名", names.TryGetValue(role.Id, out var value) ? value : "",
                    text => names[role.Id] = text);
                people.ToolTip = "可填写多个姓名，例如张三、李四";
                people.TextWrapping = TextWrapping.Wrap;
                people.MaxHeight = 96;
                people.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                row.Children.Add(people);
                RoleRows.Children.Add(row);
            }
            if (draft.Roles.Count == 0) RoleRows.Children.Add(new TextBlock { Text = "暂无职位。添加职位后可填写各星期的名单。" });
        }

        private static TextBox EditorField(string label, string value, Action<string> update)
        {
            var field = new TextBox { Text = value ?? "", MinHeight = 36, Margin = new Thickness(0, 0, 0, 4) };
            System.Windows.Automation.AutomationProperties.SetName(field, label);
            field.TextChanged += (sender, args) => update(field.Text);
            return field;
        }

        private static Button ActionButton(string label, Action action, bool enabled = true)
        {
            var button = new Button { Content = label, MinHeight = 36, Margin = new Thickness(4, 0, 0, 4), IsEnabled = enabled };
            button.Click += (sender, args) => action();
            return button;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            draft.Roles.Add(new DutyRole { Id = Guid.NewGuid(), Name = "新职位" });
            RenderRows();
            (RoleRows.Children[RoleRows.Children.Count - 1] as FrameworkElement)?.BringIntoView();
        }

        private void DeleteRole(DutyRole role)
        {
            if (FluentMessageBox.Show("将删除该职位及其在各星期的名单。", "删除职位", MessageBoxButton.OKCancel)
                != MessageBoxResult.OK) return;
            draft.DeleteRole(role.Id);
            RenderRows();
        }

        private void MoveRole(DutyRole role, int index)
        {
            draft.Roles.Remove(role);
            draft.Roles.Insert(index, role);
            RenderRows();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (draft.Roles.Exists(role => string.IsNullOrWhiteSpace(role.Name)))
            {
                FluentMessageBox.Show("请输入每个职位的名称。", "值日安排", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            foreach (var role in draft.Roles) role.Name = role.Name.Trim();
            draft.IsEvenWeek = CurrentWeek.SelectedIndex == 1;
            try
            {
                store.Save(draft);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                FluentMessageBox.Show(ex.Message, "值日安排保存失败", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
