using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MedicinSkema.Models
{
    public class Teams
    {
        public List<Team> teams { get; set; }
    }

    public class Team
    {
        public string name { get; set; }
        public int id { get; set; }
    }

    public class Period
    {
        public int date { get; set; }
        public int startTime { get; set; }
        public int endTime { get; set; }
        public List<PeriodElement> elements { get; set; }
    }

    public class PeriodElement
    {
        public int id { get; set; }
    }

    public class Element
    {
        public int type { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string longName { get; set; }
    }

    public class RealClass
    {
        public Element element { get; set; }
        public Element location { get; set; }
        public Element team { get; set; }
        public Period period { get; set; }
    }
}