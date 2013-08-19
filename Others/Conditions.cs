using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimelineSample
{
    public class Conditions
    {
        public Conditions(String entityA, String entityB)
        {
            this.entityA = entityA;
            this.entityB = entityB;

            distance = int.MinValue;
            velocity_difference = int.MinValue;

            operators = null;

            x = int.MinValue;
            y = int.MinValue;
            z = int.MinValue;
        }

        public string entityA { get; set; }
        public string entityB { get; set; }

        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }

        public int distance { get; set; }
        public int velocity_difference { get; set; }

        public String operators { get; set; }

        public bool moving { get; set; }

        public bool colliding { get; set; }
        public bool pointing_at { get; set; }
        public bool pointing_towards { get; set; }
        public bool touching { get; set; }
        public bool from { get; set; }
        public bool towards { get; set; }
        public bool parallel { get; set; }
        public bool perpendicular { get; set; }

        public override string ToString()
        {
            return "entitiA: " + entityA + " entityB:" + entityB + " x:" + x + " y:" + y + " z:" + z + " distance:" + distance + " velocity_difference:" + velocity_difference +
                " operators:" + operators + " moving:" + moving + " colliding:" + colliding + " pointing_at:" + pointing_at + " pointing_towards:" + pointing_towards +
                " touching:" + touching + " from:" + from + " towards:" + towards + " parallel:" + parallel + " perpendicular:" + perpendicular;
        }
    }

    public class Relations
    {
        public Relations()
        {
            relation1 = -1;
            relation2 = -1;

            operation = operations.NULL;
            condition = null;
            result = null;
            rule = null;

            order = orderType.NULL;
            orderTime = -1;
        }

        public int relation1 { get; set; }
        public int relation2 { get; set; }

        public enum operations { NULL, AND, OR };
        public operations operation { get; set; }

        public string displayRule { get; set; }
        public string rule { get; set; }
        public Conditions condition { get; set; }

        public Nullable<bool> result { get; set; }

        public enum orderType { NULL, STRICT, FLEXIBLE };
        public orderType order { get; set; }

        public int orderTime { get; set; }

        public static bool Compare(Nullable<bool> condition1, Nullable<bool> condition2, operations operation)
        {
            if (operation == operations.AND)
            {
                return condition1 == true && condition2 == true;
            }

            if (operation == operations.OR)
            {
                return condition1 == true || condition2 == true;
            }

            MainWindow.error("Some error occurred parsing your conditions", true);

            return false;
        }
    }
}
