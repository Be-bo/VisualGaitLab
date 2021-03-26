using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace VisualGaitLab.GaitAnalysis {
    public partial class GaitWindow : Window {







        // MARK: Gait Video Setup

        private void SetUpGaitForVid() {
            string file = Directory.EnumerateFiles(GaitTempPath).First(); //get the first individual frame of the analyzed video
            var files = Directory.EnumerateFiles(GaitTempPath);
            foreach(var currentFile in files) { //just in case there's random stuff in the folder
                if (currentFile.Contains(".png")) {
                    file = currentFile;
                    break;
                }
            }
            BitmapImage bitmap = new BitmapImage(new Uri(file, UriKind.Absolute));
            file = file.Substring(file.LastIndexOf("\\") + 1, file.LastIndexOf("g") - file.LastIndexOf("\\"));
            NumberOfDigits = file.Length - 8; //determine the number of digits (i.e. 4 digits if the last frame is "file4567.png" - all frames < 1000 are padded with 0s) and subtract 8 characters (for "file" and ".png")
            GaitVideoHeight = bitmap.PixelHeight;

            TimeSpan ts; //use Windows API Codepack to determine the length of the currently selected Gait video
            using (var shell = ShellObject.FromParsingName(GaitVideoPath)) {
                IShellProperty prop = shell.Properties.System.Media.Duration;
                var t = (ulong)prop.ValueAsObject;
                ts = TimeSpan.FromTicks((long)t);
            }

            if (ts != null) { //if we successfully managed to obtain the length let the program proceed
                GaitVideoLength = ParseDuration(ts.ToString());
                ReadResults();
                ReadCurrentState(true);
                SetUpPawCharts();
                SetStaticData();
                SetUpCrossCorrelationCharts();
                UpdateFrame(false);
            }
            else {
                MessageBox.Show("Failed to load the video.", "Error", MessageBoxButton.OK); //quit the program otherwise
                this.Close();
            }
        }

        private void ReadResults() { //parse the .csv files with tracking results
            string resultsPath = "";
            var files = Directory.EnumerateFiles(GaitVideoPath.Substring(0, GaitVideoPath.LastIndexOf("\\")));
            foreach (string current in files) if (current.Contains(".csv")) { resultsPath = current; break; }

            if (!resultsPath.Equals("")) {
                using (var streamReader = new StreamReader(resultsPath)) {
                    String[] rows = Regex.Split(streamReader.ReadToEnd(), "\r\n");
                    for (int i = 3; i < rows.Count(); i++) {
                        if (rows[i].Length > 5) //to eliminate empty spaces at the end
                        {
                            String[] splitLine = rows[i].Split(',');

                            RightMidPointXs.Add(float.Parse(splitLine[1])); //for now we're skipping probability -> stepping over every third column
                            RightMidPointYs.Add(GaitVideoHeight - float.Parse(splitLine[2]));
                            LeftMidPointXs.Add(float.Parse(splitLine[4]));
                            LeftMidPointYs.Add(GaitVideoHeight - float.Parse(splitLine[5]));
                            SuperButtXs.Add(float.Parse(splitLine[7]));
                            SuperButtYs.Add(GaitVideoHeight - float.Parse(splitLine[8]));
                            NoseXs.Add(float.Parse(splitLine[10]));
                            NoseYs.Add(GaitVideoHeight - float.Parse(splitLine[11]));

                            FrontRightHeelXs.Add(float.Parse(splitLine[13])); //heel marker
                            FrontRightHeelYs.Add(GaitVideoHeight - float.Parse(splitLine[14]));
                            FrontRightXs.Add(float.Parse(splitLine[16])); //toe marker
                            FrontRightYs.Add(GaitVideoHeight - float.Parse(splitLine[17]));

                            FrontLeftHeelXs.Add(float.Parse(splitLine[19]));
                            FrontLeftHeelYs.Add(GaitVideoHeight - float.Parse(splitLine[20]));
                            FrontLeftXs.Add(float.Parse(splitLine[22]));
                            FrontLeftYs.Add(GaitVideoHeight - float.Parse(splitLine[23]));

                            HindRightHeelXs.Add(float.Parse(splitLine[25]));
                            HindRightHeelYs.Add(GaitVideoHeight - float.Parse(splitLine[26]));
                            HindRightXs.Add(float.Parse(splitLine[28]));
                            HindRightYs.Add(GaitVideoHeight - float.Parse(splitLine[29]));

                            HindLeftHeelXs.Add(float.Parse(splitLine[31]));
                            HindLeftHeelYs.Add(GaitVideoHeight - float.Parse(splitLine[32]));
                            HindLeftXs.Add(float.Parse(splitLine[34]));
                            HindLeftYs.Add(GaitVideoHeight - float.Parse(splitLine[35]));
                        }
                    }
                }
            }
        }












        // MARK: Chart Setup Methods

        private void SetUpPawCharts() { //set up the primary top 4 charts corresponding to mouse's paws

            // Temporary arrays to apply same operation to each component
            var charts = new CartesianChart[] { LeftHindChart, LeftFrontChart, RightHindChart, RightFrontChart };
            var observables = new IChartValues[] { HindLeftObservables, FrontLeftObservables, HindRightObservables, FrontRightObservables };
            var scatters = new IChartValues[] { HindLeftScatter, FrontLeftScatter, HindRightScatter, FrontRightScatter };

            GaitNumberOfFrames = HindLeftInStance.Count; //a few more values we need to initialize but needed to parse the .csv for first
            XMAX = GaitNumberOfFrames;
            FPS = (int)(GaitNumberOfFrames / GaitVideoLength);


            if (GaitFirstSetup)
            { // First time setup

                // Set up zooming along the X axis for all paw charts
                foreach (var chart in charts) chart.Zoom = ZoomingOptions.X;

                // Add a scatter plot to each chart with a single value
                // (it will be moving through the chart as the current frame updates to indicate to the user which x value corresponds to the frame they're seeing)
                foreach (var scatter in scatters) scatter.Add(new ObservablePoint());

                GaitFirstSetup = false;
            }
            else
            { // Reset Charts
                for (int i = 0; i < charts.Length; i++)
                {
                    charts[i].Series.Clear();
                    observables[i].Clear();
                }
            }


            for (int i = 0; i < HindLeftInStance.Count; i++) { //set the .csv results 
                HindLeftObservables.Add(new ObservablePoint(i, HindLeftInStance[i])); //set up list values for all charts as Observable Points (chart will automatically update if there's a change to any of its values)
                HindRightObservables.Add(new ObservablePoint(i, HindRightInStance[i]));
                FrontLeftObservables.Add(new ObservablePoint(i, FrontLeftInStance[i]));
                FrontRightObservables.Add(new ObservablePoint(i, FrontRightInStance[i]));
            }
            

            // Clear the x an y axes that are created by default and add custom ones
            PrimaryGaitGrid.Visibility = Visibility.Visible;

            foreach (var chart in charts)
            {
                chart.AxisX.Clear();
                chart.AxisY.Clear();
            }

            // Set Charts (leftHind, LeftFront, RightHind, RightFront)
            for (int i = 0; i < charts.Length; i++)
            {
                charts[i].AxisX.Add(new Axis
                {
                    MaxValue = XMAX,
                    MinValue = XMIN
                });
                ((Axis)(charts[i].AxisX[0])).RangeChanged += new LiveCharts.Events.RangeChangedHandler(Axis_RangeChanged); //sync zooming

                charts[i].AxisY.Add(new Axis
                {
                    MaxValue = YMAX,
                    MinValue = YMIN,
                    LabelFormatter = val => val == 0 ? "Swing" : "Stance"
                });

                charts[i].Series.Add(new LineSeries
                { //also add all the values that indicate if the mouse is in stance or swing
                    Values = observables[i],
                    PointGeometrySize = 0,
                    LineSmoothness = 0,
                });

                charts[i].Series.Add(new ScatterSeries
                { //and add the single point that shows which x value we're looking at (i.e. which x corresponds to the current frame)
                    Values = scatters[i],
                    PointGeometry = DefaultGeometries.Circle,
                });
            }

            FrameSlider.Value = 0; //move to the beginning with the slider
            FrameSlider.Maximum = GaitNumberOfFrames - 1; //sets its maximum
            ZoomSliderEndBox.Text = (GaitNumberOfFrames - 1).ToString(); //and update the zoom slider's end box (upper boundary of the zoom segment) with last frame's index
        }















        // MARK: Cross Correlation Methods - CURRENTLY NOT IN USE

        private void SetUpCrossCorrelationCharts() { //same setup as the 4 paw charts above except we're displaying cross correlation values so no scatter plot indicating the current frame is necesssary
            CalculateCrossCorrelationSeries(ref HindLeftInStance, ref HindRightInStance, ref HindLimbsCrossCorrelation); //the cross correlation values also need to be calculated first
            CalculateCrossCorrelationSeries(ref FrontLeftInStance, ref FrontRightInStance, ref ForeLimbsCrossCorrelation);
            CalculateCrossCorrelationSeries(ref HindLeftInStance, ref FrontRightInStance, ref HindLeftToForeRightCC);
            CalculateCrossCorrelationSeries(ref HindRightInStance, ref FrontLeftInStance, ref HindRightToForeLeftCC);

            List<string> labels = new List<string>(); //livecharts won't let us choose a single X axis value and label it, has to be done by having labels for every single point
            foreach (double cur in HindLimbsCrossCorrelation) labels.Add("");
            labels[GaitNumberOfFrames - 1] = "Mid Point (" + (GaitNumberOfFrames - 1) + ")";

            LiveCharts.Wpf.Separator separator = new LiveCharts.Wpf.Separator(); //necessary to show the label properly
            separator.StrokeThickness = 0;
            separator.Step = 1;


            if (HindCorrelationChart.Series.Count < 1) { //SETTING UP FOR THE FIRST TIME
                HindCorrelationChart.AxisX.Clear();
                HindCorrelationChart.AxisY.Clear();
                ForeCorrelationChart.AxisX.Clear();
                ForeCorrelationChart.AxisY.Clear();
                LHRFCorrelationChart.AxisX.Clear();
                LHRFCorrelationChart.AxisY.Clear();
                RHLFCorrelationChart.AxisX.Clear();
                RHLFCorrelationChart.AxisY.Clear();

                //CORRELATION CURRENTLY NOT BEING DISPLAYED IN THE UI BECAUSE IT'S INCORRECT

                // hind limbs correlation
                HindCorrelationChart.AxisX.Add(new Axis {
                    MaxValue = HindLimbsCrossCorrelation.Count,
                    MinValue = 0,
                    Labels = labels,
                    Separator = new LiveCharts.Wpf.Separator { Step = 1, StrokeThickness = 0 }
                });

                HindCorrelationChart.AxisY.Add(new Axis {
                    MaxValue = 0.5,
                    MinValue = -0.5
                });

                HindCorrelationChart.Series.Add(new LineSeries {
                    Values = new ChartValues<double>(HindLimbsCrossCorrelation),
                    PointGeometrySize = 0,
                    LineSmoothness = 0,
                });

                //front limbs correlation
                ForeCorrelationChart.AxisX.Add(new Axis {
                    MaxValue = ForeLimbsCrossCorrelation.Count,
                    MinValue = 0,
                    Labels = labels,
                    Separator = new LiveCharts.Wpf.Separator { Step = 1, StrokeThickness = 0 }
                });

                ForeCorrelationChart.AxisY.Add(new Axis {
                    MaxValue = 0.5,
                    MinValue = -0.5
                });

                ForeCorrelationChart.Series.Add(new LineSeries {
                    Values = new ChartValues<double>(ForeLimbsCrossCorrelation),
                    PointGeometrySize = 0,
                    LineSmoothness = 0,
                });


                //hind left to fore right correlation
                LHRFCorrelationChart.AxisX.Add(new Axis {
                    MaxValue = HindLeftToForeRightCC.Count,
                    MinValue = 0,
                    Labels = labels,
                    Separator = new LiveCharts.Wpf.Separator { Step = 1, StrokeThickness = 0 }
                });

                LHRFCorrelationChart.AxisY.Add(new Axis {
                    MaxValue = 0.5,
                    MinValue = -0.5
                });

                LHRFCorrelationChart.Series.Add(new LineSeries {
                    Values = new ChartValues<double>(HindLeftToForeRightCC),
                    PointGeometrySize = 0,
                    LineSmoothness = 0,
                });


                //hind right to fore left correlation
                RHLFCorrelationChart.AxisX.Add(new Axis {
                    MaxValue = HindRightToForeLeftCC.Count,
                    MinValue = 0,
                    Labels = labels,
                    Separator = new LiveCharts.Wpf.Separator { Step = 1, StrokeThickness = 0 }
                });

                RHLFCorrelationChart.AxisY.Add(new Axis {
                    MaxValue = 0.5,
                    MinValue = -0.5
                });

                RHLFCorrelationChart.Series.Add(new LineSeries {
                    Values = new ChartValues<double>(HindRightToForeLeftCC),
                    PointGeometrySize = 0,
                    LineSmoothness = 0,
                });
            }
        }










        // MARK: Supporting Methods

        private double ParseDuration(String stringDuration) { //parse the duration from the format (HH:MM:SS) that the API Code Pack returns to a floating point number in seconds
            string hours = stringDuration.Substring(0, 2);
            string minutes = stringDuration.Substring((stringDuration.IndexOf(":") + 1), 2);
            string seconds = stringDuration.Substring(stringDuration.LastIndexOf(":") + 1, (stringDuration.Length - (stringDuration.LastIndexOf(":") + 1)));
            double totalInSeconds = double.Parse(hours) * 3600 + double.Parse(minutes) * 60 + double.Parse(seconds);
            return totalInSeconds;
        }






        // MARK: Reset midPoints and InStance values

        private void ResetCalculatedFields()
        {
            foreach (var midpoints in new List<double>[] { HindLeftMidPointXs, HindLeftMidPointYs, HindRightMidPointXs, HindRightMidPointYs,
                                                           FrontLeftMidPointXs, FrontLeftMidPointYs, FrontRightMidPointXs, FrontRightMidPointYs })
            {
                midpoints.Clear();
            }
            foreach (var inStances in new List<int>[] { HindLeftInStance, HindRightInStance, FrontLeftInStance, FrontRightInStance })
            {
                inStances.Clear();
            }
        }
    }
}
