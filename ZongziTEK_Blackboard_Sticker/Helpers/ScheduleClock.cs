using System;

namespace ZongziTEK_Blackboard_Sticker.Helpers
{
    internal static class ScheduleClock
    {
        public static int? DayAfterDateChange(DateTime lastDate, DateTime now) =>
            lastDate.Date == now.Date ? (int?)null : (int)now.DayOfWeek;

        public static bool CrossedReminder(TimeSpan? previous, TimeSpan current, TimeSpan boundary) =>
            previous.HasValue && current - previous.Value <= TimeSpan.FromSeconds(15) &&
            previous.Value < boundary && boundary <= current;
    }
}
