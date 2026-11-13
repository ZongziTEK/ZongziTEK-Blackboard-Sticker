using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace ZongziTEK_Blackboard_Sticker.Models
{
    public sealed class DutyRoster
    {
        [JsonProperty(Required = Required.Always)]
        public int Version { get; set; } = 1;
        public bool IsEvenWeek { get; set; }
        public List<DutyRole> Roles { get; set; } = new();
        public Dictionary<int, DutyDay> Days { get; set; } = new();

        public static readonly string[] SourceNames = { "周日", "周一", "周二", "周三", "周四", "周五", "周六", "临时" };

        public void Validate()
        {
            if (Version != 1 || Roles.Any(role => role == null || role.Id == Guid.Empty ||
                string.IsNullOrWhiteSpace(role.Name)) || Roles.Select(role => role.Id).Distinct().Count() != Roles.Count ||
                Days.Keys.Any(day => day < 0 || day > 6))
                throw new InvalidDataException("值日数据的版本或结构无效。");
        }

        public DutyDay Day(int day)
        {
            if (!Days.TryGetValue(day, out var value) || value == null)
                Days[day] = value = new DutyDay();
            return value;
        }

        public void DeleteRole(Guid id)
        {
            Roles.RemoveAll(role => role.Id == id);
            foreach (var day in Days.Values.Where(day => day != null))
            {
                day.Odd.Remove(id);
                day.Even.Remove(id);
            }
        }

        public DutySnapshot Capture(int day)
        {
            var assignments = Day(day);
            return new DutySnapshot
            {
                Rows = Roles.Select(role => new DutySnapshotRow
                {
                    RoleId = role.Id,
                    Role = role.Name,
                    Odd = assignments.Odd.TryGetValue(role.Id, out var odd) ? odd : "",
                    Even = assignments.Even.TryGetValue(role.Id, out var even) ? even : ""
                }).ToList()
            };
        }

        public DutyPresentation Present(int? source, DutySnapshot temporary)
        {
            string week = IsEvenWeek ? "双周" : "单周";
            if (!source.HasValue)
                return new DutyPresentation("ClassIsland · " + week, "共享课表未关联本地值日安排", new List<DutyRow>());

            var snapshot = source.Value == 7 ? temporary : Capture(source.Value);
            var rows = snapshot?.Rows.Where(row => row != null && !string.IsNullOrWhiteSpace(row.Role))
                .Select(row => new DutyRow(row.Role, IsEvenWeek ? row.Even : row.Odd)).ToList() ?? new List<DutyRow>();
            return new DutyPresentation(SourceNames[source.Value] + " · " + week, "未配置值日安排", rows);
        }
    }

    public sealed class DutyRole
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public sealed class DutyDay
    {
        public Dictionary<Guid, string> Odd { get; set; } = new();
        public Dictionary<Guid, string> Even { get; set; } = new();
    }

    public sealed class DutySnapshot
    {
        public List<DutySnapshotRow> Rows { get; set; } = new();
    }

    public sealed class DutySnapshotRow
    {
        public Guid RoleId { get; set; }
        public string Role { get; set; }
        public string Odd { get; set; }
        public string Even { get; set; }
    }

    public sealed class DutyRow
    {
        public DutyRow(string role, string names)
        {
            Role = role;
            Names = string.IsNullOrWhiteSpace(names) ? "未填写" : names;
        }
        public string Role { get; }
        public string Names { get; }
    }

    public sealed class DutyPresentation
    {
        public DutyPresentation(string title, string emptyText, List<DutyRow> rows)
        {
            Title = title;
            EmptyText = emptyText;
            Rows = rows;
        }
        public string Title { get; }
        public string EmptyText { get; }
        public List<DutyRow> Rows { get; }
    }
}
