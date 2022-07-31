using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using static VisualGaitLab.SupportingClasses.MathUtils;

namespace VisualGaitLab.GaitAnalysis {
    public partial class GaitWindow : Window {


        // MARK: Static Data (averages and ratios based on all frames)

        private List<int> GetTreadMillInStanceArray(List<float> allX)
        {
            // the first position is technically a switch
            List<int> switches = new List<int> { 0 };
            bool previous = true;
            if (allX.Count > 0) previous = allX[1] >= allX[0];

            // find switches
            for (int i = 1; i < allX.Count; i++)
            {
                bool gettingBigger;

                if (allX[i] >= allX[i - 1]) gettingBigger = true;
                else gettingBigger = false;

                if (previous != gettingBigger) switches.Add(i);
                previous = gettingBigger;
            }

            switches.Add(allX.Count - 1);

            // find trends for switches (if the given switch segment vector is getting bigger -> swing, or smaller -> stance)
            float slopeSum;
            List<float> switchSlopes = new List<float>();
            for (int i = 1; i < switches.Count; i++)
            {
                slopeSum = 0;
                for (int j = switches[i - 1] + 1; j < switches[i]; j++) slopeSum += CalculateSlopef(allX[j], allX[j - 1], j, (j - 1));
                switchSlopes.Add(slopeSum); // each slopeSum[a-1] starting at a+1 corresponds to series between switches[a-1] AND switches[a]
            }

            // use slope sums to determine stance and swing
            List<int> isStanceArray = new List<int>();
            int isStance; // technically a bool but int is easy to plot
            for (int i = 1; i < switches.Count; i++)
            {
                if (switchSlopes[i - 1] >= 0) isStance = 0; // slope positive, going faster than the treadmill -> swing
                else isStance = 1; // slope negative, going slower than the treadmill -> stance
                for (int j = switches[i - 1]; j < switches[i]; j++) isStanceArray.Add(isStance);
            }

            return isStanceArray; //the autocorrect feature corrects whatever this function returns (i.e. this is where autocorrect is tapping into the calculation pipeline)
        }


        // Free running bottom view of the cage (no treadmill) or just a cat walk
        private List<int> GetCatWalkInStanceArray(List<float> allX, List<float> allY, List<float> allXotherMarker, List<float> allYotherMarker, ref List<double> footMidPointXs, ref List<double> footMidpointYs) {
 
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

            double avgDiff = posDifferences.Average() * bias; // get avg of frame differences (k = adjustment to get best result)
            for (int i = 0; i < posDifferences.Count; i++) { //use avg to determine what is stance and what is swing
                if (posDifferences[i] < avgDiff) {
                    isInStanceArray.Add(1); //if small dif -> stance (paw in one spot)
                }
                else {
                    isInStanceArray.Add(0); //if large dif -> swing (paw in the air)
                }
            }

            return isInStanceArray;
        }




        private void CalculateGaitBasics(ref List<int> inStanceArray, ref int stanceDuration, ref int swingDuration, ref int numberOfStrides, ref List<int> switchPositions,
            ref List<int> stanceFrameNumByStride, ref List<int> swingFrameNumByStride) 
        {
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
            int previous = inStanceArray[start];
            int currentStances = 0;
            int currentSwings = 0;
            stanceFrameNumByStride = new List<int>();
            swingFrameNumByStride = new List<int>();
            switchPositions = new List<int>
            {
                start
            };

            for (int i = start + 1; i <= end; i++) { //go through all the accepted frames of the video
                int current = inStanceArray[i];
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
                setup_error = true; //set flag
                return;
            }
            
            strides = new List<double>();
            strideLengths = new List<double>();
            int i = 0;
            while (i < switchPositions[0]) { //add 0 mm stride lengths for all the initial frames that aren't used in calculations (incomplete strides)
                strideLengths.Add(0);
                i++;
            }

            for (int j = 0; j + 1 < switchPositions.Count; j += 2) { //for everything between the tails and incomplete strides
                double distance;
                if (IsFreeRun) { //don't have treadmill speed for the freewalk type analysis, have to grab actual position difference
                    int startPos = switchPositions[j];
                    int endPos = switchPositions[j + 2];
                    distance = CalculateDistanceBetweenPoints(midPointXs[startPos], midPointYs[startPos], midPointXs[endPos], midPointYs[endPos]);
                    distance *= RealWorldMultiplier; // Convert from pixels to mm
                }
                else { //for the treadmill scenario we have to use the treadmill's speed
                    int frameDistance = switchPositions[j + 2] - switchPositions[j]; //we grab the distance between the first frame of the current stance and the last frame of the current swing
                                                                                     //for example let's say the current stride looks like this: first frame (stance) ->1111111111000000<- last frame (swing), the frame distance is 16
                                                                                     //             and just as all the other strides this one has 3 switch positions:  ^=j       ^=j+1 ^j+2 (!!!j+2 corresponds to the beginning of the next step so it's always 1, it's the last frame of the current stride + 1)

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

        private void CalculateStrideLengthVariability(ref List<double> strides, ref double variablity) { //stride length variablity requires us to have stride lengths from the above function first
            double mean = strides.Average();
            List<double> differencesSquared = new List<double>();
            double difSum = 0;
            for (int i = 0; i < strides.Count; i++) {
                double currentDif = Math.Pow(strides[i] - mean, 2);
                difSum += currentDif;
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
                stanceSum += allStancesByStrides[i];
            }
            for (int i = 0; i < allSwingsByStrides.Count; i++) { //same for swings
                swingSum += allSwingsByStrides[i];
            }
            float durationInSeconds = ((float)stanceSum + (float)swingSum) / (float)FPS; //add up the swings and the stances to get stride frame duration, convert to seconds
            float frequency = (float)numberOfStrides / durationInSeconds; //and divide the number of strides by the duration of all the relevant strides summed up (remember, tail of incomplete strides in the data are cut off so we cannot simply divide by the duration of the video)
            return frequency;
        }













        // MARK: Dynamic Data (everything that changes from frame to frame)

        private void CalculatePawAnglesAndStanceWidths() {
            // Clear lists
            foreach (var list in new List<double>[] { HindStanceWidths, ForeStanceWidths, HindLeftStanceWidths, 
                                                      HindRightStanceWidths, FrontLeftStanceWidths, FrontRightStanceWidths,
                                                      HindLeftPawAngles, HindRightPawAngles, FrontLeftPawAngles, FrontRightPawAngles})
            {
                list.Clear();
            }


            for (int i = 0; i < GaitNumberOfFrames; i++) {
                
                // Center of Mass and Central Lines
                CalculateCenterOfMass(i);       // center of mass is necessary for paw angles
                double FrontCentralLine = CalculateSlope(NoseYs[i], CenterOfMassY, NoseXs[i], CenterOfMassX);           // slope of the line between nose and center of mass
                double BackCentralLine = CalculateSlope(CenterOfMassY, SuperButtYs[i], CenterOfMassX, SuperButtXs[i]);  // slope of the line between the butt and center of mass

                //Paw Angles
                double angle = GetPawAngle(HindLeftYs[i], HindLeftHeelYs[i], HindLeftXs[i], HindLeftHeelXs[i],
                   CenterOfMassY, SuperButtYs[i], CenterOfMassX, SuperButtXs[i], BackCentralLine);
                HindLeftPawAngles.Add(angle);

                angle = GetPawAngle(HindRightYs[i], HindRightHeelYs[i], HindRightXs[i], HindRightHeelXs[i],
                   CenterOfMassY, SuperButtYs[i], CenterOfMassX, SuperButtXs[i], BackCentralLine);
                HindRightPawAngles.Add(angle);

                angle = GetPawAngle(FrontLeftYs[i], FrontLeftHeelYs[i], FrontLeftXs[i], FrontLeftHeelXs[i],
                   NoseYs[i], CenterOfMassY, NoseXs[i], CenterOfMassX, FrontCentralLine);
                FrontLeftPawAngles.Add(angle);

                angle = GetPawAngle(FrontRightYs[i], FrontRightHeelYs[i], FrontRightXs[i], FrontRightHeelXs[i],
                   NoseYs[i], CenterOfMassY, NoseXs[i], CenterOfMassX, FrontCentralLine);
                FrontRightPawAngles.Add(angle);

                // Stance Widths
                CalculateStanceWidths(i, FrontCentralLine, BackCentralLine);
            }
        }

        private void CalculateCenterOfMass(int forFrame) { //"center of mass" is between the top mid point and the bottom midpoint (run the program and label a video for gait to see which points I'm referring to)
            if (forFrame < RightMidPointXs.Count)
            {
                CenterOfMassX = (RightMidPointXs[forFrame] + LeftMidPointXs[forFrame]) / 2;
                CenterOfMassY = (RightMidPointYs[forFrame] + LeftMidPointYs[forFrame]) / 2;
            }
        }



        private void CalculateStanceWidths(int i, double FrontCentralLine, double BackCentralLine)
        {
            // Stance Width (old method - Front and Hind, new method - Per Paw)

            // Hind Stance Widths
            float midPointRightX = MidPoint(HindRightHeelXs[i], HindRightXs[i]);
            float midPointRightY = MidPoint(HindRightHeelYs[i], HindRightYs[i]);
            float midPointLeftX = MidPoint(HindLeftHeelXs[i], HindLeftXs[i]);
            float midPointLeftY = MidPoint(HindLeftHeelYs[i], HindLeftYs[i]);
            double distToCenterlineRight = CalculateDistanceBetweenPointLine(midPointRightX, midPointRightY, CenterOfMassX, CenterOfMassY, BackCentralLine);
            double distToCenterlineLeft = CalculateDistanceBetweenPointLine(midPointLeftX, midPointLeftY, CenterOfMassX, CenterOfMassY, BackCentralLine);

            HindStanceWidths.Add((midPointRightY - midPointLeftY) * RealWorldMultiplier);
            HindRightStanceWidths.Add(distToCenterlineRight * RealWorldMultiplier);
            HindLeftStanceWidths.Add(distToCenterlineLeft * RealWorldMultiplier);

            // Fore Stance Widths
            midPointRightX = MidPoint(FrontRightHeelXs[i], FrontRightXs[i]);
            midPointRightY = MidPoint(FrontRightHeelYs[i], FrontRightYs[i]);
            midPointLeftX = MidPoint(FrontLeftHeelXs[i], FrontLeftXs[i]);
            midPointLeftY = MidPoint(FrontLeftHeelYs[i], FrontLeftYs[i]);
            distToCenterlineRight = CalculateDistanceBetweenPointLine(midPointRightX, midPointRightY, CenterOfMassX, CenterOfMassY, FrontCentralLine);
            distToCenterlineLeft = CalculateDistanceBetweenPointLine(midPointLeftX, midPointLeftY, CenterOfMassX, CenterOfMassY, FrontCentralLine);

            ForeStanceWidths.Add((midPointRightY - midPointLeftY) * RealWorldMultiplier);
            FrontRightStanceWidths.Add(distToCenterlineRight * RealWorldMultiplier);
            FrontLeftStanceWidths.Add(distToCenterlineLeft * RealWorldMultiplier);
        }

        private void CalculateDynamicDataAverages() {
            // Paw angles
            GetAdjustedPawAngles(); //need to account for the paw angle values being disregarded in swing frames for each individual paw (see the declaration of the adjusted arrays in GaitVariables.cs)

            HindLeftPawAngleAvg = HindLeftPawAnglesAdjusted.Sum() / HindLeftPawAnglesAdjusted.Count;
            HindRightPawAngleAvg = HindRightPawAnglesAdjusted.Sum() / HindRightPawAnglesAdjusted.Count;
            FrontLeftPawAngleAvg = FrontLeftPawAnglesAdjusted.Sum() / FrontLeftPawAnglesAdjusted.Count;
            FrontRightPawAngleAvg = FrontRightPawAnglesAdjusted.Sum() / FrontRightPawAnglesAdjusted.Count;

            // Stance widths (old - fore and back)
            HindStanceWidthAvg = HindStanceWidths.Sum() / HindStanceWidths.Count;
            ForeStanceWidthAvg = ForeStanceWidths.Sum() / ForeStanceWidths.Count;

            // Stance widths (new - individual paws)
            HindLeftStanceWidthAvg = HindLeftStanceWidths.Sum() / HindLeftStanceWidths.Count;
            HindRightStanceWidthAvg = HindRightStanceWidths.Sum() / HindRightStanceWidths.Count;
            FrontLeftStanceWidthAvg = FrontLeftStanceWidths.Sum() / FrontLeftStanceWidths.Count;
            FrontRightStanceWidthAvg = FrontRightStanceWidths.Sum() / FrontRightStanceWidths.Count;

            // Stride lengths
            HindLeftStrideLenAvg = HindLeftStrides.Sum() / HindLeftStrides.Count;
            HindRightStrideLenAvg = HindRightStrides.Sum() / HindRightStrides.Count;
            FrontLeftStrideLenAvg = FrontLeftStrides.Sum() / FrontLeftStrides.Count;
            FrontRightStrideLenAvg = FrontRightStrides.Sum() / FrontRightStrides.Count;
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
                sum += listA[i] * listB[i];
            }
            return sum;
        }














        //MARK: Standard Error of Mean Methods

        private double CalculateSEM(List<double> allValues, double mean) { //baseline SEM calculation for a list of double values along with their mean
            double SDnumerator = 0;
            for (int i = 0; i < allValues.Count; i++) { //get the sum numerator of the Standard Deviation
                SDnumerator += Math.Pow(allValues[i] - mean, 2);
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
            double mean = allValuesInMillis.Average(); //get the mean of either stance or swing
            Console.WriteLine(mean);
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

            HindLeftPawAngleSEM = CalculateSEM(HindLeftPawAnglesAdjusted, HindLeftPawAngleAvg); //have to use the adjusted values to get the correct SEM (to makes sure swing frame paw angle values are disregarded)
            HindRightPawAngleSEM = CalculateSEM(HindRightPawAnglesAdjusted, HindRightPawAngleAvg);
            FrontLeftPawAngleSEM = CalculateSEM(FrontLeftPawAnglesAdjusted, FrontLeftPawAngleAvg);
            FrontRightPawAngleSEM = CalculateSEM(FrontRightPawAnglesAdjusted, FrontRightPawAngleAvg);

            HindStanceWidthSEM = CalculateSEM(HindStanceWidths, HindStanceWidthAvg);
            ForeStanceWidthSEM = CalculateSEM(ForeStanceWidths, ForeStanceWidthAvg);

            HindLeftStrideLenSEM = CalculateSEM(HindLeftStrides, HindLeftStrideLenAvg);
            HindRightStrideLenSEM = CalculateSEM(HindRightStrides, HindRightStrideLenAvg);
            FrontLeftStrideLenSEM = CalculateSEM(FrontLeftStrides, FrontLeftStrideLenAvg);
            FrontRightStrideLenSEM = CalculateSEM(FrontRightStrides, FrontRightStrideLenAvg);
        }
    }
}
