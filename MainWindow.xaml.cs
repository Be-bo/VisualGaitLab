using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using VisualGaitLab.GaitAnalysis;
using VisualGaitLab.OtherWindows;
using VisualGaitLab.SupportingClasses;

namespace VisualGaitLab
{
    public partial class MainWindow : Window {





        // MARK: Variables and Constants

        Project CurrentProject;
        string ProgramFolder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\VisualGaitLab";
        string WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\VisualGaitLab\\Projects";
        string EnvDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\VisualGaitLab\\Miniconda3\\envs\\dlc-windowsGPU";
        string EnvName = "dlc-windowsGPU";
        string Drive = "c:";
        PythonScripts AllScripts = new PythonScripts();
        LoadingWindow LoadingWindow;
        List<AnalysisVideo> GaitVideos;
        List<AnalysisVideo> GaitCombinedVideos;
        ListBox DragSource = null;
        ListBox DragTarget = null;
        Stopwatch globalStopWatch = new Stopwatch();





        // MARK: Startup Functions

        public MainWindow() {

            if (IsAdministrator() == false) {
                // Restart program and run as admin (because we're digging around the C: drive)
                var exeName = Process.GetCurrentProcess().MainModule.FileName;
                ProcessStartInfo startInfo = new ProcessStartInfo(exeName);
                startInfo.Verb = "runas";
                System.Diagnostics.Process.Start(startInfo);
                Application.Current.Shutdown();
                return;
            }

            /*
            if (!Directory.Exists(EnvDirectory)) { //condition code that was stripped down, if the Miniconda env does not exist, we close the program
                EnvDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\VisualGaitLab\\Miniconda3\\envs\\dlc-windowsGPU";
                EnvName = "dlc-windowsGPU";
                if (!Directory.Exists(EnvDirectory)) this.Close();
            }*/

            InitializeComponent();
            CheckInstallation();
            ShowDisclaimer();
        }

        private static bool IsAdministrator() { //check if the Windows user is running the program as administrator
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void ShowDisclaimer() { //show some info about the current state of the program
            MessageBox.Show(
                "We STRONGLY recommend that you first quickly run through the entire process before committing to a project (to make sure VGL runs well on your machine)." +
                "\n\n" +
                "version 2.0.0 - OSF frozen", "Important Info", MessageBoxButton.OK, MessageBoxImage.Warning);
        }













        // MARK: UI Functionality Methods

        private void SetHint(string text) { //no longer using hints anywhere, disregard
            //HintText.Text = "Hint: " + text;
        }

        private void SyncUI() { //sync the UI with the current state of the project
            this.Dispatcher.Invoke(() => { //run on the main thread in case the call came from a different thread
                LoadProject(CurrentProject.ConfigPath.Substring(0, CurrentProject.ConfigPath.LastIndexOf("\\"))); //load the project
                if (CurrentProject != null) {
                    TrainButton.Visibility = Visibility.Visible;
                    TrainingAddDock.Visibility = Visibility.Visible;
                    bool readyToTrain = false;
                    for (int i = 0; i < CurrentProject.TrainingVideos.Count; i++) { //check if there is at least one video that has its frames extracted and labeled
                        if (CurrentProject.TrainingVideos[i].FramesExtracted && CurrentProject.TrainingVideos[i].FramesLabeled) {
                            readyToTrain = true;
                            break;
                        }
                    }

                    if (readyToTrain) { //if all vids are extracted and labeled allow training
                        TrainButton.IsEnabled = true;
                        if (!CurrentProject.IsTrained) {
                            TrainStatsButton.Visibility = Visibility.Collapsed;
                            TrainStatsButton.IsEnabled = false;
                            SetHint("All of your training videos have been labeled. You should review your training settings (the cog button). Now you can either train the network or add more videos for better results (we recommend 200 frames across all videos).");
                        }
                        else {
                            TrainStatsButton.Visibility = Visibility.Visible;
                            TrainStatsButton.IsEnabled = true;
                        }
                    }
                    else { //else don't allow training or stats viewing
                        TrainButton.IsEnabled = false;
                        TrainStatsButton.IsEnabled = false;
                        TrainStatsButton.Visibility = Visibility.Collapsed;
                        SetHint("Some of your training videos still need to be labeled. For videos that have two crosses you need to first extract the frames and then label them. For the ones that have only one cross, those only need to be labeled.");
                    }

                    TrainingListBox.ItemsSource = null; //reset training box and update frames label
                    if (CurrentProject.TrainingVideos != null && CurrentProject.TrainingVideos.Count > 0) {
                        TrainingListBox.ItemsSource = CurrentProject.TrainingVideos;
                        int frames = 0;
                        foreach (TrainingVideo vid in CurrentProject.TrainingVideos) {
                            frames = frames + vid.Frames;
                        }
                        FramesTextBlock.Text = frames.ToString() + " frames";
                    }
                    else {
                        TrainingListBox.ItemsSource = null;
                        FramesTextBlock.Text = "0 frames";
                    }

                    if (CurrentProject.AnalysisVideos != null && CurrentProject.AnalysisVideos.Count > 0) AnalyzedListBox.ItemsSource = CurrentProject.AnalysisVideos; //if there are analysis videos add them to the analysis listbox
                    else AnalyzedListBox.ItemsSource = null;

                    if (CurrentProject.IsGaitOnly) GaitTab.Visibility = Visibility.Visible; else GaitTab.Visibility = Visibility.Hidden; //if it's a gait focused project show the Gait tab
                    PrepareGaitTab();
                }
            });
        }

        private void BarInteraction() { //show the progress ring and disable the primary window so the user can't click anything, also make the window opaque
            this.Dispatcher.Invoke(() => {
                PrimaryGrid.IsEnabled = false;
                PrimaryGrid.Opacity = 0.3;
                ProgressRing.IsActive = true;
            });
        }

        private void EnableInteraction() { //cancel all the effects of the BarInteraction method
            this.Dispatcher.Invoke(() => {
                PrimaryGrid.IsEnabled = true;
                PrimaryGrid.Opacity = 1;
                ProgressRing.IsActive = false;
            });
        }

        private void EnableAnalysisPart() { //enable analysis tab
            AnalyzeTab.IsEnabled = true;
        }

        private void BarAnalysisPart() { //disable analysis tab
            AnalyzeTab.IsEnabled = false;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e) { //not in use currently

        }











        // MARK: Gait Methods

        private void PrepareGaitTab() { //set up the last (gait) tab = grab the analyzed videos from the analysis tab and make them show up here + set up logic for drag and drop to the other list box (combined gait export feature)
            GaitVideos = new List<AnalysisVideo>();
            GaitCombineListBox.AllowDrop = true;
            GaitCombinedVideos = new List<AnalysisVideo>();
            CombineResultsButton.IsEnabled = false;
            foreach (AnalysisVideo vid in CurrentProject.AnalysisVideos) { //the combo box will contain all analyzed videos from the "Analyze" tab
                if (vid.IsAnalyzed) {
                    string gaitStateFolder = vid.Path.Substring(0, vid.Path.LastIndexOf("\\")) + "\\gaitsavedstate";
                    if (Directory.Exists(gaitStateFolder) && File.Exists(gaitStateFolder + "\\metrics.txt")) {
                        vid.GaitAnalyzedImageName = "Images/check.png";
                        vid.IsGaitAnalyzed = true;
                    }
                    else {
                        vid.GaitAnalyzedImageName = "Images/cross.png";
                        vid.IsGaitAnalyzed = false;
                    }
                    GaitVideos.Add(vid);
                }
            }

            if (GaitVideos.Count > 0) { //if there are analyzed videos allow access to the "Gait" tab
                GaitListBox.ItemsSource = null;
                GaitListBox.ItemsSource = GaitVideos;
                GaitTab.IsEnabled = true;
            }
            else GaitTab.IsEnabled = false; //if there are none disable it
        }

        private void GaitListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { //change button description depending on whether gait has been analyzed on a particular video
            AnalysisVideo selectedVid = (AnalysisVideo)GaitListBox.SelectedItem;
            if (selectedVid != null) {
                AnalyzeGaitButton.IsEnabled = true;
                if (selectedVid.IsGaitAnalyzed) {
                    AnalyzeGaitButton.Content = "View/Edit";
                }
                else {
                    AnalyzeGaitButton.Content = "Analyze Gait";
                }
            }
            else AnalyzeGaitButton.IsEnabled = false;
        }

        private void AnalyzeGaitButton_Click(object sender, RoutedEventArgs e) {
            AnalysisVideo vid = (AnalysisVideo)GaitListBox.SelectedItem; //get the selected video
            if (vid != null) {
                string gaitVideoPath = vid.Path; //extract necessary info from the video
                string gaitVideoName = vid.Path.Substring(vid.Path.LastIndexOf("\\") + 1, vid.Path.LastIndexOf(".") - vid.Path.LastIndexOf("\\"));
                string gaitTempPath = vid.Path.Substring(0, vid.Path.LastIndexOf("\\")) + "\\temp-" + gaitVideoName;
                var files = Directory.EnumerateFiles(gaitTempPath);
                var file = ""; //might crash
                foreach(var currentImg in files) {
                    if (currentImg.Contains(".png")) {
                        file = currentImg;
                        break;
                    }
                }
                BarInteraction();

                // Check if Gait data is saved from before
                string stateFolder = gaitVideoPath.Substring(0, gaitVideoPath.LastIndexOf("\\")) + "\\gaitsavedstate"; 
                if (Directory.Exists(stateFolder) && File.Exists(stateFolder + "\\inputParams.txt")) {
                    GaitWindow gaitWindow = new GaitWindow(gaitVideoPath, gaitVideoName, gaitTempPath);
                    if (gaitWindow.ShowDialog() == true) {
                        SyncUI();
                    }
                    EnableInteraction();
                }
                else //input params not saved, ask the user for'em
                {
                    MeasureWindow window = new MeasureWindow(file); //spawn a window through which the user will give us the treadmill speed and a real world reference (for distance measurements)
                    if (window.ShowDialog() == true) {
                        double realWorldMultiplier = window.getSinglePixelSize();
                        float treadmillSpeed = float.Parse(window.TreadmillSpeedTextBox.Text);
                        bool isFreeRun = false;
                        if ((bool)window.AnalysisTypeRadioFreeWalking.IsChecked) isFreeRun = true;

                        GaitWindow gaitWindow = new GaitWindow(realWorldMultiplier, treadmillSpeed, gaitVideoPath, gaitVideoName, gaitTempPath, isFreeRun);
                        if (gaitWindow.ShowDialog() == true) {
                            SyncUI();
                        }
                        EnableInteraction();
                    }
                    else EnableInteraction();
                }
            }
        }

        private void CombineResultsButton_Click(object sender, RoutedEventArgs e) {
            ExportCombinedGait();
        }

        private void DragStarted(object sender, System.Windows.Input.MouseButtonEventArgs e) { //started dragging an item in the Gait Listbox
            DragSource = (ListBox)sender;
            object data = DragSource.SelectedItem;

            if (data != null) {
                DragDrop.DoDragDrop(DragSource, data, DragDropEffects.Move);
            }
        }

        private void DragDropped(object sender, DragEventArgs e) { //dropped a dragged item above the Gait "combined export" Listbox
            DragTarget = (ListBox)sender;
            AnalysisVideo draggedData = (AnalysisVideo)DragSource.SelectedItem;
            if (!GaitCombinedVideos.Contains(draggedData) && draggedData.IsGaitAnalyzed) {
                GaitCombinedVideos.Add(draggedData);
                GaitCombineListBox.ItemsSource = null;
                GaitCombineListBox.ItemsSource = GaitCombinedVideos;
                DragNDropLabel.Visibility = Visibility.Collapsed;
                if (GaitCombinedVideos.Count > 1) CombineResultsButton.IsEnabled = true;
            }
        }

        private void EmptyButton_Click(object sender, RoutedEventArgs e) {
            ClearCombinedGait();
        }







        private void DebugConsoleClicked(object sender, RoutedEventArgs e) {
            if(MessageBox.Show("Debug Console displays the command line each time VGL is interfacing with DeepLabCut. Enable this if you're experiencing problems with creating a project, labelling, training, or analysis. If you encounter a bug, enable this, and email rfiker@ucalgary.ca with a screenshot of the console. Do you want to show the console?"
                ,"Enable Debug Console?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes){
                UpdateSettings(true);
            }
            else {
                UpdateSettings(false);
            }
        }
    }
}
