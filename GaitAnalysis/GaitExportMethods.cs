using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VisualGaitLab.GaitAnalysis {
    public partial class GaitWindow : Window {

        // MARK: UI Methods

        private void GaitExportButton_Click(object sender, RoutedEventArgs e) {
            PrepSEM();
            SaveCurrentState();
            ExportStaticGaitData();
            ExportDynamicGaitData();
            //ExportCrossCorrelation();
            MessageBox.Show("Both static and dynamic data were exported.", "Data Exported", MessageBoxButton.OK);
            Process.Start("explorer.exe", GaitVideoPath.Substring(0, GaitVideoPath.LastIndexOf("\\")) + "\\gait-export");
        }






        //MARK: Static Data Export Methods

        private void ExportStaticGaitData() { //exports all static data into a single csv file
            var csv = new StringBuilder();
            var newLine = string.Format("{0},{1},{2},{3},{4}", "", "Left Hind", "Right Hind", "Left Fore", "Right Fore");
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Average Stance (ms)", HindLeftAverageStanceDuration, HindRightAverageStanceDuration, FrontLeftAverageStanceDuration, FrontRightAverageStanceDuration);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Standard Error of Mean", HindLeftAverageStanceDurationSEM, HindRightAverageStanceDurationSEM, FrontLeftAverageStanceDurationSEM, FrontRightAverageStanceDurationSEM);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Average Swing (ms)", HindLeftAverageSwingDuration, HindRightAverageSwingDuration, FrontLeftAverageSwingDuration, FrontRightAverageSwingDuration);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Standard Error of Mean", HindLeftAverageSwingDurationSEM, HindRightAverageSwingDurationSEM, FrontLeftAverageSwingDurationSEM, FrontRightAverageSwingDurationSEM);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Average Stride (ms)", HindLeftAverageStanceDuration + HindLeftAverageSwingDuration, HindRightAverageStanceDuration + HindRightAverageSwingDuration, FrontLeftAverageStanceDuration + FrontLeftAverageSwingDuration, FrontRightAverageStanceDuration + FrontRightAverageSwingDuration);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Standard Error of Mean", HindLeftStrideDurationSEM, HindRightStrideDurationSEM, FrontLeftStrideDurationSEM, FrontRightStrideDurationSEM);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Percentage Stance", LHStancePercentageLabel.Text, RHStancePercentageLabel.Text, LFStancePercentageLabel.Text, RFStancePercentageLabel.Text);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Percentage Swing", LHSwingPercentageLabel.Text, RHSwingPercentageLabel.Text, LFSwingPercentageLabel.Text, RFSwingPercentageLabel.Text);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Swing to Stance", LHSwingToStanceLabel.Text, RHSwingToStanceLabel.Text, LFSwingToStanceLabel.Text, RFSwingToStanceLabel.Text);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Stride Frequency (Hz)", LHFrequencyLabel.Text, RHFrequencyLabel.Text, LFFrequencyLabel.Text, RFFrequencyLabel.Text);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Number of Strides", HindLeftNumberOfStrides, HindRightNumberOfStrides, FrontLeftNumberOfStrides, FrontRightNumberOfStrides);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Stride Length Variability", LHStrideLengthVariabilityLabel.Text, RHStrideLengthVariabilityLabel.Text, LFStrideLengthVariabilityLabel.Text, RFStrideLengthVariabilityLabel.Text);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Gait Symmetry", LFGaitSymmetryLabel.Text, LFGaitSymmetryLabel.Text, LFGaitSymmetryLabel.Text, LFGaitSymmetryLabel.Text);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Paw Angle Avg (deg)", HindLeftPawAngleAvg, HindRightPawAngleAvg, FrontLeftPawAngleAvg, FrontRightPawAngleAvg);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Standard Error of Mean", HindLeftPawAngleSEM, HindRightPawAngleSEM, FrontLeftPawAngleSEM, FrontRightPawAngleSEM);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Stance Width Avg (mm)", HindStanceWidthAvg, HindStanceWidthAvg, ForeStanceWidthAvg, ForeStanceWidthAvg);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Standard Error of Mean", HindStanceWidthSEM, HindStanceWidthSEM, ForeStanceWidthSEM, ForeStanceWidthSEM);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Stride Length Avg (mm)", HindLeftStrideLenAvg, HindRightStrideLenAvg, FrontLeftStrideLenAvg, FrontRightStrideLenAvg);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Standard Error of Mean", HindLeftStrideLenSEM, HindRightStrideLenSEM, FrontLeftStrideLenSEM, FrontRightStrideLenSEM);
            csv.AppendLine(newLine);

            if (!Directory.Exists(GaitVideoPath.Substring(0, GaitVideoPath.LastIndexOf("\\")) + "\\gait-export")) {
                Directory.CreateDirectory(GaitVideoPath.Substring(0, GaitVideoPath.LastIndexOf("\\")) + "\\gait-export");
            }
            File.WriteAllText(GaitVideoPath.Substring(0, GaitVideoPath.LastIndexOf("\\")) + "\\gait-export\\staticdata.csv", csv.ToString());
        }











        // MARK: Dynamic Data Export Methods

        private void ExportDynamicGaitData() { //exports all dynamic data into separate CSVs by paw and stride (Eg. hindleft_stride3, frontleft_stride2, etc.)
            StringBuilder csv;
            string newLine;
            for (int i = 0; i < HindLeftNumberOfStrides; i++) { //Hind Left
                csv = new StringBuilder();

                //headers
                newLine = string.Format("{0},{1},{2},{3},{4}", "Frame", "Stride Length", "Paw Angle", "Stance Width", "Step Angle");
                csv.AppendLine(newLine);

                //values for each Frame for a given stride
                for (int currentFrame = HindLeftSwitchPositions[i * 2]; currentFrame < HindLeftSwitchPositions[i * 2 + 2]; currentFrame++) {
                    string formattedAngle = ""; //empty by default
                    CalculateCenterOfMass(currentFrame); //TODO: don't remember why I'm calculating the angle all over again
                    if (HindLeftInStance[currentFrame] == 1) //if it's stance actually caluclate the angle, else just leave it blank (because it's swing and we don't want to include those values)
                    {
                        double angle = GetPawAngle(HindLeftYs[currentFrame], HindLeftHeelYs[currentFrame], HindLeftXs[currentFrame], HindLeftHeelXs[currentFrame], CenterOfMassY, SuperButtYs[currentFrame], CenterOfMassX, SuperButtXs[currentFrame]);
                        formattedAngle = string.Format("{0:00.00}", angle);
                    }

                    float midPointTop = (HindRightHeelYs[currentFrame] + HindRightYs[currentFrame]) / 2;
                    float midPointBottom = (HindLeftHeelYs[currentFrame] + HindLeftYs[currentFrame]) / 2;
                    string formattedStanceWidth = string.Format("{0:00.00}", (midPointTop - midPointBottom) * RealWorldMultiplier);

                    newLine = string.Format("{0},{1},{2},{3},{4}", currentFrame, HindLeftStrides[i], formattedAngle, formattedStanceWidth, "");
                    csv.AppendLine(newLine);
                }

                File.WriteAllText(GaitVideoPath.Substring(0, GaitVideoPath.LastIndexOf("\\")) + "\\gait-export\\" + "hindleft_stride" + i + ".csv", csv.ToString());
            }

            for (int i = 0; i < HindRightNumberOfStrides; i++) { //Hind Right
                csv = new StringBuilder();

                //headers
                newLine = string.Format("{0},{1},{2},{3},{4}", "Frame", "Stride Length", "Paw Angle", "Stance Width", "Step Angle");
                csv.AppendLine(newLine);

                //values for each Frame for a given stride
                for (int currentFrame = HindRightSwitchPositions[i * 2]; currentFrame < HindRightSwitchPositions[i * 2 + 2]; currentFrame++) {
                    string formattedAngle = "";
                    CalculateCenterOfMass(currentFrame);
                    if (HindRightInStance[currentFrame] == 1) {
                        double angle = GetPawAngle(HindRightYs[currentFrame], HindRightHeelYs[currentFrame], HindRightXs[currentFrame], HindRightHeelXs[currentFrame], CenterOfMassY, SuperButtYs[currentFrame], CenterOfMassX, SuperButtXs[currentFrame]);
                        formattedAngle = string.Format("{0:00.00}", angle);
                    }

                    float midPointTop = (HindRightHeelYs[currentFrame] + HindRightYs[currentFrame]) / 2;
                    float midPointBottom = (HindLeftHeelYs[currentFrame] + HindLeftYs[currentFrame]) / 2;
                    string formattedStanceWidth = string.Format("{0:00.00}", (midPointTop - midPointBottom) * RealWorldMultiplier);

                    newLine = string.Format("{0},{1},{2},{3},{4}", currentFrame, HindRightStrides[i], formattedAngle, formattedStanceWidth, "");
                    csv.AppendLine(newLine);
                }

                File.WriteAllText(GaitVideoPath.Substring(0, GaitVideoPath.LastIndexOf("\\")) + "\\gait-export\\" + "hindright_stride" + i + ".csv", csv.ToString());
            }

            for (int i = 0; i < FrontLeftNumberOfStrides; i++) { //Front Left
                csv = new StringBuilder();

                //headers
                newLine = string.Format("{0},{1},{2},{3},{4}", "Frame", "Stride Length", "Paw Angle", "Stance Width", "Step Angle");
                csv.AppendLine(newLine);

                //values for each Frame for a given stride
                for (int currentFrame = FrontLeftSwitchPositions[i * 2]; currentFrame < FrontLeftSwitchPositions[i * 2 + 2]; currentFrame++) {
                    string formattedAngle = "";
                    CalculateCenterOfMass(currentFrame);
                    if (FrontLeftInStance[currentFrame] == 1) {
                        double angle = GetPawAngle(FrontLeftYs[currentFrame], FrontLeftHeelYs[currentFrame], FrontLeftXs[currentFrame], FrontLeftHeelXs[currentFrame], NoseYs[currentFrame], CenterOfMassY, NoseXs[currentFrame], CenterOfMassX);
                        formattedAngle = string.Format("{0:00.00}", angle);
                    }

                    float midPointTop = (FrontRightHeelYs[currentFrame] + FrontRightYs[currentFrame]) / 2;
                    float midPointBottom = (FrontLeftHeelYs[currentFrame] + FrontLeftYs[currentFrame]) / 2;
                    string formattedStanceWidth = string.Format("{0:00.00}", (midPointTop - midPointBottom) * RealWorldMultiplier);

                    newLine = string.Format("{0},{1},{2},{3},{4}", currentFrame, FrontLeftStrides[i], formattedAngle, formattedStanceWidth, "");
                    csv.AppendLine(newLine);
                }

                File.WriteAllText(GaitVideoPath.Substring(0, GaitVideoPath.LastIndexOf("\\")) + "\\gait-export\\" + "frontleft_stride" + i + ".csv", csv.ToString());
            }

            for (int i = 0; i < FrontRightNumberOfStrides; i++) { //Front Right
                csv = new StringBuilder();

                //headers
                newLine = string.Format("{0},{1},{2},{3},{4}", "Frame", "Stride Length", "Paw Angle", "Stance Width", "Step Angle");
                csv.AppendLine(newLine);

                //values for each Frame for a given stride
                for (int currentFrame = FrontRightSwitchPositions[i * 2]; currentFrame < FrontRightSwitchPositions[i * 2 + 2]; currentFrame++) {
                    string formattedAngle = "";
                    CalculateCenterOfMass(currentFrame);
                    if (FrontRightInStance[currentFrame] == 1) {
                        double angle = GetPawAngle(FrontRightYs[currentFrame], FrontRightHeelYs[currentFrame], FrontRightXs[currentFrame], FrontRightHeelXs[currentFrame], NoseYs[currentFrame], CenterOfMassY, NoseXs[currentFrame], CenterOfMassX);
                        formattedAngle = string.Format("{0:00.00}", angle);
                    }

                    float midPointTop = (FrontRightHeelYs[currentFrame] + FrontRightYs[currentFrame]) / 2;
                    float midPointBottom = (FrontLeftHeelYs[currentFrame] + FrontLeftYs[currentFrame]) / 2;
                    string formattedStanceWidth = string.Format("{0:00.00}", (midPointTop - midPointBottom) * RealWorldMultiplier);

                    newLine = string.Format("{0},{1},{2},{3},{4}", currentFrame, FrontRightStrides[i], formattedAngle, formattedStanceWidth, "");
                    csv.AppendLine(newLine);
                }

                File.WriteAllText(GaitVideoPath.Substring(0, GaitVideoPath.LastIndexOf("\\")) + "\\gait-export\\" + "frontright_stride" + i + ".csv", csv.ToString());
            }
        }










        // MARK: Export Cross Correlation Data - NOT IN USE CURRENTLY

        private void ExportCrossCorrelation() {
            StringBuilder csv;
            string newLine;
            csv = new StringBuilder();

            newLine = string.Format("{0},{1},{2},{3},{4}", "Index", "Hind Paws", "Fore Paws", "Left Hind to Right Fore", "Right Hind to Left Fore"); //headers
            csv.AppendLine(newLine);

            for (int i = 0; i < HindLimbsCrossCorrelation.Count; i++) {
                newLine = string.Format("{0},{1},{2},{3},{4}", i, HindLimbsCrossCorrelation[i], ForeLimbsCrossCorrelation[i], HindLeftToForeRightCC[i], HindRightToForeLeftCC[i]);
                csv.AppendLine(newLine);
            }

            File.WriteAllText(GaitVideoPath.Substring(0, GaitVideoPath.LastIndexOf("\\")) + "\\gait-export\\" + "cross_correlation.csv", csv.ToString());
        }
    }
}
