﻿using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Windows;

namespace VisualGaitLab.GaitAnalysis {
    /// <summary>
    /// Interaction logic for GaitWindow.xaml.
    /// </summary>
    public partial class GaitWindow : Window {
        public GaitWindow(double realWorldMultiplier, float treadmillSpeed, string gaitVideoPath, string gaitVideoName, string gaitTempPath, bool isFreeRun) {
            InitializeComponent();
            RealWorldMultiplier = realWorldMultiplier;
            TreadmillSpeed = treadmillSpeed;
            GaitVideoPath = gaitVideoPath;
            GaitVideoName = gaitVideoName;
            GaitTempPath = gaitTempPath;
            IsFreeRun = isFreeRun;
            SetUpGaitForVid();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {

            if (MessageBox.Show("Do you want to save your data?", "Save Data", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) {
                Console.WriteLine("SAVING");
                SaveCurrentState();
                MessageBox.Show("Your data has been saved, including any error corrections.", "Data Saved", MessageBoxButton.OK);
            }
            this.DialogResult = true;
        }







        // MARK: Auto Correct

        private void GaitAutoCorrectButton_Click(object sender, RoutedEventArgs e) {

            List<int> oldHindLeftInStance = new List<int>(HindLeftInStance);
            List<int> oldHindRightInStance = new List<int>(HindRightInStance);
            List<int> oldFrontLeftInStance = new List<int>(FrontLeftInStance);
            List<int> oldFrontRightInStance = new List<int>(FrontRightInStance);

            autoCorrectInStance(ref HindLeftInStance, ref HindLeftSwitchPositions, ref HindLeftObservables); //correct all actual inStance array values
            autoCorrectInStance(ref HindRightInStance, ref HindRightSwitchPositions, ref HindRightObservables);
            autoCorrectInStance(ref FrontLeftInStance, ref FrontLeftSwitchPositions, ref FrontLeftObservables);
            autoCorrectInStance(ref FrontRightInStance, ref FrontRightSwitchPositions, ref FrontRightObservables);
            SetStaticData();

            var origStroke = ((LineSeries)LeftHindChart.Series[0]).Stroke;
            ((LineSeries)LeftHindChart.Series[0]).Stroke = System.Windows.Media.Brushes.ForestGreen;
            ((LineSeries)RightHindChart.Series[0]).Stroke = System.Windows.Media.Brushes.ForestGreen;
            ((LineSeries)LeftFrontChart.Series[0]).Stroke = System.Windows.Media.Brushes.ForestGreen;
            ((LineSeries)RightFrontChart.Series[0]).Stroke = System.Windows.Media.Brushes.ForestGreen;

            if (MessageBox.Show("Auto-Correction complete, do you want to keep the changes?", "Data Corrected", MessageBoxButton.YesNo, MessageBoxImage.None) == MessageBoxResult.No) { //no - restore original
                HindLeftInStance = oldHindLeftInStance;
                HindRightInStance = oldHindRightInStance;
                FrontLeftInStance = oldFrontLeftInStance;
                FrontRightInStance = oldFrontRightInStance;
                SetStaticData();
                for (int i = 0; i < HindLeftInStance.Count; i++) {
                    ((ObservablePoint)HindLeftObservables[i]).Y = HindLeftInStance[i];
                    ((ObservablePoint)HindRightObservables[i]).Y = HindRightInStance[i];
                    ((ObservablePoint)FrontLeftObservables[i]).Y = FrontLeftInStance[i];
                    ((ObservablePoint)FrontRightObservables[i]).Y = FrontRightInStance[i];
                }
            }
            ((LineSeries)LeftHindChart.Series[0]).Stroke = origStroke; //restore color
            ((LineSeries)RightHindChart.Series[0]).Stroke = origStroke;
            ((LineSeries)LeftFrontChart.Series[0]).Stroke = origStroke;
            ((LineSeries)RightFrontChart.Series[0]).Stroke = origStroke;
        }

        private void autoCorrectInStance(ref List<int> inStanceArray, ref List<int> switches, ref IChartValues observables) {
            int singleSwitchSegmentAvgLen = GaitNumberOfFrames / switches.Count;
            int threshold = singleSwitchSegmentAvgLen / 4; //our clustering threshold is one quarter
            threshold++; //rounding up

            for (int i = 1; i < switches.Count - 2; i++) {
                if (switches[i + 1] - switches[i] < threshold) //if we have a bad one
                {
                    if (switches[i] - switches[i - 1] > threshold && switches[i + 2] - switches[i + 1] > threshold && inStanceArray[switches[i - 1]] == inStanceArray[switches[i + 1]]) //check its immediate neighbors and see if they're good and the same type (stance/swing)
                    {
                        for (int j = switches[i]; j < switches[i + 1]; j++) //if so, correct this bad guy with the neighboring guys' value
                        {
                            inStanceArray[j] = inStanceArray[switches[i - 1]];
                            ((ObservablePoint)observables[j]).Y = inStanceArray[j]; //and update the actual 0 or 1 value in the corresponding chart values (ObservablePoint changes update the chart automatically)
                        }
                    }
                }
            }
        }
    }
}
