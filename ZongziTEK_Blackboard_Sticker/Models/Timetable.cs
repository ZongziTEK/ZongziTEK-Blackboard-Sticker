using CsesSharp;
using CsesSharp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZongziTEK.BlackboardSticker.Models
{
    public enum TimetableConditionType { Weekdays, Dates, Manual, WeekParity }
    public enum WeekParity { Odd, Even }

    public class TimetableSelection
    {
        public bool IsAutomatic { get; set; } = true;
        public string TimetableId { get; set; } = "";
    }

    public class Timetable
    {
        public List<TimetableDefinition> Schedules { get; set; } = new();

        public List<Lesson> Monday { get; set; } = new();
        public List<Lesson> Tuesday { get; set; } = new();
        public List<Lesson> Wednesday { get; set; } = new();
        public List<Lesson> Thursday { get; set; } = new();
        public List<Lesson> Friday { get; set; } = new();
        public List<Lesson> Saturday { get; set; } = new();
        public List<Lesson> Sunday { get; set; } = new();
        public List<Lesson> Temp { get; set; } = new();

        [JsonIgnore]
        public bool IsLegacyFormat => (Schedules == null || Schedules.Count == 0) &&
            new[] { Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday, Temp }.Any(x => x != null && x.Count > 0);

        public void EnsureMigrated()
        {
            Schedules ??= new List<TimetableDefinition>();
            Monday ??= new List<Lesson>();
            Tuesday ??= new List<Lesson>();
            Wednesday ??= new List<Lesson>();
            Thursday ??= new List<Lesson>();
            Friday ??= new List<Lesson>();
            Saturday ??= new List<Lesson>();
            Sunday ??= new List<Lesson>();
            Temp ??= new List<Lesson>();
            foreach (var schedule in Schedules)
            {
                schedule.Monday ??= new List<Lesson>();
                schedule.Tuesday ??= new List<Lesson>();
                schedule.Wednesday ??= new List<Lesson>();
                schedule.Thursday ??= new List<Lesson>();
                schedule.Friday ??= new List<Lesson>();
                schedule.Saturday ??= new List<Lesson>();
                schedule.Sunday ??= new List<Lesson>();
                schedule.Weekdays ??= new List<DayOfWeek>();
                schedule.Dates ??= new List<DateTime>();
            }
            if (Schedules.Count > 0) return;
            var days = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday };
            var lists = new[] { Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday };
            for (int i = 0; i < days.Length; i++)
            {
                var schedule = new TimetableDefinition
                {
                    Name = $"周{ChineseDay(days[i])}",
                    ConditionType = TimetableConditionType.Weekdays,
                    Weekdays = new List<DayOfWeek> { days[i] }
                };
                schedule.GetLessons(days[i]).AddRange(lists[i]);
                Schedules.Add(schedule);
            }
            var temp = new TimetableDefinition { Name = "临时课表", ConditionType = TimetableConditionType.Manual };
            temp.Monday.AddRange(Temp);
            Schedules.Add(temp);
        }

        public TimetableDefinition ResolveAutomatic(DateTime now, DateTime? schoolStartDate)
        {
            EnsureMigrated();
            return Schedules.Select((x, i) => new { Schedule = x, Index = i })
                .Where(x => x.Schedule.IsEffective(now, schoolStartDate))
                .OrderBy(x => x.Schedule.ConditionPriority).ThenBy(x => x.Index)
                .Select(x => x.Schedule).LastOrDefault();
        }

        public List<Lesson> GetCurrentTimetable(DateTime now, TimetableSelection selection, DateTime? schoolStartDate)
        {
            EnsureMigrated();
            var schedule = selection != null && !selection.IsAutomatic
                ? Schedules.FirstOrDefault(x => x.Id == selection.TimetableId)
                : ResolveAutomatic(now, schoolStartDate);
            if (schedule == null) return new List<Lesson>();
            return schedule.ConditionType == TimetableConditionType.Manual
                ? schedule.Monday
                : schedule.GetLessons(now.DayOfWeek);
        }

        public static void Sort(Timetable timetable)
        {
            timetable.EnsureMigrated();
            foreach (var schedule in timetable.Schedules)
                foreach (var day in schedule.GetAllDays())
                    day.Sort((x, y) => x.StartTime.CompareTo(y.StartTime));
        }

        private static string ChineseDay(DayOfWeek day) => day switch
        {
            DayOfWeek.Monday => "一", DayOfWeek.Tuesday => "二", DayOfWeek.Wednesday => "三",
            DayOfWeek.Thursday => "四", DayOfWeek.Friday => "五", DayOfWeek.Saturday => "六", _ => "日"
        };

        public static Timetable ConvertFromCses(string csesText)
        {
            var timetable = new Timetable();
            var profile = CsesLoader.LoadFromYamlString(csesText);
            if (profile == null) return timetable;
            foreach (Schedule source in profile.Schedules)
            {
                var schedule = new TimetableDefinition
                {
                    Name = $"周{ChineseDay(source.EnableDay)}",
                    ConditionType = TimetableConditionType.Weekdays,
                    Weekdays = new List<DayOfWeek> { source.EnableDay }
                };
                foreach (ClassInfo info in source.Classes)
                    schedule.GetLessons(source.EnableDay).Add(Lesson.ConvertFromCsesClass(info));
                timetable.Schedules.Add(schedule);
            }
            Sort(timetable);
            return timetable;
        }
    }

    public class TimetableDefinition
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = "新课表";
        public TimetableConditionType ConditionType { get; set; } = TimetableConditionType.Weekdays;
        public List<DayOfWeek> Weekdays { get; set; } = new();
        public List<DateTime> Dates { get; set; } = new();
        public WeekParity WeekParity { get; set; } = WeekParity.Odd;
        public int ConditionPriority => ConditionType switch
        {
            TimetableConditionType.Dates => 3,
            TimetableConditionType.WeekParity => 2,
            TimetableConditionType.Weekdays => 1,
            _ => 0
        };
        public List<Lesson> Monday { get; set; } = new();
        public List<Lesson> Tuesday { get; set; } = new();
        public List<Lesson> Wednesday { get; set; } = new();
        public List<Lesson> Thursday { get; set; } = new();
        public List<Lesson> Friday { get; set; } = new();
        public List<Lesson> Saturday { get; set; } = new();
        public List<Lesson> Sunday { get; set; } = new();

        public IEnumerable<List<Lesson>> GetAllDays() => new[] { Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday };
        public List<Lesson> GetLessons(DayOfWeek day) => day switch
        {
            DayOfWeek.Monday => Monday, DayOfWeek.Tuesday => Tuesday, DayOfWeek.Wednesday => Wednesday,
            DayOfWeek.Thursday => Thursday, DayOfWeek.Friday => Friday, DayOfWeek.Saturday => Saturday, _ => Sunday
        };

        public bool IsEffective(DateTime date, DateTime? schoolStartDate)
        {
            Monday ??= new List<Lesson>();
            Tuesday ??= new List<Lesson>();
            Wednesday ??= new List<Lesson>();
            Thursday ??= new List<Lesson>();
            Friday ??= new List<Lesson>();
            Saturday ??= new List<Lesson>();
            Sunday ??= new List<Lesson>();
            Weekdays ??= new List<DayOfWeek>();
            Dates ??= new List<DateTime>();
            if (ConditionType == TimetableConditionType.Manual) return false;
            if (ConditionType == TimetableConditionType.Weekdays) return Weekdays.Contains(date.DayOfWeek);
            if (ConditionType == TimetableConditionType.Dates) return Dates.Any(x => x.Date == date.Date);
            if (!Weekdays.Contains(date.DayOfWeek) || !schoolStartDate.HasValue) return false;
            int daysFromMonday = ((int)schoolStartDate.Value.DayOfWeek + 6) % 7;
            DateTime firstWeek = schoolStartDate.Value.Date.AddDays(-daysFromMonday);
            int week = (int)((date.Date - firstWeek).TotalDays / 7) + 1;
            return (week % 2 == 1) == (WeekParity == WeekParity.Odd);
        }
    }

    public class Lesson
    {
        public string Subject { get; set; } = "";
        public TimeSpan StartTime { get; set; } = new();
        public TimeSpan EndTime { get; set; } = new();
        public bool IsSplitBelow { get; set; }
        public bool IsStrongClassOverNotificationEnabled { get; set; }
        public Lesson(string subject, TimeSpan startTime, TimeSpan endTime, bool isSplitBelow, bool isStrongClassOverNotificationEnabled)
        {
            Subject = subject; StartTime = startTime; EndTime = endTime;
            IsSplitBelow = isSplitBelow; IsStrongClassOverNotificationEnabled = isStrongClassOverNotificationEnabled;
        }
        public static Lesson ConvertFromCsesClass(ClassInfo classInfo) =>
            classInfo == null ? new("", new(), new(), false, false) :
            new(classInfo.Subject, classInfo.StartTime, classInfo.EndTime, false, false);
    }
}
