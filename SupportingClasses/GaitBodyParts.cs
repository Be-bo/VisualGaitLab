using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualGaitLab.SupportingClasses
{
    static class GaitBodyParts
    {
        static public string[] names = {
            "Nose",
            "Butt",
            "FrontRight1",
            "FrontRight2",
            "FrontLeft1",
            "FrontLeft2",
            "HindRight1",
            "HindRight2",
            "HindLeft1",
            "HindLeft2",
            "MidPointLeft",
            "MidPointRight"
        };

        //for now we're skipping probability -> stepping over every third column
        public enum ColNum : int
        {
            NoseX = 1,
            NoseY = 2,
            ButtX = 4,
            ButtY = 5,
            FrontRight1X = 7,
            FrontRight1Y = 8,
            FrontRight2X = 10,
            FrontRight2Y = 11,
            FrontLeft1X = 13,
            FrontLeft1Y = 14,
            FrontLeft2X = 16,
            FrontLeft2Y = 17,
            HindRight1X = 19,
            HindRight1Y = 20,
            HindRight2X = 22,
            HindRight2Y = 23,
            HindLeft1X = 25,
            HindLeft1Y = 26,
            HindLeft2X = 28,
            HindLeft2Y = 29,
            MidPointLeftX = 31,
            MidPointLeftY = 32,
            MidPointRightX = 34,
            MidPointRightY = 35
        }
    }
}
