using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimelineSample
{
    public class Comment
    {
        public int time { get; set; }
        public int duration { get; set; }
        public string comment { get; set; }
        public int row { get; set; }

        public Comment(int time, int duration, string comment, int row)
        {
            this.time = time;
            this.duration = duration;
            this.comment = comment;
            this.row = row;
        }
    }
}
