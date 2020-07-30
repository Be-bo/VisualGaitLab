using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VisualGaitLab.GaitAnalysis {
    public partial class GaitWindow : Window {







        // MARK: Static Data (averages and ratios based on all frames)
        private List<int> GetInStanceArray(List<float> allX, List<float> allY, List<float> allXotherMarker, List<float> allYotherMarker, ref List<double> footMidPointXs, ref List<double> footMidpointYs) {

            if (IsFreeRun) { //free running bottom view of the cage (no treadmill) or just a cat walk
                List<double> posDifferences = new List<double>(); //difs in paw positions will be used to calculate switches instead of vectors
                List<int> isInStanceArray = new List<int>();

                for (int i = 0; i < allX.Count; i++) { //get array of midpoints for both X and Y positions for all frames of the video
                    footMidPointXs.Add((allX[i] + allXotherMarker[i]) / 2);
                }
                for (int i = 0; i < allY.Count; i++) {
                    footMidpointYs.Add((allY[i] + allYotherMarker[i]) / 2);
                }

                for (int i = 1; i < footMidpointYs.Count; i++) { //calculate the array of differences between paw positions (midpoints)
                    posDifferences.Add(CalculateDistanceBetweenPoints(footMidPointXs[i - 1], footMidpointYs[i - 1], footMidPointXs[i], footMidpointYs[i]));
                }

                double quarterAvg = posDifferences.Average() / 4; //get avg/4
                for (int i = 0; i < posDifferences.Count; i++) { //use avg/4 to determine what is stance and what is swing
                    if (posDifferences[i] < quarterAvg) {
                        isInStanceArray.Add(1); //if small dif -> stance (paw in one spot)
                    }
                    else {
                        isInStanceArray.Add(0); //if large dif -> swing (paw in the air)
                    }
                }

                return isInStanceArray;
            }
            else {
                List<int> switches = new List<int>();
                switches.Add(0); // the first position is technically a switch
                bool gettingBigger = true;
                bool previous = true;

                // find switches
                for (int i = 1; i < allX.Count; i++) {
                    if (allX[i] >= allX[i - 1]) {
                        gettingBigger = true;
                        if (i == 1) previous = true;
                    }
                    else {
                        gettingBigger = false;
                        if (i == 1) previous = false;
                    }

                    if (previous != gettingBigger) switches.Add(i);
                    previous = gettingBigger;
                }

                switches.Add(allX.Count - 1);

                // find trends for switches (if the given switch segment vector is getting bigger -> swing, or smaller -> stance)
                float slopeSum;
                List<float> switchSlopes = new List<float>();
                for (int i = 1; i < switches.Count; i++) {
                    slopeSum = 0;
                    for (int j = switches[i - 1] + 1; j < switches[i]; j++) slopeSum = slopeSum + CalculateSlope(allX[j], allX[j - 1], j, (j - 1));
                    switchSlopes.Add(slopeSum); // each slopeSum[a-1] starting at a+1 corresponds to series between switches[a-1] AND switches[a]
                }

                // use slope sums to determine stance and swing
                List<int> isStanceArray = new List<int>();
                int isStance; // technically a bool but int is easy to plot
                for (int i = 1; i < switches.Count; i++) {
                    if (switchSlopes[i - 1] >= 0) isStance = 0; // slope positive, going faster than the treadmill -> swing
                    else isStance = 1; // slope negative, going slower than the treadmill -> stance
                    for (int j = switches[i - 1]; j < switches[i]; j++) isStanceArray.Add(isStance);
                }

                return isStanceArray; //the autocorrect feature corrects whatever this function returns (i.e. this is where autocorrect is tapping into the calculation pipeline)
            }
        }

        private double CalculateDistanceBetweenPoints(double x1, double y1, double x2, double y2) {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2)); //ret dist between two points using Pythagorean Theorem
        }

        private float CalculateSlope(float y2, float y1, float x2, float x1) {
            return (float)((y2 - y1) / (x2 - x1));
        }

        private void CalculateGaitBasics(ref List<int> inStanceArray, ref int stanceDuration, ref int swingDuration, ref int numberOfStrides, ref List<int> switchPositions,
            ref List<int> stanceFrameNumByStride, ref List<int> swingFrameNumByStride) {
            //eliminate tails:
            int start = 1;
            while (inStanceArray[start] == inStanceArray[start - 1]) start++; //eliminate the first series (it's always a partial stance/swing -> we disregard)
            int end = inStanceArray.Count - 2;
            while (inStanceArray[end] == inStanceArray[end + 1]) end--; //eliminate the last series (again, to get rid of partials)

            //eliminate incomplete strides (i.e. the whole series has to start with a stance and end with a swing):
            while (inStanceArray[start] == 0) start++;
            while (inStanceArray[end] == 1) end--;

            numberOfStrides = 1; //the last step won't have a switch so we need to compensate for that
            int stanceFrames = 0;
            int swingFrames = 0;
            int switchCounter = 0; //each switch from 0->1 or 1->0 increments this number, once the num reaches 2 (= stance and swing occured) we reset it to 0 and increment the number of strides
            int current = inStanceArray[start + 1];
            int previous = inStanceArray[start];
            int currentStances = 0;
            int currentSwings = 0;
            stanceFrameNumByStride = new List<int>();
            swingFrameNumByStride = new List<int>();
            switchPositions = new List<int>();
            switchPositions.Add(start);

            for (int i = start + 1; i <= end; i++) { //go through all the accepted frames of the video
                current = inStanceArray[i];
                if (current == 1) {
                    stanceFrames++;
                    currentStances++;
                }
                else {
                    swingFrames++;
                    currentSwings++;
                }
                if (previous != current) {
                    switchCounter++;
                    switchPositions.Add(i);
                    if (switchCounter > 1) { //new stride
                        switchCounter = 0;
                        numberOfStrides++;

                        stanceFrameNumByStride.Add(currentStances);
                        swingFrameNumByStride.Add(currentSwings);
                        currentStances = 0;
                        currentSwings = 0;
                    }
                }
                previous = current;
            }

            stanceFrameNumByStride.Add(currentStances);
            swingFrameNumByStride.Add(currentSwings);
            switchPositions.Add(end + 1); //last index doesn't count so we need to shift it to the start of the next stance/stride

            float avgFramesPerStance = (float)stanceFrames / numberOfStrides; // at least one of the operands has to be a float to get float division
            float avgFramesPerSwing = (float)swingFrames / numberOfStrides;

            stanceDuration = (int)(avgFramesPerStance * 1000) / FPS; // to millis
            swingDuration = (int)(avgFramesPerSwing * 1000) / FPS;
        }

        private void CalculateStrideData(int totalCount, ref List<double> strideLengths, ref List<double> strides, ref List<int> switchPositions, ref List<double> midPointXs, ref List<double> midPointYs) {
            //calculating stride lengths, this method doesn't have to be called every frame (-> it's in static data section) but it's the basis for stride dynamic data

            if (switchPositions.Count < 3) {
                MessageBox.Show("Video cannot be added because it's too short. Program will close.", "Video Too Short");
                this.DialogResult = true;
            }
            else {
                strides = new List<double>();
                strideLengths = new List<double>();
                int i = 0;
                while (i < switchPositions[0]) { //add 0 mm stride lengths for all the initial frames that aren't used in calculations (incomplete strides)
                    strideLengths.Add(0);
                    i++;
                }

                for (int j = 0; j + 1 < switchPositions.Count; j = j + 2) { //for everything between the tails and incomplete strides
                    int frameDistance = switchPositions[j + 2] - switchPositions[j]; //we grab the distance between the first frame of the current stance and the last frame of the current swing
                                                                                     //for example let's say the current stride looks like this: first frame (stance) ->1111111111000000<- last frame (swing), the frame distance is 16
                                                                                     //             and just as all the other strides this one has 3 switch positions:  ^=j       ^=j+1 ^j+2 (!!!j+2 corresponds to the beginning of the next step so it's always 1, it's the last frame of the current stride + 1)
                    double distance;
                    if (IsFreeRun) { //don't have treadmill speed for the freewalk type analysis, have to grab actual position difference
                        int startPos = switchPositions[j];
                        int endPos = switchPositions[j + 2];
                        distance = CalculateDistanceBetweenPoints(midPointXs[startPos], midPointYs[startPos], midPointXs[endPos], midPointYs[endPos]);
                        distance = distance * RealWorldMultiplier;
                    }
                    else { //for the treadmill scenario we have to use the treadmill's speed
                        double timeDistance = (double)frameDistance / FPS; //then we simply calculate the duration of the stride in seconds
                        distance = TreadmillSpeed * timeDistance; //and use treadmill speed to get the distance that the paw has traveled through this stride
                    }

                    strides.Add(distance);

                    for (int k = switchPositions[j]; k < switchPositions[j + 2]; k++) { //fill in the distance for all of the stride's frames
                        strideLengths.Add(distance);
                    }
                    //the reason I decided to add the stride length so many times was to make the values easily accessible (i.e. we don't have to worry about stride frame ranges when showing the current value to the user, we simply go strideLengths[currentFrame])
                    //it also makes exporting this data easier
                }

                i = switchPositions[switchPositions.Count - 1];
                while (i < totalCount) { //add 0 mm stride lengths for all the trailing frames that aren't used in calculations (incomplete strides)
                    strideLengths.Add(0);
                    i++;
                }
            }
        }

        private void CalculateStrideLengthVariability(ref List<double> strides, ref double variablity) { //stride length variablity requires us to have stride lengths from the above function first
            double mean = strides.Average();
            List<double> differencesSquared = new List<double>();
            double difSum = 0;
            for (int i = 0; i < strides.Count; i++) {
                double currentDif = Math.Pow(strides[i] - mean, 2);
                difSum = difSum + currentDif;
                differencesSquared.Add(currentDif);
            }
            //double difMean = differencesSquared.Average();
            double difMean = difSum / (differencesSquared.Count - 1);
            variablity = Math.Sqrt(difMean);
        }

        private float CalculateStrideFrequency(List<int> allStancesByStrides, List<int> allSwingsByStrides, int numberOfStrides) { //calculating stride frequency
            int stanceSum = 0;
            int swingSum = 0;
            for (int i = 0; i < allStancesByStrides.Count; i++) { //get all stances by stride (Eg. stride 1 has 20 stance frames, we add those up for all strides)
                stanceSum = stanceSum + allStancesByStrides[i];
            }
            for (int i = 0; i < allSwingsByStrides.Count; i++) { //same for swings
                swingSum = swingSum + allSwingsByStrides[i];
            }
            float durationInSeconds = ((float)stanceSum + (float)swingSum) / (float)FPS; //add up the swings and the stances to get stride frame duration, convert to seconds
            float frequency = (float)numberOfStrides / durationInSeconds; //and divide the number of strides by the duration of all the relevant strides summed up (remember, tail of incomplete strides in the data are cut off so we cannot simply divide by the duration of the video)
            return frequency;
        }













        // MARK: Dynamic Data (everything that changes from frame to frame)

        private void CalculatePawAnglesAndStanceWidths() {
            HindStanceWidths = new List<double>();
            ForeStanceWidths = new List<double>();

            HindLeftPawAngles = new List<double>();
            HindRightPawAngles = new List<double>();
            FrontLeftPawAngles = new List<double>();
            FrontRightPawAngles = new List<double>();

            for (int i = 0; i < GaitNumberOfFrames; i++) {
                //Paw Angles
                CalculateCenterOfMass(i); //center of mass is necessary for paw angles
                double angle = GetPawAngle(HindLeftYs[i], HindLeftHeelYs[i], HindLeftXs[i], HindLeftHeelXs[i],
                   CenterOfMassY, SuperButtYs[i], CenterOfMassX, SuperButtXs[i]);
                HindLeftPawAngles.Add(angle);

                angle = GetPawAngle(HindRightYs[i], HindRightHeelYs[i], HindRightXs[i], HindRightHeelXs[i],
                   CenterOfMassY, SuperButtYs[i], CenterOfMassX, SuperButtXs[i]);
                HindRightPawAngles.Add(angle);

                angle = GetPawAngle(FrontLeftYs[i], FrontLeftHeelYs[i], FrontLeftXs[i], FrontLeftHeelXs[i],
                   NoseYs[i], CenterOfMassY, NoseXs[i], CenterOfMassX);
                FrontLeftPawAngles.Add(angle);

                angle = GetPawAngle(FrontRightYs[i], FrontRightHeelYs[i], FrontRightXs[i], FrontRightHeelXs[i],
                   NoseYs[i], CenterOfMassY, NoseXs[i], CenterOfMassX);
                FrontRightPawAngles.Add(angle);

                //Stance Width
                // right y - left y (midpoints of each paw)
                float midPointTop = (HindRightHeelYs[i] + HindRightYs[i]) / 2;
                float midPointBottom = (HindLeftHeelYs[i] + HindLeftYs[i]) / 2;
                HindStanceWidths.Add((midPointTop - midPointBottom) * RealWorldMultiplier);

                midPointTop = (FrontRightHeelYs[i] + FrontRightYs[i]) / 2;
                midPointBottom = (FrontLeftHeelYs[i] + FrontLeftYs[i]) / 2;
                ForeStanceWidths.Add((midPointTop - midPointBottom) * RealWorldMultiplier);
            }
        }

        private void CalculateCenterOfMass(int forFrame) { //"center of mass" is between the top mid point and the bottom midpoint (run the program and label a video for gait to see which points I'm referring to)
            CenterOfMassX = (RightMidPointXs[forFrame] + LeftMidPointXs[forFrame]) / 2;
            CenterOfMassY = (RightMidPointYs[forFrame] + LeftMidPointYs[forFrame]) / 2;
        }

        private double GetPawAngle(float y2, float y1, float x2, float x1, float yb, float ya, float xb, float xa) { //get an angle between two lines (for hind paws its between their respective points and the "butt-center of mass" line)
            //(for fore paws it's between their respective points and the "center of mass-nose" line) -> we have these two cases because mouse's body can curve and simply running a midline through it's body results in big inaccuracies
            double m2 = (y2 - y1) / (x2 - x1);
            double m1 = (yb - ya) / (xb - xa);

            double angle2 = getAngleFromSlope(m2, y2, y1, x2, x1);
            double angle1 = getAngleFromSlope(m1, yb, ya, xb, xa);
            return Math.Abs(angle1 - angle2);
        }

        private double getAngleFromSlope(double m, float y2, float y1, float x2, float x1) {
            double atanAngle = Math.Atan(m) * (180 / Math.PI);
            double toSubtractFrom = 180; //case Q3

            if (y2 >= y1) toSubtractFrom = -(toSubtractFrom); //case Q2

            if (x2 < x1) { //case Q3 or Q2
                return -(toSubtractFrom - atanAngle);
            }
            else { //case Q1 or Q4, for those atan returns the result as expected
                return atanAngle;
            }
        }

        private void CalculateDynamicDataAverages() {
            //paw angles
            GetAdjustedPawAngles(); //need to account for the paw angle values being disregarded in swing frames for each individual paw (see the declaration of the adjusted arrays in GaitVariables.cs)
            double hlSum = 0;
            double hrSum = 0;
            double flSum = 0;
            double frSum = 0;
            for (int i = 0; i < HindLeftPawAnglesAdjusted.Count; i++) { //getting the sum of all relevant angle values
                hlSum = hlSum + HindLeftPawAnglesAdjusted[i];
            }
            for (int i = 0; i < HindRightPawAnglesAdjusted.Count; i++) {
                hrSum = hrSum + HindRightPawAnglesAdjusted[i];
            }
            for (int i = 0; i < FrontLeftPawAnglesAdjusted.Count; i++) {
                flSum = flSum + FrontLeftPawAnglesAdjusted[i];
            }
            for (int i = 0; i < FrontRightPawAnglesAdjusted.Count; i++) {
                frSum = frSum + FrontRightPawAnglesAdjusted[i];
            }
            HindLeftPawAngleAvg = hlSum / HindLeftPawAnglesAdjusted.Count; //getting the average
            HindRightPawAngleAvg = hrSum / HindRightPawAnglesAdjusted.Count;
            FrontLeftPawAngleAvg = flSum / FrontLeftPawAnglesAdjusted.Count;
            FrontRightPawAngleAvg = frSum / FrontRightPawAnglesAdjusted.Count;

            //stance widths
            hlSum = 0;
            flSum = 0;
            for (int i = 0; i < HindStanceWidths.Count; i++) {
                hlSum = hlSum + HindStanceWidths[i];
                flSum = flSum + ForeStanceWidths[i];
            }
            HindStanceWidthAvg = hlSum / HindStanceWidths.Count;
            ForeStanceWidthAvg = flSum / ForeStanceWidths.Count;

            //stride lengths, same approach
            double sum = 0;
            for (int i = 0; i < HindLeftStrides.Count; i++) sum = sum + HindLeftStrides[i];
            HindLeftStrideLenAvg = sum / HindLeftStrides.Count;

            sum = 0;
            for (int i = 0; i < HindRightStrides.Count; i++) sum = sum + HindRightStrides[i];
            HindRightStrideLenAvg = sum / HindRightStrides.Count;

            sum = 0;
            for (int i = 0; i < FrontLeftStrides.Count; i++) sum = sum + FrontLeftStrides[i];
            FrontLeftStrideLenAvg = sum / FrontLeftStrides.Count;

            sum = 0;
            for (int i = 0; i < FrontRightStrides.Count; i++) sum = sum + FrontRightStrides[i];
            FrontRightStrideLenAvg = sum / FrontRightStrides.Count;
        }

        private void GetAdjustedPawAngles() {
            HindLeftPawAnglesAdjusted = new List<double>();
            HindRightPawAnglesAdjusted = new List<double>();
            FrontLeftPawAnglesAdjusted = new List<double>();
            FrontRightPawAnglesAdjusted = new List<double>();

            for (int i = 0; i < HindLeftInStance.Count; i++) {
                if (HindLeftInStance[i] == 1) //if the given paw is in stance, add the paw angle value, if it's in swing (i.e. else) don't add it
                {
                    HindLeftPawAnglesAdjusted.Add(HindLeftPawAngles[i]);
                }

                if (HindRightInStance[i] == 1) {
                    HindRightPawAnglesAdjusted.Add(HindRightPawAngles[i]);
                }

                if (FrontLeftInStance[i] == 1) {
                    FrontLeftPawAnglesAdjusted.Add(FrontLeftPawAngles[i]);
                }

                if (FrontRightInStance[i] == 1) {
                    FrontRightPawAnglesAdjusted.Add(FrontRightPawAngles[i]);
                }
            }
        }










        // MARK: Cross Correlation - CURRENTLY NOT IN USE

        private void CalculateCrossCorrelationSeries(ref List<int> originalInStanceA, ref List<int> originalInStanceB, ref List<double> resultArray) {

            List<double> inStanceA = new List<double>();
            List<double> inStanceB = new List<double>();
            for (int i = 0; i < originalInStanceA.Count; i++) {
                inStanceA.Add(originalInStanceA[i] - 0.5);
            }
            for (int i = 0; i < originalInStanceB.Count; i++) {
                inStanceB.Add(originalInStanceB[i] - 0.5);
            }

            List<double> shiftyList;
            shiftyList = new List<double>(inStanceB);
            int numberOfOnesListA = 0;
            int numberOfOnesListB = 0;
            foreach (double current in inStanceA) if (current == 0.5) numberOfOnesListA++;
            foreach (double current in inStanceB) if (current == 0.5) numberOfOnesListB++;
            double scale = (double)Math.Max(numberOfOnesListA, numberOfOnesListB);

            for (int i = 0; i < inStanceA.Count; i++) { //for all delays
                resultArray.Add(GetCrossProduct(inStanceA, shiftyList) / scale); //calculate a single correlation coeff with the shifted list
                shiftyList.Add(0);
                shiftyList.RemoveAt(0);
            }
            shiftyList = new List<double>(inStanceB);
            for (int i = 0; i < inStanceA.Count; i++) { //for all delays
                shiftyList.Insert(0, 0);
                shiftyList.RemoveAt(shiftyList.Count - 1);
                resultArray.Insert(0, GetCrossProduct(inStanceA, shiftyList) / scale); //calculate a single correlation coeff with the shifted list
            }
        }

        private double GetCrossProduct(List<double> listA, List<double> listB) {
            double sum = 0;
            for (int i = 0; i < listA.Count; i++) {
                sum = sum + listA[i] * listB[i];
            }
            return sum;
        }














        //MARK: Standard Error of Mean Methods

        private double CalculateSEM(List<double> allValues, double mean) { //baseline SEM calculation for a list of double values along with their mean
            double SDnumerator = 0;
            for (int i = 0; i < allValues.Count; i++) { //get the sum numerator of the Standard Deviation
                SDnumerator = SDnumerator + Math.Pow(allValues[i] - mean, 2);
            }

            double sd = Math.Sqrt(SDnumerator / (allValues.Count - 1)); //get SD by dividing by n-1
            double sem = sd / Math.Sqrt(allValues.Count); //and get the SEM by deviding SD by the square root of n
            return sem;
        }

        private double CalculateSwingStanceSEM(List<int> allFramesByStride) { //adjusted swing stance SEM calculation
            //convert to millis
            List<double> allValuesInMillis = new List<double>();
            for (int i = 0; i < allFramesByStride.Count; i++) {
                allValuesInMillis.Add((((double)allFramesByStride[i]) / (FPS)) * 1000); //first get all values for swing and stance in ms (Eg. stride 1 has a swing lasting 200ms and a stance lasting 300ms, we add up those for each stride)
            }
            double mean = allValuesInMillis.Average(); //get the mean of either stance and swing
            return CalculateSEM(allValuesInMillis, mean); //now we can actually calcuate the SEM with the baseline function
        }

        private double CalculateStrideDurationSEM(List<int> stanceFramesByStride, List<int> swingFramesByStride) { //stride duration SEM calculation (adjusted)
            //convert to millis
            List<double> allValuesInMillis = new List<double>();
            for (int i = 0; i < stanceFramesByStride.Count; i++) { //first add up all durations for each stride of the given paw
                int currentStrideFrames = stanceFramesByStride[i] + swingFramesByStride[i];
                allValuesInMillis.Add((((double)currentStrideFrames) / (FPS)) * 1000);
            }
            double mean = allValuesInMillis.Average(); //get the average stride duration
            return CalculateSEM(allValuesInMillis, mean); //and use the baseline calculation method for SEM
        }

        private void PrepSEM() { //method that calculates all the relevant SEM values for gait data export
            HindLeftAverageStanceDurationSEM = CalculateSwingStanceSEM(HindLeftStancesByStride);
            HindLeftAverageSwingDurationSEM = CalculateSwingStanceSEM(HindLeftSwingsByStride);

            HindRightAverageStanceDurationSEM = CalculateSwingStanceSEM(HindRightStancesByStride);
            HindRightAverageSwingDurationSEM = CalculateSwingStanceSEM(HindRightSwingsByStride);

            FrontLeftAverageStanceDurationSEM = CalculateSwingStanceSEM(FrontLeftStancesByStride);
            FrontLeftAverageSwingDurationSEM = CalculateSwingStanceSEM(FrontLeftSwingsByStride);

            FrontRightAverageStanceDurationSEM = CalculateSwingStanceSEM(FrontRightStancesByStride);
            FrontRightAverageSwingDurationSEM = CalculateSwingStanceSEM(FrontRightSwingsByStride);

            HindLeftStrideDurationSEM = CalculateStrideDurationSEM(HindLeftStancesByStride, HindLeftSwingsByStride);
            HindRightStrideDurationSEM = CalculateStrideDurationSEM(HindRightStancesByStride, HindRightSwingsByStride);
            FrontLeftStrideDurationSEM = CalculateStrideDurationSEM(FrontLeftStancesByStride, FrontLeftSwingsByStride);
            FrontRightStrideDurationSEM = CalculateStrideDurationSEM(FrontRightStancesByStride, FrontRightSwingsByStride);

            //TODO
            Console.WriteLine("HL ANGLE SIZE: " + HindLeftPawAnglesAdjusted.Count);
            HindLeftPawAngleSEM = CalculateSEM(HindLeftPawAnglesAdjusted, HindLeftPawAngleAvg); //have to use the adjusted values to get the correct SEM (to makes sure swing frame paw angle values are disregarded)
            HindRightPawAngleSEM = CalculateSEM(HindRightPawAnglesAdjusted, HindRightPawAngleAvg);
            FrontLeftPawAngleSEM = CalculateSEM(FrontLeftPawAnglesAdjusted, FrontLeftPawAngleAvg);
            FrontRightPawAngleSEM = CalculateSEM(FrontRightPawAnglesAdjusted, FrontRightPawAngleAvg);

            Console.WriteLine("HL WIDTH SIZE: " + HindStanceWidths.Count);
            HindStanceWidthSEM = CalculateSEM(HindStanceWidths, HindStanceWidthAvg);
            ForeStanceWidthSEM = CalculateSEM(ForeStanceWidths, ForeStanceWidthAvg);
            //TODOEND

            HindLeftStrideLenSEM = CalculateSEM(HindLeftStrides, HindLeftStrideLenAvg);
            HindRightStrideLenSEM = CalculateSEM(HindRightStrides, HindRightStrideLenAvg);
            FrontLeftStrideLenSEM = CalculateSEM(FrontLeftStrides, FrontLeftStrideLenAvg);
            FrontRightStrideLenSEM = CalculateSEM(FrontRightStrides, FrontRightStrideLenAvg);
        }
    }
}
