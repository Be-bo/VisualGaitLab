using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VisualGaitLab.GaitAnalysis {
    public partial class GaitWindow : Window {






        // MARK: Saving State Methods (only includes in stance arrays, everything else gets recalculated)

        private void GaitSaveButton_Click(object sender, RoutedEventArgs e) {
            SaveCurrentState();
            MessageBox.Show("Your data has been saved, including any error corrections.", "Data Saved", MessageBoxButton.OK);
        }

        private void SaveCurrentState() {
            string stateFolder = GaitVideoPath.Substring(0, GaitVideoPath.LastIndexOf("\\")) + "\\gaitsavedstate";
            Directory.CreateDirectory(stateFolder);

            //saving input params
            List<string> tempList = new List<string>();
            tempList.Add(RealWorldMultiplier.ToString());
            tempList.Add(TreadmillSpeed.ToString());
            if (IsFreeRun) tempList.Add("1");
            else tempList.Add("0");
            System.IO.File.WriteAllLines(stateFolder + "\\inputParams.txt", tempList);

            //saving in stance lists to a separate folder
            tempList = HindLeftInStance.ConvertAll(item => item.ToString());
            System.IO.File.WriteAllLines(stateFolder + "\\HindLeftInStance.txt", tempList);

            tempList = HindRightInStance.ConvertAll(item => item.ToString());
            System.IO.File.WriteAllLines(stateFolder + "\\HindRightInStance.txt", tempList);

            tempList = FrontLeftInStance.ConvertAll(item => item.ToString());
            System.IO.File.WriteAllLines(stateFolder + "\\FrontLeftInStance.txt", tempList);

            tempList = FrontRightInStance.ConvertAll(item => item.ToString());
            System.IO.File.WriteAllLines(stateFolder + "\\FrontRightInStance.txt", tempList);



            tempList = HindLeftMidPointXs.ConvertAll(item => item.ToString());
            System.IO.File.WriteAllLines(stateFolder + "\\HindLeftMidPointXs.txt", tempList);

            tempList = HindLeftMidPointYs.ConvertAll(item => item.ToString());
            System.IO.File.WriteAllLines(stateFolder + "\\HindLeftMidPointYs.txt", tempList);

            tempList = HindRightMidPointXs.ConvertAll(item => item.ToString());
            System.IO.File.WriteAllLines(stateFolder + "\\HindRightMidPointXs.txt", tempList);

            tempList = HindRightMidPointYs.ConvertAll(item => item.ToString());
            System.IO.File.WriteAllLines(stateFolder + "\\HindRightMidPointYs.txt", tempList);

            tempList = FrontLeftMidPointXs.ConvertAll(item => item.ToString());
            System.IO.File.WriteAllLines(stateFolder + "\\FrontLeftMidPointXs.txt", tempList);

            tempList = FrontLeftMidPointYs.ConvertAll(item => item.ToString());
            System.IO.File.WriteAllLines(stateFolder + "\\FrontLeftMidPointYs.txt", tempList);

            tempList = FrontRightMidPointXs.ConvertAll(item => item.ToString());
            System.IO.File.WriteAllLines(stateFolder + "\\FrontRightMidPointXs.txt", tempList);

            tempList = FrontRightMidPointYs.ConvertAll(item => item.ToString());
            System.IO.File.WriteAllLines(stateFolder + "\\FrontRightMidPointYs.txt", tempList);

            SaveMetrics(stateFolder);
        }

        private void ReadCurrentState() {
            string stateFolder = GaitVideoPath.Substring(0, GaitVideoPath.LastIndexOf("\\")) + "\\gaitsavedstate"; //reading in stance lists from the folder
            if (Directory.Exists(stateFolder) && File.Exists(stateFolder + "\\HindLeftInStance.txt") && File.Exists(stateFolder + "\\HindRightInStance.txt") && File.Exists(stateFolder + "\\FrontLeftInStance.txt") && File.Exists(stateFolder + "\\FrontRightInStance.txt")) {
                HindLeftInStance = File.ReadAllLines(stateFolder + "\\HindLeftInStance.txt").ToList().ConvertAll(item => int.Parse(item));
                HindRightInStance = File.ReadAllLines(stateFolder + "\\HindRightInStance.txt").ToList().ConvertAll(item => int.Parse(item));
                FrontLeftInStance = File.ReadAllLines(stateFolder + "\\FrontLeftInStance.txt").ToList().ConvertAll(item => int.Parse(item));
                FrontRightInStance = File.ReadAllLines(stateFolder + "\\FrontRightInStance.txt").ToList().ConvertAll(item => int.Parse(item));


                HindLeftMidPointXs = File.ReadAllLines(stateFolder + "\\HindLeftMidPointXs.txt").ToList().ConvertAll(item => double.Parse(item));
                HindLeftMidPointYs = File.ReadAllLines(stateFolder + "\\HindLeftMidPointYs.txt").ToList().ConvertAll(item => double.Parse(item));

                HindRightMidPointXs = File.ReadAllLines(stateFolder + "\\HindRightMidPointXs.txt").ToList().ConvertAll(item => double.Parse(item));
                HindRightMidPointYs = File.ReadAllLines(stateFolder + "\\HindRightMidPointYs.txt").ToList().ConvertAll(item => double.Parse(item));

                FrontLeftMidPointXs = File.ReadAllLines(stateFolder + "\\FrontLeftMidPointXs.txt").ToList().ConvertAll(item => double.Parse(item));
                FrontLeftMidPointYs = File.ReadAllLines(stateFolder + "\\FrontLeftMidPointYs.txt").ToList().ConvertAll(item => double.Parse(item));

                FrontRightMidPointXs = File.ReadAllLines(stateFolder + "\\FrontRightMidPointXs.txt").ToList().ConvertAll(item => double.Parse(item));
                FrontRightMidPointYs = File.ReadAllLines(stateFolder + "\\FrontRightMidPointYs.txt").ToList().ConvertAll(item => double.Parse(item));
                MessageBox.Show("Loaded saved data from previous session.", "Data Loaded", MessageBoxButton.OK);
            }
            else {
                HindLeftInStance = GetInStanceArray(HindLeftXs, HindLeftYs, HindLeftHeelXs, HindLeftHeelYs, ref HindLeftMidPointXs, ref HindLeftMidPointYs); //calculate in stance arrays for all paws (0 means that in the current frame the paw is in swing and 1 means that it's in stance)
                HindRightInStance = GetInStanceArray(HindRightXs, HindRightYs, HindRightHeelXs, HindRightHeelYs, ref HindRightMidPointXs, ref HindRightMidPointYs);
                FrontLeftInStance = GetInStanceArray(FrontLeftXs, FrontLeftYs, FrontLeftHeelXs, FrontLeftHeelYs, ref FrontLeftMidPointXs, ref FrontLeftMidPointYs);
                FrontRightInStance = GetInStanceArray(FrontRightXs, FrontRightYs, FrontRightHeelXs, FrontRightHeelYs, ref FrontRightMidPointXs, ref FrontRightMidPointYs);
            }
        }










        // MARK: Saving Metrics Methods (same data user can export except they might want to combine this video with other videos in the Gait tab -> we're saving the export data in here for that purpose)

        private void SaveMetrics(string stateFolder) {
            using (var writer = new StreamWriter(stateFolder + "\\metrics.txt")) {
                //avg stance
                writer.WriteLine(HindLeftAverageStanceDuration);
                writer.WriteLine(HindRightAverageStanceDuration);
                writer.WriteLine(FrontLeftAverageStanceDuration);
                writer.WriteLine(FrontRightAverageStanceDuration);

                //avg swing
                writer.WriteLine(HindLeftAverageSwingDuration);
                writer.WriteLine(HindRightAverageSwingDuration);
                writer.WriteLine(FrontLeftAverageSwingDuration);
                writer.WriteLine(FrontRightAverageSwingDuration);

                //avg stride
                writer.WriteLine(HindLeftAverageSwingDuration + HindLeftAverageStanceDuration);
                writer.WriteLine(HindRightAverageSwingDuration + HindRightAverageStanceDuration);
                writer.WriteLine(FrontLeftAverageSwingDuration + FrontLeftAverageStanceDuration);
                writer.WriteLine(FrontRightAverageSwingDuration + FrontRightAverageStanceDuration);

                //% stance
                writer.WriteLine(LHStancePercentageLabel.Text);
                writer.WriteLine(RHStancePercentageLabel.Text);
                writer.WriteLine(LFStancePercentageLabel.Text);
                writer.WriteLine(RFStancePercentageLabel.Text);

                //% swing
                writer.WriteLine(LHSwingPercentageLabel.Text);
                writer.WriteLine(RHSwingPercentageLabel.Text);
                writer.WriteLine(LFSwingPercentageLabel.Text);
                writer.WriteLine(RFSwingPercentageLabel.Text);

                //swing/stance
                writer.WriteLine(LHSwingToStanceLabel.Text);
                writer.WriteLine(RHSwingToStanceLabel.Text);
                writer.WriteLine(LFSwingToStanceLabel.Text);
                writer.WriteLine(RFSwingToStanceLabel.Text);

                //stride frequency
                writer.WriteLine(LHFrequencyLabel.Text);
                writer.WriteLine(RHFrequencyLabel.Text);
                writer.WriteLine(LFFrequencyLabel.Text);
                writer.WriteLine(RFFrequencyLabel.Text);

                //# of strides
                writer.WriteLine(HindLeftNumberOfStrides);
                writer.WriteLine(HindRightNumberOfStrides);
                writer.WriteLine(FrontLeftNumberOfStrides);
                writer.WriteLine(FrontRightNumberOfStrides);

                //stride len variability
                writer.WriteLine(HindLeftStrideLengthVariablity);
                writer.WriteLine(HindRightStrideLengthVariability);
                writer.WriteLine(FrontLeftStrideLengthVariability);
                writer.WriteLine(FrontRightStrideLengthVariability);

                //Gait Symmetry
                writer.WriteLine(LFGaitSymmetryLabel.Text);

                //Avg Paw Angles
                writer.WriteLine(HindLeftPawAngleAvg);
                writer.WriteLine(HindRightPawAngleAvg);
                writer.WriteLine(FrontLeftPawAngleAvg);
                writer.WriteLine(FrontRightPawAngleAvg);

                //Avg Stance Widths
                writer.WriteLine(HindStanceWidthAvg);
                writer.WriteLine(ForeStanceWidthAvg);

                //Avg Stride Lengths
                writer.WriteLine(HindLeftStrideLenAvg);
                writer.WriteLine(HindRightStrideLenAvg);
                writer.WriteLine(FrontLeftStrideLenAvg);
                writer.WriteLine(FrontRightStrideLenAvg);
            }
        }
    }
}
