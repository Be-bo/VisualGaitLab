using System;
using System.Numerics;


namespace VisualGaitLab.SupportingClasses
{
    public static class MathUtils
    {

        public static double MidPoint(double a, double b)
        {
            return (a + b) / 2;
        }

        public static float MidPoint(float a, float b)
        {
            return (a + b) / 2;
        }

        public static double CalculateDistanceBetweenPoints(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2)); //ret dist between two points using Pythagorean Theorem
        }

        public static double CalculateSlope(float y2, float y1, float x2, float x1)
        {
            return ((y2 - y1) / (x2 - x1));
        }

        public static float CalculateSlopef(float y2, float y1, float x2, float x1)
        {
            return ((y2 - y1) / (x2 - x1));
        }

        public static double CalculateDistanceBetweenPointLine(float x1, float y1, float x2, float y2, double m2)
        { // Closest point between point 1 and line with slope m and point 2 on line

            double m1 = -(1 / m2);          // slope of perpendicular line
            double b1 = y1 - (m1 * x1);     // calculate y-intercept of lines 
            double b2 = y2 - (m2 * x2);

            // Calculate x-coordinate
            float x = (float) ((b2 - b1) / (m1 - m2));
            float y = (float) (m1 * x + b1);

            return CalculateDistanceBetweenPoints(x1, y1, x, y);
        }

        public static double GetPawAngle(float y2, float y1, float x2, float x1, float yb, float ya, float xb, float xa, double m1)
        { //get an angle between two lines (for hind paws its between their respective points and the "butt-center of mass" line)
            //(for fore paws it's between their respective points and the "center of mass-nose" line) -> we have these two cases because mouse's body can curve and simply running a midline through it's body results in big inaccuracies
            double m2 = CalculateSlope(y2, y1, x2, x1);

            double angle2 = GetAngleFromSlope(m2, y2, y1, x2, x1);
            double angle1 = GetAngleFromSlope(m1, yb, ya, xb, xa);
            return Math.Abs(angle1 - angle2);
        }

        public static double GetPawAngle(float y2, float y1, float x2, float x1, float yb, float ya, float xb, float xa)
        { //get an angle between two lines (for hind paws its between their respective points and the "butt-center of mass" line)
            //(for fore paws it's between their respective points and the "center of mass-nose" line) -> we have these two cases because mouse's body can curve and simply running a midline through it's body results in big inaccuracies
            double m2 = CalculateSlope(y2, y1, x2, x1);
            double m1 = (yb - ya) / (xb - xa);

            double angle2 = GetAngleFromSlope(m2, y2, y1, x2, x1);
            double angle1 = GetAngleFromSlope(m1, yb, ya, xb, xa);
            return Math.Abs(angle1 - angle2);
        }

        private static double GetAngleFromSlope(double m, float y2, float y1, float x2, float x1)
        {
            double atanAngle = Math.Atan(m) * (180 / Math.PI);
            double toSubtractFrom = 180; //case Q3

            if (y2 >= y1) toSubtractFrom = -(toSubtractFrom); //case Q2

            if (x2 < x1)
            { //case Q3 or Q2
                return -(toSubtractFrom - atanAngle);
            }
            else
            { //case Q1 or Q4, for those atan returns the result as expected
                return atanAngle;
            }
        }
    }
}
