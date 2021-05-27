using System;


namespace VisualGaitLab.SupportingClasses
{
    public static class MathUtils
    {
        public static double CalculateDistanceBetweenPoints(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2)); //ret dist between two points using Pythagorean Theorem
        }

        public static float CalculateSlope(float y2, float y1, float x2, float x1)
        {
            return (float)((y2 - y1) / (x2 - x1));
        }
    }
}
