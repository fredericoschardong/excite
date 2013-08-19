using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace TimelineSample
{
    public abstract class Package
    {
        protected double time;

        protected String entityA;
        protected String entityB;
        protected List<String> entities;

        public Package()
        {

        }

        public Package(BinaryReader reader, List<String> entities)
        {
            this.entities = entities;
            resolveEntities(reader.ReadInt32(), reader.ReadInt32());
            time = reader.ReadDouble();
        }

        //true on success
        protected bool resolveEntities(int a, int b)
        {
            if (a >= 0)
            {
                entityA = entities[a];
            }
            else
            {
                entityA = a.ToString();
            }

            if (b >= 0)
            {
                entityB = entities[b];
            }
            else
            {
                entityB = b.ToString();
            }

            return true;
        }

        public double getTime()
        {
            return this.time;
        }

        public String getEntityA(){
            return this.entityA;
        }

        public String getEntityB()
        {
            return this.entityB;
        }
    }

    class PackageCollision : Package
    {
        private double distance;
        private Tuple<double, double, double> intersection;
        private bool colliding;

        public PackageCollision(BinaryReader reader)
        {
            //time = reader.ReadDouble();
            distance = reader.ReadDouble();
            intersection = new Tuple<double, double, double>(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());
            colliding = reader.ReadBoolean();

            /*Console.Out.WriteLine("Time: " + time);
            Console.Out.WriteLine("Distance: " + distance);
            Console.Out.WriteLine("Intersection  (" + intersection.Item1 + ", " + intersection.Item2 + ", " + intersection.Item3 + ")");
            Console.Out.WriteLine("A and B collide: " + colliding);*/
        }

        public double getDistance()
        {
            return distance;
        }

        public Tuple<double, double, double> getIntersection()
        {
            return intersection;
        }

        public bool getColliding()
        {
            return colliding;
        }
    }

    class PackagePointing : Package
    {
        private double distance;
        private Tuple<double, double> displayPoint;
        private Tuple<double, double, double> intersection;
        private bool isTouching;
        private bool isPointingAt;
        private bool isPointingToward;

        public PackagePointing(BinaryReader reader, List<String> entities)
            : base(reader, entities)
        {
            distance = reader.ReadDouble();
            displayPoint = new Tuple<double, double>(reader.ReadDouble(), reader.ReadDouble());
            intersection = new Tuple<double, double, double>(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());
            isTouching = reader.ReadBoolean();
            isPointingAt = reader.ReadBoolean();
            isPointingToward = reader.ReadBoolean();

            /*Console.Out.WriteLine("A: " + entityA);
            Console.Out.WriteLine("B: " + entityB);
            Console.Out.WriteLine("Time: " + time);

            Console.Out.WriteLine("Distance: " + distance);
            Console.Out.WriteLine("Display point (" + intersection.Item1 + ", " + intersection.Item2 + ")");
            Console.Out.WriteLine("Intersection (" + intersection.Item1 + ", " + intersection.Item2 + ", " + intersection.Item3 + ")");
            Console.Out.WriteLine("Is touching: " + isTouching);
            Console.Out.WriteLine("Is pointing at: " + isPointingAt);
            Console.Out.WriteLine("Is pointing towards: " + isPointingAt);*/
        }

        public double getDistance()
        {
            return distance;
        }

        public Tuple<double, double> getDisplayPoint()
        {
            return displayPoint;
        }

        public Tuple<double, double, double> getIntersection()
        {
            return intersection;
        }

        public bool getIsTouching()
        {
            return isTouching;
        }

        public bool getIsPointingAt()
        {
            return isPointingAt;
        }

        public bool getIsPointingToward()
        {
            return isPointingToward;
        }
    }

    class PackageDirection : Package
    {
        private Tuple<double, double, double> entityAPosition;
        private Tuple<double, double, double> entityBPosition;
        private bool isAFromB;
        private bool isBFromA;
        private bool isATowardsB;
        private bool isBTowardsA;
        private bool isAParallelB;
        private bool isBParallelA;

        public PackageDirection(BinaryReader reader, List<String> entities)
            : base(reader, entities)
        {
            entityAPosition = new Tuple<double, double, double>(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());
            entityBPosition = new Tuple<double, double, double>(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());
            isAFromB = reader.ReadBoolean();
            isBFromA = reader.ReadBoolean();
            isATowardsB = reader.ReadBoolean();
            isBTowardsA = reader.ReadBoolean();
            isAParallelB = reader.ReadBoolean();
            isBParallelA = reader.ReadBoolean();

            /*Console.Out.WriteLine("A: " + entityA);
            Console.Out.WriteLine("B: " + entityB);
            Console.Out.WriteLine("Time: " + time);

            Console.Out.WriteLine("A (" + entityAPosition.Item1 + ", " + entityAPosition.Item2 + ", " + entityAPosition.Item3 + ")");
            Console.Out.WriteLine("B (" + entityBPosition.Item1 + ", " + entityBPosition.Item2 + ", " + entityBPosition.Item3 + ")");
            Console.Out.WriteLine("A from B: " + isAFromB);
            Console.Out.WriteLine("B from A: " + isBFromA);
            Console.Out.WriteLine("A towards B: " + isATowardsB);
            Console.Out.WriteLine("B towards A: " + isBTowardsA);
            Console.Out.WriteLine("Is A parallel: " + isAParallelB);
            Console.Out.WriteLine("Is B perpendicular: " + isBParallelA);*/

            /*if (isATowardsB && (entityA == "bluehat" && entityB == "tabletc"))
            {
                Console.WriteLine(entityA + " " + entityB + " " + isATowardsB + " " + isBTowardsA);
            }

            if (isBTowardsA && (entityB == "bluehat" && entityA == "tabletc"))
            {
                Console.WriteLine(entityA + " " + entityB + " " + isATowardsB + " " + isBTowardsA);
            }*/

            //if((isATowardsB || isBTowardsA) && ((entityA == "bluehat" && entityB == "tabletc") || (entityB == "bluehat" && entityA == "tabletc")))
            //Console.WriteLine(entityA + " " + entityB + " " + isATowardsB + " " + isBTowardsA);
        }

        public Tuple<double, double, double> getEntityAPosition()
        {
            return entityAPosition;
        }

        public Tuple<double, double, double> getEntityBPosition()
        {
            return entityBPosition;
        }

        public bool getIsAFromB()
        {
            return isAFromB;
        }

        public bool getIsBFromA()
        {
            return isBFromA;
        }

        public bool getIsATowardsB()
        {
            return isATowardsB;
        }

        public bool getIsBTowardsA()
        {
            return isBTowardsA;
        }

        public bool getIsAParallelB()
        {
            return isAParallelB;
        }

        public bool getIsBParallelA()
        {
            return isBParallelA;
        }
    }

    class PackageLocation : Package
    {
        private Tuple<double, double, double> entityAPosition;
        private Tuple<double, double, double> entityBPosition;
        private double distance;
        private bool isAFromB;
        private bool isBFromA;
        private bool isATowardsB;
        private bool isBTowardsA;
        private bool isAMoving;
        private bool isBMoving;
        private bool parallel;
        private bool perpendicular;

        public PackageLocation(BinaryReader reader, List<String> entities)
            : base(reader, entities)
        {
            entityAPosition = new Tuple<double, double, double>(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());
            entityBPosition = new Tuple<double, double, double>(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());
            distance = reader.ReadDouble();
            isAFromB = reader.ReadBoolean();
            isBFromA = reader.ReadBoolean();
            isATowardsB = reader.ReadBoolean();
            isBTowardsA = reader.ReadBoolean();
            isAMoving = reader.ReadBoolean();
            isBMoving = reader.ReadBoolean();
            parallel = reader.ReadBoolean();
            perpendicular = reader.ReadBoolean();

            /*Console.Out.WriteLine("A: " + entityA);
            Console.Out.WriteLine("B: " + entityB);
            Console.Out.WriteLine("Time: " + time);

            Console.Out.WriteLine("A (" + entityAPosition.Item1 + ", " + entityAPosition.Item2 + ", " + entityAPosition.Item3 + ")");
            Console.Out.WriteLine("B (" + entityBPosition.Item1 + ", " + entityBPosition.Item2 + ", " + entityBPosition.Item3 + ")");
            Console.Out.WriteLine("Distance: " + distance);
            Console.Out.WriteLine("A from B: " + isAFromB);
            Console.Out.WriteLine("B from A: " + isBFromA);
            Console.Out.WriteLine("A towards B: " + isATowardsB);
            Console.Out.WriteLine("B towards A: " + isBTowardsA);
            Console.Out.WriteLine("Is A moving: " + isAMoving);
            Console.Out.WriteLine("Is B moving: " + isBMoving);
            Console.Out.WriteLine("parallel: " + parallel);
            Console.Out.WriteLine("perpendicular: " + perpendicular);*/
        }

        public Tuple<double, double, double> getEntityAPosition()
        {
            return entityAPosition;
        }

        public Tuple<double, double, double> getEntityBPosition()
        {
            return entityBPosition;
        }

        public double getDistance()
        {
            return distance;
        }

        public bool getIsAFromB()
        {
            return isAFromB;
        }

        public bool getIsBFromA()
        {
            return isBFromA;
        }

        public bool getIsATowardsB()
        {
            return isATowardsB;
        }

        public bool getIsBTowardsA()
        {
            return isBTowardsA;
        }

        public bool getIsAMoving()
        {
            return isAMoving;
        }

        public bool getIsBMoving()
        {
            return isBMoving;
        }

        public bool getParallel()
        {
            return this.parallel;
        }

        public bool getPerpendicular()
        {
            return this.perpendicular;
        }
    }

    class PackageMotion : Package
    {
        private bool isAMoving;
        private bool isBMoving;
        private Double velocityDifference;
        private bool xAccelerationAgrees;
        private bool xVelocityAgrees;
        private bool yAccelerationAgrees;
        private bool yVelocityAgrees;
        private bool zAccelerationAgrees;
        private bool zVelocityAgrees;

        public PackageMotion(BinaryReader reader, List<String> entities)
            : base(reader, entities)
        {
            isAMoving = reader.ReadBoolean();
            isBMoving = reader.ReadBoolean();

            velocityDifference = reader.ReadDouble();

            xAccelerationAgrees = reader.ReadBoolean();
            xVelocityAgrees = reader.ReadBoolean();
            yAccelerationAgrees = reader.ReadBoolean();
            yVelocityAgrees = reader.ReadBoolean();
            zAccelerationAgrees = reader.ReadBoolean();
            zVelocityAgrees = reader.ReadBoolean();

            /*Console.Out.WriteLine("A: " + entityA);
            Console.Out.WriteLine("B: " + entityB);
            Console.Out.WriteLine("Time: " + time);

            Console.Out.WriteLine("isAMoving: " + isAMoving);
            Console.Out.WriteLine("isBMoving: " + isBMoving);
            Console.Out.WriteLine("velocityDifference: " + velocityDifference);
            Console.Out.WriteLine("xAccelerationAgrees: " + xAccelerationAgrees);
            Console.Out.WriteLine("xVelocityAgrees: " + xVelocityAgrees);
            Console.Out.WriteLine("yAccelerationAgrees: " + yAccelerationAgrees);
            Console.Out.WriteLine("yVelocAgrees: " + yVelocityAgrees);
            Console.Out.WriteLine("zAccelerationAgrees: " + zAccelerationAgrees);
            Console.Out.WriteLine("xVelocityAgrees: " + xVelocityAgrees);
            Console.Out.WriteLine("zVelocAgrees: " + zVelocityAgrees);*/
        }

        public bool getIsAMoving()
        {
            return isAMoving;
        }

        public bool getIsBMoving()
        {
            return isBMoving;
        }

        public Double getVelocityDifference()
        {
            return velocityDifference;
        }

        public bool getXAccelerationAgrees()
        {
            return xAccelerationAgrees;
        }

        public bool getXVelocityAgrees()
        {
            return xVelocityAgrees;
        }

        public bool getYAccelerationAgrees()
        {
            return yAccelerationAgrees;
        }

        public bool getYVelocAgrees()
        {
            return yVelocityAgrees;
        }

        public bool getZAccelerationAgrees()
        {
            return zAccelerationAgrees;
        }

        public bool getZVelocityAgrees()
        {
            return zVelocityAgrees;
        }
    }
}
