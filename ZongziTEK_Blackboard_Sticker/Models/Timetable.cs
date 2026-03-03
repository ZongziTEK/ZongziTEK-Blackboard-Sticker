using CsesSharp;
using CsesSharp.Models;
using System;
using System.Collections.Generic;

namespace ZongziTEK_Blackboard_Sticker.Models
{
    public class Timetable
    {
        public List<Lesson> Monday { get; set; } = new();
        public List<Lesson> Tuesday { get; set; } = new();
        public List<Lesson> Wednesday { get; set; } = new();
        public List<Lesson> Thursday { get; set; } = new();
        public List<Lesson> Friday { get; set; } = new();
        public List<Lesson> Saturday { get; set; } = new();
        public List<Lesson> Sunday { get; set; } = new();
        public List<Lesson> Temp { get; set; } = new();

        public string ToCurriculums(List<Lesson> list)
        {
            string curriculums = "";
            if (list.Count > 0)
            {
                foreach (Lesson lesson in list)
                {
                    curriculums += lesson.Subject + "\n";
                }
                if (curriculums.Length > 0) curriculums = curriculums.Remove(curriculums.Length - 1);
            }
            else
            {
                curriculums = "无课程";
            }
            return curriculums;
        }

        public static void Sort(Timetable timetable)
        {
            foreach (List<Lesson> day in new[] { timetable.Monday, timetable.Tuesday, timetable.Wednesday, timetable.Thursday, timetable.Friday, timetable.Saturday, timetable.Sunday, timetable.Temp })
            {
                day.Sort((x, y) => x.StartTime.CompareTo(y.StartTime));
            }
        }

        public static Timetable ConvertFromCses(string csesText)
        {
            Timetable timetable = new Timetable();
            Profile profile = CsesLoader.LoadFromYamlString(csesText);

            if (profile != null)
            {
                foreach (Schedule schedule in profile.Schedules)
                {
                    List<Lesson> targetDay = new();
                    switch (schedule.EnableDay)
                    {
                        case DayOfWeek.Monday:
                            targetDay = timetable.Monday;
                            break;
                        case DayOfWeek.Tuesday:
                            targetDay = timetable.Tuesday;
                            break;
                        case DayOfWeek.Wednesday:
                            targetDay = timetable.Wednesday;
                            break;
                        case DayOfWeek.Thursday:
                            targetDay = timetable.Thursday;
                            break;
                        case DayOfWeek.Friday:
                            targetDay = timetable.Friday;
                            break;
                        case DayOfWeek.Saturday:
                            targetDay = timetable.Saturday;
                            break;
                        case DayOfWeek.Sunday:
                            targetDay = timetable.Sunday;
                            break;
                    }

                    targetDay.Clear();
                    foreach (ClassInfo classInfo in schedule.Classes)
                    {
                        targetDay.Add(Lesson.ConvertFromCsesClass(classInfo));
                    }
                }
                Sort(timetable);
            }

            return timetable;
        }
    }

    public class Lesson
    {
        public string Subject { get; set; } = "";
        public TimeSpan StartTime { get; set; } = new TimeSpan();
        public TimeSpan EndTime { get; set; } = new TimeSpan();
        public bool IsSplitBelow { get; set; } = false;
        public bool IsStrongClassOverNotificationEnabled { get; set; } = false;

        public Lesson(string subject, TimeSpan startTime, TimeSpan endTime, bool isSplitBelow, bool isStrongClassOverNotificationEnabled)
        {
            Subject = subject;
            StartTime = startTime;
            EndTime = endTime;
            IsSplitBelow = isSplitBelow;
            IsStrongClassOverNotificationEnabled = isStrongClassOverNotificationEnabled;
        }

        public static Lesson ConvertFromCsesClass(ClassInfo classInfo)
        {
            if (classInfo == null) return new("", new(), new(), false, false);
            Lesson lesson = new(classInfo.Subject, classInfo.StartTime, classInfo.EndTime, false, false);
            return lesson;
        }
    }
}
