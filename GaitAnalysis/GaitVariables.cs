using LiveCharts;
using LiveCharts.Defaults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace VisualGaitLab.GaitAnalysis {
    public partial class GaitWindow : Window {








        // MARK: Setup Vars

        Regex NumberRegex = new Regex("^[0-9]*$");
        private bool GaitFirstSetup = true;
        private List<int> GaitErrorFrames = new List<int>();
        private bool IsFreeRun = false;

        private int XMAX = 500; //X and Y maxes and mins for the paw charts
        private int XMIN = 0;
        private double YMAX = 1.2;
        private int YMIN = 0;
        private int FPS = 100;
        private double GaitVideoLength = 0;
        private int GaitVideoHeight = 0;
        private double RealWorldMultiplier; //or "how many millimeters each pixel corresponds to", we can simply calculate our distances in pixels and then multiply those numbers with this value
        private double TreadmillSpeed;

        private string GaitVideoPath;
        private string GaitVideoName;
        private string GaitTempPath;
        private int NumberOfDigits;
        private int GaitNumberOfFrames;
        private int GaitCurrentFrame = 0;

        private IChartValues HindLeftObservables = new ChartValues<ObservablePoint>(); //in stance arrays that are fed to the paw charts (all elems have a value of either 1 = stance or 0 = swing)
        private IChartValues HindRightObservables = new ChartValues<ObservablePoint>();
        private IChartValues FrontLeftObservables = new ChartValues<ObservablePoint>();
        private IChartValues FrontRightObservables = new ChartValues<ObservablePoint>();

        private IChartValues HindLeftScatter = new ChartValues<ObservablePoint>(); //a single value list that's fed as a second series to each paw chart and as the user adjusts current frame this point reflects it
        private IChartValues HindRightScatter = new ChartValues<ObservablePoint>();
        private IChartValues FrontLeftScatter = new ChartValues<ObservablePoint>();
        private IChartValues FrontRightScatter = new ChartValues<ObservablePoint>();




        // MARK: Calculation Related Variables and Constants

        private List<float> SuperButtXs = new List<float>();
        private List<float> SuperButtYs = new List<float>();
        private List<float> NoseXs = new List<float>();
        private List<float> NoseYs = new List<float>();

        private List<float> LeftMidPointXs = new List<float>();
        private List<float> LeftMidPointYs = new List<float>();

        private List<float> RightMidPointXs = new List<float>();
        private List<float> RightMidPointYs = new List<float>();

        private List<float> HindLeftXs = new List<float>();
        private List<float> HindLeftHeelXs = new List<float>();

        private List<float> HindLeftYs = new List<float>();
        private List<float> HindLeftHeelYs = new List<float>();

        private List<float> HindRightXs = new List<float>();
        private List<float> HindRightHeelXs = new List<float>();

        private List<float> HindRightYs = new List<float>();
        private List<float> HindRightHeelYs = new List<float>();

        private List<float> FrontLeftXs = new List<float>();
        private List<float> FrontLeftHeelXs = new List<float>();

        private List<float> FrontLeftYs = new List<float>();
        private List<float> FrontLeftHeelYs = new List<float>();

        private List<float> FrontRightXs = new List<float>();
        private List<float> FrontRightHeelXs = new List<float>();

        private List<float> FrontRightYs = new List<float>();
        private List<float> FrontRightHeelYs = new List<float>();

        private List<int> HindLeftInStance;
        private List<int> HindRightInStance;
        private List<int> FrontLeftInStance;
        private List<int> FrontRightInStance;







        // MARK: Static Metrics

        private int HindLeftAverageSwingDuration;
        private int HindLeftAverageStanceDuration;
        private int HindLeftNumberOfStrides;

        private int HindRightAverageSwingDuration;
        private int HindRightAverageStanceDuration;
        private int HindRightNumberOfStrides;

        private int FrontLeftAverageSwingDuration;
        private int FrontLeftAverageStanceDuration;
        private int FrontLeftNumberOfStrides;

        private int FrontRightAverageSwingDuration;
        private int FrontRightAverageStanceDuration;
        private int FrontRightNumberOfStrides;








        // MARK: Dynamic Metrics

        float CenterOfMassX = 0;
        float CenterOfMassY = 0;

        private List<double> HindLeftStrideLengths; //stride lengths for each paw corresponding to frames (i.e. if a stride is happening between frames 100-200, HindLeftSL[100]-HindLeftSL[200] will all be filled with the stride len corresponding to that stride)
        private List<double> HindRightStrideLengths;
        private List<double> FrontLeftStrideLengths;
        private List<double> FrontRightStrideLengths;

        private List<int> HindLeftSwitchPositions;
        private List<int> HindRightSwitchPositions;
        private List<int> FrontLeftSwitchPositions;
        private List<int> FrontRightSwitchPositions;

        private double HindLeftStrideLengthVariablity;
        private double HindRightStrideLengthVariability;
        private double FrontLeftStrideLengthVariability;
        private double FrontRightStrideLengthVariability;

        private List<double> HindLeftStrides; //same as <paw>StrideLengths except without any duplicates, i.e. the len of these is the number of strides for a that particular paw
        private List<double> HindRightStrides;
        private List<double> FrontLeftStrides;
        private List<double> FrontRightStrides;

        private double HindLeftStrideLenAvg;
        private double HindRightStrideLenAvg;
        private double FrontLeftStrideLenAvg;
        private double FrontRightStrideLenAvg;

        private List<double> HindLeftPawAngles = new List<double>(); //all of these are for UI purposes (want to have the given index correspond to the current frame)
        private List<double> HindRightPawAngles = new List<double>();
        private List<double> FrontLeftPawAngles = new List<double>();
        private List<double> FrontRightPawAngles = new List<double>();

        private List<double> HindLeftPawAnglesAdjusted = new List<double>(); //because of the fact that paw angle values have to be disregarded for all swing phases, we have another set of values for angles, these are the same as the four arrays above but with swing frames removed
        //so the frames and indices don't correspond here
        private List<double> HindRightPawAnglesAdjusted = new List<double>();
        private List<double> FrontLeftPawAnglesAdjusted = new List<double>();
        private List<double> FrontRightPawAnglesAdjusted = new List<double>();

        private double HindLeftPawAngleAvg;
        private double HindRightPawAngleAvg;
        private double FrontLeftPawAngleAvg;
        private double FrontRightPawAngleAvg;

        private List<double> HindStanceWidths = new List<double>(); //same as <paw>StrideLengths except without any duplicates, i.e. the len of these is the number of strides for a that particular paw
        private List<double> ForeStanceWidths = new List<double>();

        private double HindStanceWidthAvg;
        private double ForeStanceWidthAvg;








        // MARK: Cross Correlation

        private List<double> HindLimbsCrossCorrelation = new List<double>();
        private List<double> ForeLimbsCrossCorrelation = new List<double>();
        private List<double> HindLeftToForeRightCC = new List<double>();
        private List<double> HindRightToForeLeftCC = new List<double>();





        // MARK: Free Run Specific Variables

        List<double> HindLeftMidPointXs = new List<double>();
        List<double> HindLeftMidPointYs = new List<double>();
        List<double> HindRightMidPointXs = new List<double>();
        List<double> HindRightMidPointYs = new List<double>();
        List<double> FrontLeftMidPointXs = new List<double>();
        List<double> FrontLeftMidPointYs = new List<double>();
        List<double> FrontRightMidPointXs = new List<double>();
        List<double> FrontRightMidPointYs = new List<double>();





        //MARK: Standard Error of Mean

        private List<int> HindLeftStancesByStride;
        private List<int> HindLeftSwingsByStride;

        private List<int> HindRightStancesByStride;
        private List<int> HindRightSwingsByStride;

        private List<int> FrontLeftStancesByStride;
        private List<int> FrontLeftSwingsByStride;

        private List<int> FrontRightStancesByStride;
        private List<int> FrontRightSwingsByStride;

        private double HindLeftStrideDurationSEM;
        private double HindRightStrideDurationSEM;
        private double FrontLeftStrideDurationSEM;
        private double FrontRightStrideDurationSEM;

        private double HindLeftPawAngleSEM;
        private double HindRightPawAngleSEM;
        private double FrontLeftPawAngleSEM;
        private double FrontRightPawAngleSEM;

        private double HindStanceWidthSEM;
        private double ForeStanceWidthSEM;

        private double HindLeftStrideLenSEM;
        private double HindRightStrideLenSEM;
        private double FrontLeftStrideLenSEM;
        private double FrontRightStrideLenSEM;

        private double HindLeftAverageStanceDurationSEM;
        private double HindRightAverageStanceDurationSEM;
        private double FrontLeftAverageStanceDurationSEM;
        private double FrontRightAverageStanceDurationSEM;

        private double HindLeftAverageSwingDurationSEM;
        private double HindRightAverageSwingDurationSEM;
        private double FrontLeftAverageSwingDurationSEM;
        private double FrontRightAverageSwingDurationSEM;




        private void PrintSizesOfAllTypesOfVars() { //a method for testing
            Console.WriteLine("size: HindLeftInStance " + HindLeftInStance.Count);
            Console.WriteLine("size: HindLeftStrideLengths " + HindLeftStrideLengths.Count);
            Console.WriteLine("size: HindLeftSwitchPositions " + HindLeftSwitchPositions.Count);
            Console.WriteLine("size: HindLeftStrides " + HindLeftStrides.Count);
            Console.WriteLine("size: HindLeftPawAngles " + HindLeftPawAngles.Count);
            Console.WriteLine("size: HindLeftPawAnglesAdjusted " + HindLeftPawAnglesAdjusted.Count);
            Console.WriteLine("size: HindStanceWidths " + HindStanceWidths.Count);
            Console.WriteLine("size: HindLeftMidPointXs " + HindLeftMidPointXs.Count);
            Console.WriteLine("size: HindLeftStancesByStride " + HindLeftStancesByStride.Count);
            Console.WriteLine();
        }
    }
}
