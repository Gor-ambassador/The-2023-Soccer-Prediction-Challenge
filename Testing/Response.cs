using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing
{

    public class Details
    {
        public int id { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public string selectedSeason { get; set; }
    }

    public class ResponseData
    {
        public string[] tabs { get; set; }
        public Details details { get; set; }
        public string[] allAvailableSeasons { get; set; }
        public Matches matches { get; set; }
    }

    public class Matches
    {
        public object firstUnplayedMatch { get; set; }
        public Allmatch[] allMatches { get; set; }
    }

    public class Allmatch
    {
        public object round { get; set; }
        public object roundName { get; set; }
        public string pageUrl { get; set; }
        public string id { get; set; }
        public Home18 home { get; set; }
        public Away18 away { get; set; }
        public Status1 status { get; set; }
    }

    public class Home18
    {
        public string name { get; set; }
        public string shortName { get; set; }
        public string id { get; set; }
    }

    public class Away18
    {
        public string name { get; set; }
        public string shortName { get; set; }
        public string id { get; set; }
    }

    public class Status1
    {
        public DateTime utcTime { get; set; }
        public bool finished { get; set; }
        public bool started { get; set; }
        public bool cancelled { get; set; }
        public string scoreStr { get; set; }
        public Reason1 reason { get; set; }
    }

    public class Reason1
    {
        public string _short { get; set; }
        public string shortKey { get; set; }
        public string _long { get; set; }
        public string longKey { get; set; }
    }

}
