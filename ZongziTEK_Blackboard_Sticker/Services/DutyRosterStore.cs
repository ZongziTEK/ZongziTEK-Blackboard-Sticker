using System.IO;
using ZongziTEK_Blackboard_Sticker.Helpers;
using ZongziTEK_Blackboard_Sticker.Models;

namespace ZongziTEK_Blackboard_Sticker.Services
{
    internal sealed class DutyRosterStore
    {
        private readonly string path;

        public DutyRosterStore(string directory)
        {
            path = Path.Combine(directory, "DutyRoster.json");
        }

        public DutyRoster Load()
        {
            var roster = LocalJson.ReadOrNew<DutyRoster>(path);
            roster.Validate();
            return roster;
        }

        public void Save(DutyRoster roster)
        {
            roster.Validate();
            LocalJson.Write(path, roster);
        }
    }
}
