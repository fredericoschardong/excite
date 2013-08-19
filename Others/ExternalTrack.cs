using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimelineSample
{
    class ExternalTrack
    {
        public int time { get; set; }
        public string name { get; set; }
        public string comment { get; set; }
        public int duration { get; set; }
    }

    class Csv
    {
        public string name { get; set; }
        public string file { get; set; }
    }
}
