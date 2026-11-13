using System;
using Newtonsoft.Json;
namespace ZongziTEK_Blackboard_Sticker.Models
{
    public class Curriculums
    {
        // Store the two temporary assignments in the same atomic write as the temporary courses.
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DutySnapshot TemporaryDuty { get; set; }

        public Monday Monday { get; set; } = new Monday();
        public Tuesday Tuesday { get; set; } = new Tuesday();
        public Wednesday Wednesday { get; set; } = new Wednesday();
        public Friday Friday { get; set; } = new Friday();
        public Sunday Sunday { get; set; } = new Sunday();
        public Thursday Thursday { get; set; } = new Thursday();
        public Saturday Saturday { get; set; } = new Saturday();
        public Temp Temp { get; set; } = new Temp();

        public string GetDay(int source)
        {
            switch (source)
            {
                case 0: return Sunday.Curriculums;
                case 1: return Monday.Curriculums;
                case 2: return Tuesday.Curriculums;
                case 3: return Wednesday.Curriculums;
                case 4: return Thursday.Curriculums;
                case 5: return Friday.Curriculums;
                case 6: return Saturday.Curriculums;
                case 7: return Temp.Curriculums;
                default: throw new ArgumentOutOfRangeException(nameof(source));
            }
        }

        public void ReplaceTemporary(int source, DutySnapshot duty)
        {
            Temp.Curriculums = GetDay(source);
            TemporaryDuty = duty;
        }
    }

    public class Monday
    {
        public string Curriculums { get; set; }
    }

    public class Tuesday
    {
        public string Curriculums { get; set; }
    }

    public class Wednesday
    {
        public string Curriculums { get; set; }
    }

    public class Thursday
    {
        public string Curriculums { get; set; }
    }

    public class Friday
    {
        public string Curriculums { get; set; }
    }

    public class Saturday
    {
        public string Curriculums { get; set; }
    }

    public class Sunday
    {
        public string Curriculums { get; set; }
    }

    public class Temp
    {
        public string Curriculums { get; set; }
    }
}
