using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
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
        string ScriptsFolder = Directory.GetCurrentDirectory() + "\\CustomScripts";
        string ScriptsListFile = "scriptsList.txt";
        string EnvName = "dlc-windowsGPU";
        string Drive = "c:";
        PythonScripts AllScripts = new PythonScripts();
        LoadingWindow LoadingWindow;
        List<AnalysisVideo> GaitVideos;
        List<AnalysisVideo> GaitCombinedVideos;
        ListBox DragSource = null;
        ListBox DragTarget = null;
        Stopwatch globalStopWatch = new Stopwatch();
        List<CustomScript> PAScripts;
        bool PAScriptsPrepared = false;





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
                "Please check the OSF link: https://osf.io/2ydzn/ for documentation. \n\n" +
                "version 2.4.0", "Important Info", MessageBoxButton.OK, MessageBoxImage.Warning);
        }













        // MARK: UI Functionality Methods

        private void SetHint(string text) { //no longer using hints anywhere, disregard
            //HintText.Text = "Hint: " + text;
        }

        private void SyncUI() { //sync the UI with the current state of the project
            Dispatcher.Invoke(() => { //run on the main thread in case the call came from a different thread
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

                    //if it's a gait focused project show the Gait tab
                    if (CurrentProject.IsGaitOnly) EnableGait();
                    else BarGait();
                }
            });
        }

        private void BarInteraction() { //show the progress ring and disable the primary window so the user can't click anything, also make the window opaque
            Dispatcher.Invoke(() => {
                PrimaryGrid.IsEnabled = false;
                PrimaryGrid.Opacity = 0.3;
                ProgressRing.IsActive = true;
            });
        }

        private void EnableInteraction() { //cancel all the effects of the BarInteraction method
            Dispatcher.Invoke(() => {
                PrimaryGrid.IsEnabled = true;
                PrimaryGrid.Opacity = 1;
                ProgressRing.IsActive = false;
            });
        }

        private void EnableAnalysisPart() { //enable analysis tabs
            AnalyzeTab.IsEnabled = true;
            PostAnalysisTab.IsEnabled = true;
        }

        private void BarAnalysisPart() { //disable analysis tabs
            AnalyzeTab.IsEnabled = false;
            PostAnalysisTab.IsEnabled = false;
        }

        private void EnableGait()
        {
            GaitTab.Visibility = Visibility.Visible;
            PrepareGaitTab();
        }

        private void BarGait()
        {
            GaitTab.Visibility = Visibility.Collapsed;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e) {

            if (PostAnalysisTab.IsSelected && !PAScriptsPrepared)
            {
                PreparePostAnalysisTab();
                PAScriptsPrepared = true;
            }
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
