
using System;

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

        // Each number corresponds to the label's X/Y column number
        public static int NoseX = 1;
        public static int NoseY = 2;
        public static int ButtX = 4;
        public static int ButtY = 5;
        public static int FrontRight1X = 7;
        public static int FrontRight1Y = 8;
        public static int FrontRight2X = 10;
        public static int FrontRight2Y = 11;
        public static int FrontLeft1X = 13;
        public static int FrontLeft1Y = 14;
        public static int FrontLeft2X = 16;
        public static int FrontLeft2Y = 17;
        public static int HindRight1X = 19;
        public static int HindRight1Y = 20;
        public static int HindRight2X = 22;
        public static int HindRight2Y = 23;
        public static int HindLeft1X = 25;
        public static int HindLeft1Y = 26;
        public static int HindLeft2X = 28;
        public static int HindLeft2Y = 29;
        public static int MidPointLeftX = 31;
        public static int MidPointLeftY = 32;
        public static int MidPointRightX = 34;
        public static int MidPointRightY = 35;


        // Given the header line (second row) of the analyzed video csv file,
        // this function will assign the correct column number to each bodypart.
        public static void configureColumnNames(string line)
        {
            string[] splitLine = line.Split(',');
            for (int i = 1; i < splitLine.Length; i += 3) // Skip probability -> stepping over every third column
            {
                string label = splitLine[i];
                switch (label)
                {
                    case "Nose":
                        NoseX = i;
                        NoseY = i + 1;
                        break;

                    case "Butt":
                        ButtX = i;
                        ButtY = i + 1;
                        break;

                    case "FrontRight1":
                        FrontRight1X = i;
                        FrontRight1Y = i + 1;
                        break;

                    case "FrontRight2":
                        FrontRight2X = i;
                        FrontRight2Y = i + 1;
                        break;

                    case "FrontLeft1":
                        FrontLeft1X = i;
                        FrontLeft1Y = i + 1;
                        break;

                    case "FrontLeft2":
                        FrontLeft2X = i;
                        FrontLeft2Y = i + 1;
                        break;

                    case "HindRight1":
                        HindRight1X = i;
                        HindRight1Y = i + 1;
                        break;

                    case "HindRight2":
                        HindRight2X = i;
                        HindRight2Y = i + 1;
                        break;

                    case "HindLeft1":
                        HindLeft1X = i;
                        HindLeft1Y = i + 1;
                        break;

                    case "HindLeft2":
                        HindLeft2X = i;
                        HindLeft2Y = i + 1;
                        break;

                    case "MidPointLeft":
                        MidPointLeftX = i;
                        MidPointLeftY = i + 1;
                        break;

                    case "MidPointRight":
                        MidPointRightX = i;
                        MidPointRightY = i + 1; 
                        break;
                }
            }

            // To Test
            Console.WriteLine("Nose Column number = " + NoseX);
        }
    }
}
