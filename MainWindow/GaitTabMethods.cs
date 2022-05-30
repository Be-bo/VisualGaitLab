using System.Collections.Generic;
using System.Windows;
using VisualGaitLab.SupportingClasses;
using VisualGaitLab.GaitAnalysis;
using System.IO;
using System.Windows.Controls;
using VisualGaitLab.OtherWindows;
using System.Linq;
using System;
using System.Text;
using System.Diagnostics;

namespace VisualGaitLab
{
    public partial class MainWindow : Window
    {
        private void PrepareGaitTab()
        { //set up the gait tab = grab the analyzed videos from the analysis tab and make them show up here + set up logic for drag and drop to the other list box (combined gait export feature)
            GaitVideos = new List<AnalysisVideo>();
            GaitCombineListBox.AllowDrop = true;
            GaitCombinedVideos = new List<AnalysisVideo>();
            CombineResultsButton.IsEnabled = false;
            foreach (AnalysisVideo vid in CurrentProject.AnalysisVideos)
            { //the combo box will contain all analyzed videos from the "Analyze" tab
                if (vid.IsAnalyzed)
                {
                    string gaitStateFolder = vid.Path.Substring(0, vid.Path.LastIndexOf("\\")) + "\\gaitsavedstate";
                    if (Directory.Exists(gaitStateFolder) && File.Exists(gaitStateFolder + "\\metrics.txt"))
                    {
                        vid.GaitAnalyzedImageName = "Images/check.png";
                        vid.IsGaitAnalyzed = true;
                    }
                    else
                    {
                        vid.GaitAnalyzedImageName = "Images/cross.png";
                        vid.IsGaitAnalyzed = false;
                    }
                    GaitVideos.Add(vid);
                }
            }

            if (GaitVideos.Count > 0)
            { //if there are analyzed videos allow access to the "Gait" tab
                GaitListBox.ItemsSource = null;
                GaitListBox.ItemsSource = GaitVideos;
                GaitTab.IsEnabled = true;
            }
            else GaitTab.IsEnabled = false; //if there are none disable it
        }

        private void GaitListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        { //change button description depending on whether gait has been analyzed on a particular video
            AnalysisVideo selectedVid = (AnalysisVideo)GaitListBox.SelectedItem;
            if (selectedVid != null)
            {
                AnalyzeGaitButton.IsEnabled = true;
                if (selectedVid.IsGaitAnalyzed)
                {
                    AnalyzeGaitButton.Content = "View/Edit";
                }
                else
                {
                    AnalyzeGaitButton.Content = "Analyze Gait";
                }
            }
            else AnalyzeGaitButton.IsEnabled = false;
        }

        private void AnalyzeGaitButton_Click(object sender, RoutedEventArgs e)
        {
            AnalysisVideo vid = (AnalysisVideo)GaitListBox.SelectedItem; //get the selected video
            if (vid != null)
            {
                string gaitVideoPath = vid.Path; //extract necessary info from the video
                string gaitVideoName = gaitVideoPath.Substring(gaitVideoPath.LastIndexOf("\\") + 1, gaitVideoPath.LastIndexOf(".") - gaitVideoPath.LastIndexOf("\\"));
                string gaitTempPath = gaitVideoPath.Substring(0, gaitVideoPath.LastIndexOf("\\")) + "\\temp-" + gaitVideoName;
                var files = Directory.EnumerateFiles(gaitTempPath);
                var file = ""; //might crash
                foreach (var currentImg in files)
                {
                    if (currentImg.Contains(".png"))
                    {
                        file = currentImg;
                        break;
                    }
                }
                BarInteraction();

                // Check if Gait data is saved from before
                string stateFolder = gaitVideoPath.Substring(0, gaitVideoPath.LastIndexOf("\\")) + "\\gaitsavedstate";
                if (Directory.Exists(stateFolder) && File.Exists(stateFolder + "\\inputParams.txt"))
                {
                    GaitWindow gaitWindow = new GaitWindow(gaitVideoPath, gaitVideoName, gaitTempPath);
                    if (gaitWindow.ShowDialog() == true)
                    {
                        SyncUI();
                    }
                    EnableInteraction();
                }
                else //input params not saved, ask the user for'em
                {
                    MeasureWindow window = new MeasureWindow(file, stateFolder); //spawn a window through which the user will give us the treadmill speed and a real world reference (for distance measurements)
                    if (window.ShowDialog() == true)
                    {
                        double realWorldMultiplier = window.getSinglePixelSize();
                        float treadmillSpeed = float.Parse(window.TreadmillSpeedTextBox.Text);
                        bool isFreeRun = false;
                        if ((bool)window.AnalysisTypeRadioFreeWalking.IsChecked) isFreeRun = true;

                        GaitWindow gaitWindow = new GaitWindow(realWorldMultiplier, treadmillSpeed, gaitVideoPath, gaitVideoName, gaitTempPath, isFreeRun);
                        try
                        { // In case there's an error during setup and window is already closed. (eg. short/invalid video)
                            if (gaitWindow.ShowDialog() == true)
                            {
                                SyncUI();
                            }
                        } 
                        catch (System.InvalidOperationException)
                        {
                            Console.WriteLine("An error occured while Setting up Gait Window.");
                        }
                        
                        EnableInteraction();
                    }
                    else EnableInteraction();
                }
            }
        }

        private void CombineResultsButton_Click(object sender, RoutedEventArgs e)
        {
            ExportCombinedGait();
        }

        private void DragStarted(object sender, System.Windows.Input.MouseButtonEventArgs e)
        { //started dragging an item in the Gait Listbox
            DragSource = sender as ListBox;
            object data = DragSource.SelectedItem;

            if (data != null)
            {
                DragDrop.DoDragDrop(DragSource, data, DragDropEffects.Move);
            }
        }

        private void DragDropped(object sender, DragEventArgs e)
        { //dropped a dragged item above the Gait "combined export" Listbox
            DragTarget = (ListBox)sender;
            AnalysisVideo draggedData = (AnalysisVideo)DragSource.SelectedItem;
            if (!GaitCombinedVideos.Contains(draggedData) && draggedData.IsGaitAnalyzed)
            {
                GaitCombinedVideos.Add(draggedData);
                GaitCombineListBox.ItemsSource = null;
                GaitCombineListBox.ItemsSource = GaitCombinedVideos;
                DragNDropLabel.Visibility = Visibility.Collapsed;
                if (GaitCombinedVideos.Count > 1) CombineResultsButton.IsEnabled = true;
            }
        }

        private void EmptyButton_Click(object sender, RoutedEventArgs e)
        {
            ClearCombinedGait();
        }








        ////////////////////////////////////////////////////////////////////
        // Combined Gaint Methods
        ////////////////////////////////////////////////////////////////////



        private void ClearCombinedGait()
        { //reset the combined gait listbox and related variables to the initial state
            GaitCombinedVideos = new List<AnalysisVideo>();
            GaitCombineListBox.ItemsSource = null;
            DragNDropLabel.Visibility = Visibility.Visible;
            CombineResultsButton.IsEnabled = false;
        }


        private void ExportCombinedGait()
        { //export static data from all the gait-analyzed videos that were added (dragged) to the combined listbox
            List<List<double>> allFiles = new List<List<double>>();
            for (int i = 0; i < GaitCombinedVideos.Count; i++)
            { //first read all the metrics.txt (= static data) from all the videos
                string stateFolder = GaitCombinedVideos[i].Path.Substring(0, GaitCombinedVideos[i].Path.LastIndexOf("\\")) + "\\gaitsavedstate";
                allFiles.Add(File.ReadAllLines(stateFolder + "\\metrics.txt").ToList().ConvertAll(item => double.Parse(item)));
            }

            List<double> combinedList = new List<double>();
            List<double> semList = new List<double>();

            for (int i = 0; i < allFiles[0].Count; i++)
            { //for each of the metrics
                double sum = 0;
                double sdNumerator = 0;
                double sd = 0;

                for (int j = 0; j < allFiles.Count; j++)
                { //add up values for that metric across all files and divide by the number of videos
                    sum = sum + allFiles[j][i];
                }

                double mean = sum / GaitCombinedVideos.Count;

                for (int j = 0; j < allFiles.Count; j++)
                {
                    sdNumerator = sdNumerator + Math.Pow(allFiles[j][i] - mean, 2);
                }

                sd = Math.Sqrt(sdNumerator / (allFiles.Count - 1));
                semList.Add(sd / Math.Sqrt(allFiles.Count));
                combinedList.Add(mean);
            }

            WriteCombinedGaitToCsv(combinedList, semList);
        }


        private void WriteCombinedGaitToCsv(List<double> list, List<double> semList)
        { //export averages from all the videos into a csv
            var csv = new StringBuilder();
            var newLine = string.Format("{0},{1},{2},{3},{4}", "", "Left Hind", "Right Hind", "Left Fore", "Right Fore");
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Average Stance (ms)", list[0], list[1], list[2], list[3]);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Standard Error of Mean", semList[0], semList[1], semList[2], semList[3]);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Average Swing (ms)", list[4], list[5], list[6], list[7]);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Standard Error of Mean", semList[4], semList[5], semList[6], semList[7]);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Average Stride (ms)", list[8], list[9], list[10], list[11]);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Standard Error of Mean", semList[8], semList[9], semList[10], semList[11]);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Percentage Stance", list[12], list[13], list[14], list[15]);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Standard Error of Mean", semList[12], semList[13], semList[14], semList[15]);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Percentage Swing", list[16], list[17], list[18], list[19]);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Standard Error of Mean", semList[16], semList[17], semList[18], semList[19]);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Swing to Stance", list[20], list[21], list[22], list[23]);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Standard Error of Mean", semList[20], semList[21], semList[22], semList[23]);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Stride Frequency (Hz)", list[24], list[25], list[26], list[27]);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Standard Error of Mean", semList[24], semList[25], semList[26], semList[27]);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Number of Strides", list[28], list[29], list[30], list[31]);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Standard Error of Mean", semList[28], semList[29], semList[30], semList[31]);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Stride Length Variability", list[32], list[33], list[34], list[35]);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Standard Error of Mean", semList[32], semList[33], semList[34], semList[35]);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Gait Symmetry", list[36], list[36], list[36], list[36]); //gait symmetry only takes up one lines since it's a single number
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Standard Error of Mean", semList[36], semList[36], semList[36], semList[36]);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Paw Angle Avg (deg)", list[37], list[38], list[39], list[40]);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Standard Error of Mean", semList[37], semList[38], semList[39], semList[40]);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Stance Width (mm)", list[41], list[41], list[42], list[42]); //stance widths only take up two lines
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Standard Error of Mean", semList[41], semList[41], semList[42], semList[43]);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Stride Length Avg (mm)", list[43], list[44], list[45], list[46]);
            csv.AppendLine(newLine);

            newLine = string.Format("{0},{1},{2},{3},{4}", "Standard Error of Mean", semList[43], semList[44], semList[45], semList[46]);
            csv.AppendLine(newLine);

            File.WriteAllText(WorkingDirectory + "\\combined_export.csv", csv.ToString());
            ClearCombinedGait();
            MessageBox.Show("Your data has been saved. Remember that the next combined export will override the file.", "Data Saved", MessageBoxButton.OK);
            Process.Start("explorer.exe", WorkingDirectory);

        }
    }
}
