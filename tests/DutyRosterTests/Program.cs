using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ZongziTEK_Blackboard_Sticker.Helpers;
using ZongziTEK_Blackboard_Sticker.Models;
using ZongziTEK_Blackboard_Sticker.Services;

internal static class Program
{
    private static int checks;

    [STAThread]
    private static void Main()
    {
        string directory = Path.Combine(Path.GetTempPath(), "Blackboard-Sticker-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            CheckDutyAndTemporary(directory);
            CheckOldData(directory);
            CheckTextStorage(directory);
            CheckLauncherLayout();
            CheckScheduleClock();
            Console.WriteLine($"Passed {checks} checks.");
        }
        finally { Directory.Delete(directory, true); }
    }

    private static void CheckDutyAndTemporary(string directory)
    {
        var store = new DutyRosterStore(directory);
        var roster = store.Load();
        Require(!roster.IsEvenWeek, "default odd week");
        var role = new DutyRole { Id = Guid.NewGuid(), Name = "擦黑板" };
        roster.Roles.Add(role);
        roster.Day(5).Odd[role.Id] = "张三、李四";
        roster.Day(5).Even[role.Id] = "王五";
        roster.Day(2).Even[role.Id] = "小赵";
        roster.IsEvenWeek = true;
        store.Save(roster);

        roster = new DutyRosterStore(directory).Load();
        Require(roster.IsEvenWeek, "manual week survives restart");
        Require(roster.Present(5, null).Rows.Single().Names == "王五", "Friday follows active week");
        Require(roster.Present(2, null).Rows.Single().Names == "小赵", "Tuesday follows shown source");
        Require(roster.Present(4, null).Rows.Single().Names == "未填写", "empty role is labelled");
        Require(roster.Present(null, null).EmptyText.Contains("未关联"), "shared timetable is unassociated");

        string path = Path.Combine(directory, "Timetable.json");
        var timetable = new Timetable();
        timetable.Friday.Add(new Lesson("物理", TimeSpan.FromHours(8), TimeSpan.FromHours(9), true, false));
        timetable.ReplaceTemporary(timetable.Friday, roster.Capture(5));
        LocalJson.Write(path, timetable);
        timetable.Friday[0].Subject = "化学";
        Require(timetable.Temp[0].Subject == "物理", "temporary lesson is independent");
        timetable = LocalJson.ReadOrNew<Timetable>(path);
        Require(timetable.Temp[0].Subject == "物理", "temporary lesson survives restart");
        Require(roster.Present(7, timetable.TemporaryDuty).Title == "临时 · 双周", "temporary heading");
        Require(roster.Present(7, timetable.TemporaryDuty).Rows.Single().Names == "王五", "temporary even list");

        roster.IsEvenWeek = false;
        roster.Roles[0].Name = "整理黑板";
        Require(roster.Present(5, null).Rows.Single().Names == "张三、李四", "renaming retains names");
        roster.Day(5).Odd[role.Id] = "新名单";
        roster.DeleteRole(role.Id);
        Require(roster.Day(5).Odd.Count == 0 && roster.Day(5).Even.Count == 0, "deletion removes active assignments");
        var temp = roster.Present(7, timetable.TemporaryDuty).Rows.Single();
        Require(temp.Names == "张三、李四" && temp.Role == "擦黑板", "temporary odd snapshot survives source edits");

        var replacement = new DutySnapshot();
        timetable.ReplaceTemporary(timetable.Tuesday, replacement);
        using (File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            Expect<IOException>(() => LocalJson.Write(path, timetable), "locked schedule write reports failure");
        var previous = LocalJson.ReadOrNew<Timetable>(path);
        Require(previous.Temp.Single().Subject == "物理" && previous.TemporaryDuty.Rows.Count == 1, "failed write preserves both halves");
        LocalJson.Write(path, timetable);
        var updated = LocalJson.ReadOrNew<Timetable>(path);
        Require(updated.Temp.Count == 0 && updated.TemporaryDuty.Rows.Count == 0, "overwrite replaces both halves");
        Require(LocalJson.ReadOrNew<Timetable>(path + ".bak").Temp.Count == 1, "overwrite retains previous pair in backup");

        var curriculum = new Curriculums();
        curriculum.Friday.Curriculums = "语文";
        curriculum.ReplaceTemporary(5, previous.TemporaryDuty);
        curriculum.Friday.Curriculums = "历史";
        string simplePath = Path.Combine(directory, "Curriculums.json");
        LocalJson.Write(simplePath, curriculum);
        curriculum = LocalJson.ReadOrNew<Curriculums>(simplePath);
        Require(curriculum.GetDay(7) == "语文" && curriculum.TemporaryDuty.Rows.Single().Even == "王五",
            "simple curriculum copies both halves independently");
    }

    private static void CheckOldData(string directory)
    {
        var old = LocalJson.Parse<Timetable>("{\"Monday\":null,\"Temp\":[null,{\"Subject\":\"英语\"}]}");
        old.Normalize();
        Require(old.Temp.Single().Subject == "英语" && old.TemporaryDuty == null, "legacy temporary courses preserved");
        Require(old.Monday.Count == 0, "null days use empty defaults");
        Require(new DutyRoster().Present(7, old.TemporaryDuty).Rows.Count == 0, "legacy temporary duties are empty");
        Require(LocalJson.Parse<Settings>("{\"Look\":null}").Look.ShowLauncher, "old settings keep launcher on");
        string settingsPath = Path.Combine(directory, "Settings.json");
        var settings = new Settings();
        settings.Look.ShowLauncher = false;
        LocalJson.Write(settingsPath, settings);
        Require(!LocalJson.ReadOrNew<Settings>(settingsPath).Look.ShowLauncher, "launcher switch survives restart");

        var store = new DutyRosterStore(directory);
        string rosterPath = Path.Combine(directory, "DutyRoster.json");
        AtomicTextFile.Write(rosterPath, "{\"Version\":1,\"Roles\":null,\"Days\":null}");
        Require(store.Load().Roles.Count == 0, "null roster collections use empty defaults");
        AtomicTextFile.Write(rosterPath, "{\"Version\":2}");
        Expect<InvalidDataException>(() => store.Load(), "unsupported roster version rejected");
        AtomicTextFile.Write(rosterPath, "{broken");
        Expect<Newtonsoft.Json.JsonException>(() => store.Load(), "malformed JSON rejected");
    }

    private static void CheckTextStorage(string directory)
    {
        string path = Path.Combine(directory, "Blackboard.txt");
        const string chinese = "今日值日\r\n第二行：张三";
        AtomicTextFile.Write(path, chinese);
        Require(AtomicTextFile.Read(path) == chinese, "Chinese multiline UTF-8 round trip");
        foreach (Encoding encoding in new Encoding[]
        {
            new UTF8Encoding(true), new UnicodeEncoding(false, true), new UnicodeEncoding(true, true),
            new UTF32Encoding(false, true), new UTF32Encoding(true, true)
        })
        {
            string import = Path.Combine(directory, "import.txt");
            File.WriteAllBytes(import, encoding.GetPreamble().Concat(encoding.GetBytes(chinese)).ToArray());
            Require(AtomicTextFile.Read(import) == chinese, "BOM decoding " + encoding.WebName);
        }
        string invalid = Path.Combine(directory, "invalid.txt");
        File.WriteAllBytes(invalid, new byte[] { 0xFF, 0xA0 });
        Expect<DecoderFallbackException>(() => AtomicTextFile.Read(invalid), "invalid UTF-8 is rejected");
        Require(File.ReadAllBytes(invalid).SequenceEqual(new byte[] { 0xFF, 0xA0 }), "failed read preserves bytes");

        using (File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            Expect<IOException>(() => AtomicTextFile.Write(path, "未保存"), "failed text save reported");
        Require(AtomicTextFile.Read(path) == chinese, "failed save preserves previous text");
        AtomicTextFile.Write(path, "新内容");
        Require(AtomicTextFile.Read(path + ".bak") == chinese, "backup retains previous text");
        Require(!Directory.EnumerateFiles(directory, "*.tmp").Any(), "temporary write files removed");
    }

    private static void CheckLauncherLayout()
    {
        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition());
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var timetable = new Border();
        var launcher = new Border { Height = 60 };
        Grid.SetRow(launcher, 1);
        grid.Children.Add(timetable);
        grid.Children.Add(launcher);
        var bounds = new Rect(0, 0, 260, 240);

        LauncherLayout.Apply(timetable, launcher, true);
        grid.Measure(bounds.Size);
        grid.Arrange(bounds);
        double visibleHeight = timetable.ActualHeight;
        LauncherLayout.Apply(timetable, launcher, false);
        grid.Measure(bounds.Size);
        grid.Arrange(bounds);
        Require(launcher.Visibility == Visibility.Collapsed && timetable.ActualHeight > visibleHeight,
            "collapsed launcher releases measured timetable space");
        LauncherLayout.Apply(timetable, launcher, true);
        grid.Measure(bounds.Size);
        grid.Arrange(bounds);
        Require(timetable.ActualHeight == visibleHeight, "launcher layout restores");
    }

    private static void CheckScheduleClock()
    {
        var monday = new DateTime(2026, 11, 9, 23, 59, 59);
        Require(ScheduleClock.DayAfterDateChange(monday, monday.AddHours(-1)) == null,
            "manual or temporary selection survives same-day refresh");
        Require(ScheduleClock.DayAfterDateChange(monday, monday.AddSeconds(2)) == 2, "rollover selects Tuesday");
        Require(ScheduleClock.DayAfterDateChange(monday, monday.AddDays(7)) == 1, "one-week sleep detects date change");
        var boundary = TimeSpan.FromHours(8);
        Require(ScheduleClock.CrossedReminder(boundary.Add(TimeSpan.FromSeconds(-2)), boundary.Add(TimeSpan.FromSeconds(2)), boundary),
            "reminder survives skipped target second");
        Require(!ScheduleClock.CrossedReminder(boundary, boundary.Add(TimeSpan.FromSeconds(2)), boundary),
            "reminder is not repeated");
        Require(!ScheduleClock.CrossedReminder(boundary.Add(TimeSpan.FromHours(-1)), boundary, boundary),
            "sleep does not replay stale reminders");
    }

    private static void Require(bool condition, string label)
    {
        if (!condition) throw new InvalidOperationException(label);
        checks++;
        Console.WriteLine("PASS " + label);
    }

    private static void Expect<T>(Action action, string label) where T : Exception
    {
        try { action(); }
        catch (T) { Require(true, label); return; }
        throw new InvalidOperationException(label);
    }
}
