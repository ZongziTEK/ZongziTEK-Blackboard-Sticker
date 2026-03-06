namespace ZongziTEK.BlackboardSticker.Models
{
    public class Curriculums
    {
        public Monday Monday { get; set; } = new Monday();
        public Tuesday Tuesday { get; set; } = new Tuesday();
        public Wednesday Wednesday { get; set; } = new Wednesday();
        public Friday Friday { get; set; } = new Friday();
        public Sunday Sunday { get; set; } = new Sunday();
        public Thursday Thursday { get; set; } = new Thursday();
        public Saturday Saturday { get; set; } = new Saturday();
        public Temp Temp { get; set; } = new Temp();
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
