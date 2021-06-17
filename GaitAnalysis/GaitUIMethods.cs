using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace VisualGaitLab.GaitAnalysis {
    public partial class GaitWindow : Window {






        // MARK: Slider Related Functions

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            UpdateFrame(false);
        }

        private void UpdateFrame(bool zoomUpdate) {
            string strValue = "0";
            if (!zoomUpdate) { //current frame was updated using the "full length" main slide
                strValue = Convert.ToString(Math.Round(FrameSlider.Value));
                int intValue = int.Parse(strValue);
                GaitCurrentFrame = intValue;
            }
            else { //current frame was updated using the "segment" zoom in slider
                strValue = GaitCurrentFrame.ToString();
            }

            FrameText.Text = "Frame: " + strValue; //update the necessary UI elements (frame number and the corresponding image)
            while (strValue.Length < NumberOfDigits) strValue = "0" + strValue;
            string fileName = "file" + strValue + ".png";
            string path = GaitTempPath + "\\" + fileName;
            BitmapImage bitmap = new BitmapImage(new Uri(path, UriKind.Absolute));
            CurrentFrameText.Source = bitmap;

            ((ObservablePoint)LeftHindChart.Series[1].ActualValues[0]).X = GaitCurrentFrame; //update the dots (= scatter plot with a single point) that indicate which frame in the charts the user's looking at
            ((ObservablePoint)LeftHindChart.Series[1].ActualValues[0]).Y = HindLeftInStance[GaitCurrentFrame];

            ((ObservablePoint)LeftFrontChart.Series[1].ActualValues[0]).X = GaitCurrentFrame;
            ((ObservablePoint)LeftFrontChart.Series[1].ActualValues[0]).Y = FrontLeftInStance[GaitCurrentFrame];

            ((ObservablePoint)RightHindChart.Series[1].ActualValues[0]).X = GaitCurrentFrame;
            ((ObservablePoint)RightHindChart.Series[1].ActualValues[0]).Y = HindRightInStance[GaitCurrentFrame];

            ((ObservablePoint)RightFrontChart.Series[1].ActualValues[0]).X = GaitCurrentFrame;
            ((ObservablePoint)RightFrontChart.Series[1].ActualValues[0]).Y = FrontRightInStance[GaitCurrentFrame];

            SetDynamicData(); //update values for dynamic data
        }

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (ZoomSliderStartBox != null && ZoomSliderEndBox != null && ZoomSlider != null) { //zoom slider changed value     
                if (NumberRegex.IsMatch(ZoomSliderStartBox.Text) && NumberRegex.IsMatch(ZoomSliderEndBox.Text)) { //first check if the input is in the correct format
                    int start = int.Parse(ZoomSliderStartBox.Text);
                    int end = int.Parse(ZoomSliderEndBox.Text);
                    if (start >= 0 && start < GaitNumberOfFrames && end >= 0 && end < GaitNumberOfFrames && start < end) { //then check if the range of frames is feasible
                        double sliderVal = ZoomSlider.Value; //will give an integer value between 0 and 100
                        int frame = Convert.ToInt32((end - start) * (sliderVal / 100) + start); //convert that number to an actual frame number and update with the new frame
                        GaitCurrentFrame = frame;
                        UpdateFrame(true);
                    }
                }
            }
        }












        // MARK: Data Setting

        private void SetDynamicData() { //set each frame

            //Paw Angles
            int currentIsStance = HindLeftInStance[GaitCurrentFrame]; //paw angles only applicable for stance
            if (currentIsStance == 1) {
                LHPawAngleLabel.Text = string.Format("{0:00.00}", HindLeftPawAngles[GaitCurrentFrame]);
            }
            else {
                LHPawAngleLabel.Text = "N/A";
            }

            currentIsStance = HindRightInStance[GaitCurrentFrame];
            if (currentIsStance == 1) {
                RHPawAngleLabel.Text = string.Format("{0:00.00}", HindRightPawAngles[GaitCurrentFrame]);
            }
            else {
                RHPawAngleLabel.Text = "N/A";
            }

            currentIsStance = FrontLeftInStance[GaitCurrentFrame];
            if (currentIsStance == 1) {
                LFPawAngleLabel.Text = string.Format("{0:00.00}", FrontLeftPawAngles[GaitCurrentFrame]);
            }
            else {
                LFPawAngleLabel.Text = "N/A";
            }

            currentIsStance = FrontRightInStance[GaitCurrentFrame];
            if (currentIsStance == 1) {
                RFPawAngleLabel.Text = string.Format("{0:00.00}", FrontRightPawAngles[GaitCurrentFrame]);
            }
            else {
                RFPawAngleLabel.Text = "N/A";
            }


            // Back/Fore Stance Width
            H1StanceWidthLabel.Text = string.Format("{0:00.00}", HindStanceWidths[GaitCurrentFrame]);
            H2StanceWidthLabel.Text = string.Format("{0:00.00}", HindStanceWidths[GaitCurrentFrame]);
            F1StanceWidthLabel.Text = string.Format("{0:00.00}", ForeStanceWidths[GaitCurrentFrame]);
            F2StanceWidthLabel.Text = string.Format("{0:00.00}", ForeStanceWidths[GaitCurrentFrame]);

            // Individual Stance Width
            LHStanceWidthLabel.Text = string.Format("{0:00.00}", HindLeftStanceWidths[GaitCurrentFrame]);
            RHStanceWidthLabel.Text = string.Format("{0:00.00}", HindRightStanceWidths[GaitCurrentFrame]);
            LFStanceWidthLabel.Text = string.Format("{0:00.00}", FrontLeftStanceWidths[GaitCurrentFrame]);
            RFStanceWidthLabel.Text = string.Format("{0:00.00}", FrontRightStanceWidths[GaitCurrentFrame]);

            // Stride Length
            LFStrideLengthLabel.Text = string.Format("{0:00.00}", FrontLeftStrideLengths[GaitCurrentFrame]);
            RFStrideLengthLabel.Text = string.Format("{0:00.00}", FrontRightStrideLengths[GaitCurrentFrame]);
            LHStrideLengthLabel.Text = string.Format("{0:00.00}", HindLeftStrideLengths[GaitCurrentFrame]);
            RHStrideLengthLabel.Text = string.Format("{0:00.00}", HindRightStrideLengths[GaitCurrentFrame]);
        }

        private void SetStaticData() { //set only when needed

            // Hind Left
            //Swing, Stance, Number of Strides, Stride Frequency, Gait Symmetry + everything derived from these
            CalculateGaitBasics(ref HindLeftInStance, ref HindLeftAverageStanceDuration, ref HindLeftAverageSwingDuration, ref HindLeftNumberOfStrides, ref HindLeftSwitchPositions, ref HindLeftStancesByStride, ref HindLeftSwingsByStride);
            int stride = HindLeftAverageStanceDuration + HindLeftAverageSwingDuration;
            LHStanceLabel.Text = HindLeftAverageStanceDuration.ToString();
            LHSwingLabel.Text = HindLeftAverageSwingDuration.ToString();
            LHStrideLabel.Text = stride.ToString();
            LHSwingPercentageLabel.Text = string.Format("{0:00.00}", (float)(HindLeftAverageSwingDuration * 100) / stride);
            LHStancePercentageLabel.Text = string.Format("{0:00.00}", (float)(HindLeftAverageStanceDuration * 100) / stride);
            LHSwingToStanceLabel.Text = string.Format("{0:0.000}", (float)HindLeftAverageSwingDuration / HindLeftAverageStanceDuration);
            LHFrequencyLabel.Text = string.Format("{0:0.000}", CalculateStrideFrequency(HindLeftStancesByStride, HindLeftSwingsByStride, HindLeftNumberOfStrides));
            LHStrideNumLabel.Text = HindLeftNumberOfStrides.ToString();

            //Hind Right
            CalculateGaitBasics(ref HindRightInStance, ref HindRightAverageStanceDuration, ref HindRightAverageSwingDuration, ref HindRightNumberOfStrides, ref HindRightSwitchPositions, ref HindRightStancesByStride, ref HindRightSwingsByStride);
            stride = HindRightAverageStanceDuration + HindRightAverageSwingDuration;
            RHStanceLabel.Text = HindRightAverageStanceDuration.ToString();
            RHSwingLabel.Text = HindRightAverageSwingDuration.ToString();
            RHStrideLabel.Text = stride.ToString();
            RHSwingPercentageLabel.Text = string.Format("{0:00.00}", (float)(HindRightAverageSwingDuration * 100) / stride);
            RHStancePercentageLabel.Text = string.Format("{0:00.00}", (float)(HindRightAverageStanceDuration * 100) / stride);
            RHSwingToStanceLabel.Text = string.Format("{0:0.000}", (float)HindRightAverageSwingDuration / HindRightAverageStanceDuration);
            RHFrequencyLabel.Text = string.Format("{0:0.000}", CalculateStrideFrequency(HindRightStancesByStride, HindRightSwingsByStride, HindRightNumberOfStrides));
            RHStrideNumLabel.Text = HindRightNumberOfStrides.ToString();

            //Front Left
            CalculateGaitBasics(ref FrontLeftInStance, ref FrontLeftAverageStanceDuration, ref FrontLeftAverageSwingDuration, ref FrontLeftNumberOfStrides, ref FrontLeftSwitchPositions, ref FrontLeftStancesByStride, ref FrontLeftSwingsByStride);
            stride = FrontLeftAverageStanceDuration + FrontLeftAverageSwingDuration;
            LFStanceLabel.Text = FrontLeftAverageStanceDuration.ToString();
            LFSwingLabel.Text = FrontLeftAverageSwingDuration.ToString();
            LFStrideLabel.Text = stride.ToString();
            LFSwingPercentageLabel.Text = string.Format("{0:00.00}", (float)(FrontLeftAverageSwingDuration * 100) / stride);
            LFStancePercentageLabel.Text = string.Format("{0:00.00}", (float)(FrontLeftAverageStanceDuration * 100) / stride);
            LFSwingToStanceLabel.Text = string.Format("{0:0.000}", (float)FrontLeftAverageSwingDuration / FrontLeftAverageStanceDuration);
            LFFrequencyLabel.Text = string.Format("{0:0.000}", CalculateStrideFrequency(FrontLeftStancesByStride, FrontLeftSwingsByStride, FrontLeftNumberOfStrides));
            LFStrideNumLabel.Text = FrontLeftNumberOfStrides.ToString();

            //Front Right
            CalculateGaitBasics(ref FrontRightInStance, ref FrontRightAverageStanceDuration, ref FrontRightAverageSwingDuration, ref FrontRightNumberOfStrides, ref FrontRightSwitchPositions, ref FrontRightStancesByStride, ref FrontRightSwingsByStride);
            stride = FrontRightAverageStanceDuration + FrontRightAverageSwingDuration;
            RFStanceLabel.Text = FrontRightAverageStanceDuration.ToString();
            RFSwingLabel.Text = FrontRightAverageSwingDuration.ToString();
            RFStrideLabel.Text = stride.ToString();
            RFSwingPercentageLabel.Text = string.Format("{0:00.00}", (float)(FrontRightAverageSwingDuration * 100) / stride);
            RFStancePercentageLabel.Text = string.Format("{0:00.00}", (float)(FrontRightAverageStanceDuration * 100) / stride);
            RFSwingToStanceLabel.Text = string.Format("{0:0.000}", (float)FrontRightAverageSwingDuration / FrontRightAverageStanceDuration);
            RFFrequencyLabel.Text = string.Format("{0:0.000}", CalculateStrideFrequency(FrontRightStancesByStride, FrontRightSwingsByStride, FrontRightNumberOfStrides));
            RFStrideNumLabel.Text = FrontRightNumberOfStrides.ToString();

            //Gait Symmetry (one value for all paws)
            LHGaitSymmetryLabel.Text = string.Format("{0:0.000}", (CalculateStrideFrequency(FrontLeftStancesByStride, FrontLeftSwingsByStride, FrontLeftNumberOfStrides) + CalculateStrideFrequency(FrontRightStancesByStride, FrontRightSwingsByStride, FrontRightNumberOfStrides)) / (CalculateStrideFrequency(HindLeftStancesByStride, HindLeftSwingsByStride, HindLeftNumberOfStrides) + CalculateStrideFrequency(HindRightStancesByStride, HindRightSwingsByStride, HindRightNumberOfStrides)));
            RHGaitSymmetryLabel.Text = LHGaitSymmetryLabel.Text;
            LFGaitSymmetryLabel.Text = LHGaitSymmetryLabel.Text;
            RFGaitSymmetryLabel.Text = LHGaitSymmetryLabel.Text;

            

            //Stride Lengths, Stride Length Variability
            CalculateStrideData(HindLeftInStance.Count, ref HindLeftStrideLengths, ref HindLeftStrides, ref HindLeftSwitchPositions, ref HindLeftMidPointXs, ref HindLeftMidPointYs);
            CalculateStrideData(HindRightInStance.Count, ref HindRightStrideLengths, ref HindRightStrides, ref HindRightSwitchPositions, ref HindRightMidPointXs, ref HindRightMidPointYs);
            CalculateStrideData(FrontLeftInStance.Count, ref FrontLeftStrideLengths, ref FrontLeftStrides, ref FrontLeftSwitchPositions, ref FrontLeftMidPointXs, ref FrontLeftMidPointYs);
            CalculateStrideData(FrontRightInStance.Count, ref FrontRightStrideLengths, ref FrontRightStrides, ref FrontRightSwitchPositions, ref FrontRightMidPointXs, ref FrontRightMidPointYs);

            CalculateStrideLengthVariability(ref HindLeftStrides, ref HindLeftStrideLengthVariablity);
            CalculateStrideLengthVariability(ref HindRightStrides, ref HindRightStrideLengthVariability);
            CalculateStrideLengthVariability(ref FrontLeftStrides, ref FrontLeftStrideLengthVariability);
            CalculateStrideLengthVariability(ref FrontRightStrides, ref FrontRightStrideLengthVariability);

            LHStrideLengthVariabilityLabel.Text = string.Format("{0:00.00}", HindLeftStrideLengthVariablity);
            RHStrideLengthVariabilityLabel.Text = string.Format("{0:00.00}", HindRightStrideLengthVariability);
            LFStrideLengthVariabilityLabel.Text = string.Format("{0:00.00}", FrontLeftStrideLengthVariability);
            RFStrideLengthVariabilityLabel.Text = string.Format("{0:00.00}", FrontRightStrideLengthVariability);

            

            //Dynamic Data Averages and precalculations for all frames
            CalculatePawAnglesAndStanceWidths();
            CalculateDynamicDataAverages();
            LHPawAngleAvg.Text = string.Format("{0:00.00}", HindLeftPawAngleAvg);
            RHPawAngleAvg.Text = string.Format("{0:00.00}", HindRightPawAngleAvg);
            LFPawAngleAvg.Text = string.Format("{0:00.00}", FrontLeftPawAngleAvg);
            RFPawAngleAvg.Text = string.Format("{0:00.00}", FrontRightPawAngleAvg);

            H1StanceWidthAvg.Text = string.Format("{0:00.00}", HindStanceWidthAvg);
            H2StanceWidthAvg.Text = string.Format("{0:00.00}", HindStanceWidthAvg);
            F1StanceWidthAvg.Text = string.Format("{0:00.00}", ForeStanceWidthAvg);
            F2StanceWidthAvg.Text = string.Format("{0:00.00}", ForeStanceWidthAvg);

            LHStanceWidthAvg.Text = string.Format("{0:00.00}", HindLeftStanceWidthAvg);
            RHStanceWidthAvg.Text = string.Format("{0:00.00}", HindRightStanceWidthAvg);
            LFStanceWidthAvg.Text = string.Format("{0:00.00}", FrontLeftStanceWidthAvg);
            RFStanceWidthAvg.Text = string.Format("{0:00.00}", FrontRightStanceWidthAvg);

            LHStrideLengthAvg.Text = string.Format("{0:00.00}", HindLeftStrideLenAvg);
            RHStrideLengthAvg.Text = string.Format("{0:00.00}", HindRightStrideLenAvg);
            LFStrideLengthAvg.Text = string.Format("{0:00.00}", FrontLeftStrideLenAvg);
            RFStrideLengthAvg.Text = string.Format("{0:00.00}", FrontRightStrideLenAvg);
        }















        // MARK: Error Correction Interaction

        private void LeftHindChart_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {

            if (HindLeftInStance[GaitCurrentFrame] == 1) HindLeftInStance[GaitCurrentFrame] = 0; //if the user right clicked the chart flipped the 0 (swing) to 1 (stance) or vice versa
            else HindLeftInStance[GaitCurrentFrame] = 1;

            if (!GaitErrorFrames.Contains(GaitCurrentFrame)) GaitErrorFrames.Add(GaitCurrentFrame); //add the corrected frame number to a list so we can keep track of all corrected frames

            ((ObservablePoint)HindLeftObservables[GaitCurrentFrame]).Y = HindLeftInStance[GaitCurrentFrame]; //and update the actual 0 or 1 value in the corresponding chart values (ObservablePoint changes update the chart automatically)
            SetStaticData();
        }

        private void LeftFrontChart_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {

            if (FrontLeftInStance[GaitCurrentFrame] == 1) FrontLeftInStance[GaitCurrentFrame] = 0;
            else FrontLeftInStance[GaitCurrentFrame] = 1;

            if (!GaitErrorFrames.Contains(GaitCurrentFrame)) GaitErrorFrames.Add(GaitCurrentFrame);

            ((ObservablePoint)FrontLeftObservables[GaitCurrentFrame]).Y = FrontLeftInStance[GaitCurrentFrame];
            SetStaticData();
        }

        private void RightHindChart_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
 
            if (HindRightInStance[GaitCurrentFrame] == 1) HindRightInStance[GaitCurrentFrame] = 0;
            else HindRightInStance[GaitCurrentFrame] = 1;

            if (!GaitErrorFrames.Contains(GaitCurrentFrame)) GaitErrorFrames.Add(GaitCurrentFrame);

            ((ObservablePoint)HindRightObservables[GaitCurrentFrame]).Y = HindRightInStance[GaitCurrentFrame];
            SetStaticData();
        }

        private void RightFrontChart_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {

            if (FrontRightInStance[GaitCurrentFrame] == 1) FrontRightInStance[GaitCurrentFrame] = 0;
            else FrontRightInStance[GaitCurrentFrame] = 1;

            if (!GaitErrorFrames.Contains(GaitCurrentFrame)) GaitErrorFrames.Add(GaitCurrentFrame);

            ((ObservablePoint)FrontRightObservables[GaitCurrentFrame]).Y = FrontRightInStance[GaitCurrentFrame];
            SetStaticData();
        }
        



        // MARK: Zooming & Panning Sync
        private void Axis_RangeChanged(LiveCharts.Events.RangeChangedEventArgs eventArgs) {
            double min = ((Axis)eventArgs.Axis).MinValue;
            double max = ((Axis)eventArgs.Axis).MaxValue;

            LeftHindChart.AxisX[0].MinValue = min;
            LeftHindChart.AxisX[0].MaxValue = max;

            LeftFrontChart.AxisX[0].MinValue = min;
            LeftFrontChart.AxisX[0].MaxValue = max;

            RightHindChart.AxisX[0].MinValue = min;
            RightHindChart.AxisX[0].MaxValue = max;

            RightFrontChart.AxisX[0].MinValue = min;
            RightFrontChart.AxisX[0].MaxValue = max;
        }
    }
}
