using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VisualGaitLab.SupportingClasses;
using VisualGaitLab.GaitAnalysis;
using System.IO;
using System.Windows.Controls;
using VisualGaitLab.OtherWindows;

namespace VisualGaitLab
{
    public partial class MainWindow : Window
    {
        private void PrepareGaitTab()
        { //set up the last (gait) tab = grab the analyzed videos from the analysis tab and make them show up here + set up logic for drag and drop to the other list box (combined gait export feature)
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
                        if (gaitWindow.ShowDialog() == true)
                        {
                            SyncUI();
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
    }
}
