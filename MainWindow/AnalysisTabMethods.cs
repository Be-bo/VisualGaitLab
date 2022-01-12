using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using VisualGaitLab.OtherWindows;
using VisualGaitLab.SupportingClasses;

namespace VisualGaitLab {
    public partial class MainWindow : Window {


        // MARK: Listbox Related Methods

        private void AnalyzedListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { //listen to selected item to alter analyze button accordingly
            int selectedCount = AnalyzedListBox.SelectedItems.Count;
            
            if (selectedCount > 0) {
                AnalyzeButton.IsEnabled = true;
                AnalyzeButton.Content = "Analyze (" + selectedCount + ")";
            } 
            else {
                AnalyzeButton.IsEnabled = false;
                AnalyzeButton.Content = "Analyze";
            }
        }

        private void PlayAnalyzedVideoClicked(object sender, RoutedEventArgs e) { //show the video in its folder in Windows Explorer
            var selectedVideo = AnalyzedListBox.SelectedItem as AnalysisVideo;
            if (selectedVideo != null) {
                string folderPath = selectedVideo.Path;
                Process process = Process.Start("explorer.exe", folderPath);
            }
        }

        private void AnalyzeEditClicked(object sender, RoutedEventArgs e) { //start the edit window for that video
            var selectedVideo = (AnalysisVideo)AnalyzedListBox.SelectedItem;
            if (selectedVideo != null) {
                BarInteraction();
                EditVideo window = new EditVideo(selectedVideo.Path, selectedVideo.Name, true, CurrentProject, EnvDirectory, Drive, EnvName);
                if (window.ShowDialog() == true) {
                    SyncUI();
                    EnableInteraction();
                }
                else {
                    EnableInteraction();
                }
            }
        }













        // MARK: Results Viewing Functions

        private void ResultsClicked(object sender, RoutedEventArgs e) //see the analyzed video folder containing the results of its analysis
        {
            var selectedVideo = (AnalysisVideo)AnalyzedListBox.SelectedItem;
            if (selectedVideo != null) {
                string folderPath = selectedVideo.Path.Substring(0, selectedVideo.Path.LastIndexOf("\\"));
                Process process = Process.Start("explorer.exe", folderPath);
            }
            else {
                MessageBox.Show("The video hasn't been analyzed yet.", "Invalid Action");
            }
        }

        private void AnalyzeAddClicked(object sender, RoutedEventArgs e) {
            BarInteraction();
            AddNewVideos("analyzed-videos", true);
        }






        // MARK: Video Deletion Functions

        private void AnalyzeDeleteClicked(object sender, RoutedEventArgs e) //delete the selected video
        {
            BarInteraction();
            AnalysisVideo selectedVideo = (AnalysisVideo)AnalyzedListBox.SelectedItem;

            if (MessageBox.Show("Are you sure you want to delete " + selectedVideo.Name + "?", "Delete Video", 
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) {
                EnableInteraction();
            }
            else {
                int selectedIndex = AnalyzedListBox.SelectedIndex;
                CurrentProject.AnalysisVideos.RemoveAt(selectedIndex);
                AnalyzedListBox.ItemsSource = CurrentProject.AnalysisVideos;
                AnalyzedListBox.UpdateLayout();
                string dir = CurrentProject.ConfigPath.Substring(0, CurrentProject.ConfigPath.LastIndexOf("\\")) + "\\analyzed-videos\\" + selectedVideo.Name;
                foreach (var file in Directory.GetFiles(dir)) {
                    if (!file.Equals(selectedVideo.ThumbnailPath)) File.Delete(file);
                }
                //Directory.Delete(dir);
                SyncUI();
                EnableInteraction();
            }
        }













        // MARK: Actual Analysis Methods

        private void AnalyzeButton_Click(object sender, RoutedEventArgs e) //analyze the currently selected videos
        {
            BarInteraction();
            Dictionary<int, AnalysisVideo> videosToAnalyze = new Dictionary<int, AnalysisVideo>();

            // filter out videos that have already been analyzed
            foreach (var item in AnalyzedListBox.SelectedItems)
            {
                var video = (AnalysisVideo)item;
                int index = AnalyzedListBox.Items.IndexOf(item);

                if (video != null && index >= 0) {
                    if (video.IsAnalyzed)
                    {
                        // Can cancel entire process or continue analyzing other videos
                        var result = MessageBox.Show(
                            "Video" + video.Name + " has already been analyzed. Please delete the old video and add it again, or add its copy under a different name.", 
                            "Re-analysis Not Supported", MessageBoxButton.OKCancel
                            );
                        if (result == MessageBoxResult.Cancel) break;
                    }
                    else videosToAnalyze.Add(index, video);  // Add video to analyze later
                }
            }

            if (videosToAnalyze.Count > 0)
            {
                // Set Label Size and if successful, analyze videos
                if (SetLabelSize(videosToAnalyze)) AnalyzeVideos(videosToAnalyze);
                else EnableInteraction();
            }
            else EnableInteraction();
        }


        /**
         * Set label size for given video
         * Returns true if user didn't cancel operation
         */
        private bool SetLabelSize(Dictionary<int, AnalysisVideo> videos)
        {
            bool useSameLabel = false;
            string dotsize = "5";

            foreach (var item in videos) //Key = index in listbox, Value = video 
            {
                string settingsPath = item.Value.Path.Substring(0, item.Value.Path.LastIndexOf("\\")) + "\\settings.txt";

                // Read in and adjust labelsize
                if (!useSameLabel)
                {
                    // Read in dot size for this video (default = 5)
                    StreamReader sr = new StreamReader(settingsPath);
                    String[] rows = Regex.Split(sr.ReadToEnd(), "\r\n");

                    foreach (string row in rows)
                    {
                        if (row.Contains("dotsize:"))
                        {
                            dotsize = row.Substring(row.IndexOf(":") + 2);
                            dotsize = dotsize.Replace(" ", String.Empty);
                            break;
                        }
                    }
                    sr.Close();

                    // Display a window with a thumbnail where the user can choose the label size they find appropriate
                    if (File.Exists(item.Value.ThumbnailPath))
                    {
                        // MediaToolKit not as reliable when it comes to thumbnails, in case it fails we just analyze with default label size
                        AnalysisSettings settingsDialog = new AnalysisSettings(item.Value.ThumbnailPath, item.Value.Name, dotsize);
                        if (settingsDialog.ShowDialog() == true)
                        {
                            useSameLabel = settingsDialog.getCheckBoxValue();
                            dotsize = settingsDialog.CurrentLabelSize.Text;
                        }
                        else return false; // User cancelled the operation
                    }
                }

                // Update Dotsize in settings.txt for video
                StreamWriter sw = new StreamWriter(settingsPath);
                sw.WriteLine("dotsize: " + dotsize);
                sw.Close();
                EditDotSizeInConfig(AnalyzedListBox.SelectedIndex); //update the dotsize in DLC's config file
            }
            return true;
        }


        // Analyze videos for the corresponding analysis videos using DLC's built in analyze_videos function
        // If not in Debug mode, it redirects the output to the loading window accordingly
        private void AnalyzeVideos(Dictionary<int, AnalysisVideo> videos)
        {
            Console.WriteLine("Analyzing video...");
            // Prepare script
            string filePath = EnvDirectory + "\\vdlc_analyze_video.py";
            FileSystemUtils.MurderPython();
            FileSystemUtils.RenewScript(filePath, AllScripts.AnalyzeVideo); 
            FileSystemUtils.ReplaceStringInFile(filePath, "config_path_identifier", CurrentProject.ConfigPath);

            // Prepare cmd process
            Process p = new Process(); 
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "cmd.exe";
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true; //!ReadShowDebugConsole(); 
            info.UseShellExecute = false;
            info.Verb = "runas";
            info.CreateNoWindow = !ReadShowDebugConsole(); //if show debug console = true, then create no window has to be false

            Dispatcher.Invoke(() => //once done close the loading window
            {
                LoadingWindow = new LoadingWindow();
                LoadingWindow.Title = "Analyzing";
                LoadingWindow.Show();
                LoadingWindow.Closed += LoadingClosed;
            });

            bool errorDuringAnalysis = false;
            string errorMessage = "No Error";
            string progressValue = "0";
            string progressMax = "0";
            int videoProgValue = 0;

            
            //NONDEBUG -----------------------------------------------------------------------------------------------
            if (info.CreateNoWindow)
            { //redirect output if debug mode disabled
                p.OutputDataReceived += new DataReceivedEventHandler((sender, e) => //feed cmd output to the loading window so the user knows the progress of the analysis
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        string line = e.Data;
                        Console.WriteLine(line);

                        if (line.Contains("OOM"))
                        {
                            errorMessage = "Analysis failed due to insufficient GPU memory. Try importing the video again and reducing its resolution, and/or cropping it.";
                            errorDuringAnalysis = true;
                            FileSystemUtils.MurderPython();
                        }

                        if (line.Contains("progress_maximum"))
                        {
                            progressMax = line.Substring(line.IndexOf(":") + 1, line.IndexOf("#") - line.IndexOf(":") - 1);
                            Dispatcher.Invoke(() => {
                                LoadingWindow.ProgressBar.Maximum = int.Parse(progressMax);
                            });
                        }

                        if (line.Contains("progress_value"))
                        {
                            progressValue = line.Substring(line.IndexOf(":") + 1, line.IndexOf("#") - line.IndexOf(":") - 1);
                            Dispatcher.Invoke(() => {
                                LoadingWindow.ProgressLabel2.Content = progressValue + " / " + progressMax + " frames";
                                LoadingWindow.ProgressBar.Value = int.Parse(progressValue);
                            });
                        }

                        if (line.Contains("Starting to analyze"))
                        {
                            videoProgValue++;
                            string videoName = line.Split('\\').Last();
                            Dispatcher.Invoke(() => {
                                LoadingWindow.ProgressLabel.Content = "Analyzing " + videoName + " (" + videoProgValue + "/" + videos.Count + ")";
                                LoadingWindow.ProgressBar.Value = int.Parse(progressValue);
                            });
                        }
                    }
                });
            }
            //NONDEBUG -----------------------------------------------------------------------------------------------


            p.EnableRaisingEvents = true;
            p.Exited += (sender1, e1) => {
                if (errorDuringAnalysis)
                {
                    this.Dispatcher.Invoke(() => {
                        LoadingWindow.Close();
                        EnableInteraction();
                    });
                    UpdateVGLConfig(); //update VGLS's config file
                    MessageBox.Show(errorMessage, "Error Occurred", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                // If done analyzing, start creating labelled videos
                this.Dispatcher.Invoke(() => {
                    LoadingWindow.ProgressLabel.Content = "Creating labeled videos (will take a while)...";
                    LoadingWindow.ProgressLabel2.Content = "";
                });
                CreateLabeledVideos(videos);
            };

            p.StartInfo = info;
            p.Start();

            using (StreamWriter sw = p.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    sw.WriteLine(Drive);
                    sw.WriteLine("cd " + EnvDirectory);
                    sw.WriteLine(FileSystemUtils.CONDA_ACTIVATE_PATH);
                    sw.WriteLine("conda activate " + EnvName);
                    sw.Write("ipython vdlc_analyze_video.py");

                    // Arguments
                    foreach (var video in videos.Values) sw.Write(" \"" + video.Path + "\"");
                    sw.WriteLine("");

                    if (info.CreateNoWindow == false)
                    { //for debug purposes
                        sw.WriteLine("\n\n\nECHO WHEN YOU'RE DONE, CLOSE THIS WINDOW");
                        p.WaitForExit();
                        sw.WriteLine("Done, exiting.");
                    }
                }
            }
            if (info.CreateNoWindow) p.BeginOutputReadLine();   //only redirect output if debug enabled
        }
















        // MARK: Creating Labeled Video Methods

        // Create a labeled video for the corresponding analysis video using DLC's built in create_labeled_video function
        private void CreateLabeledVideos(Dictionary<int, AnalysisVideo> videos) 
        {
            // Preapare Script
            string filePath = EnvDirectory + "\\vdlc_create_labeled_video.py";
            FileSystemUtils.MurderPython();
            FileSystemUtils.RenewScript(filePath, AllScripts.CreateLabeledVideo);
            FileSystemUtils.ReplaceStringInFile(filePath, "config_path_identifier", CurrentProject.ConfigPath);

            // Prepare process
            Process p = new Process();
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "cmd.exe";
            info.RedirectStandardInput = true;
            info.UseShellExecute = false;
            info.Verb = "runas";
            info.CreateNoWindow = !ReadShowDebugConsole(); //if show debug console = true, then create no window has to be false

            p.EnableRaisingEvents = true;
            p.Exited += (sender1, e1) => {
                Dispatcher.Invoke(() => {
                    LoadingWindow.Close();
                });
                SyncUI();
                EnableInteraction();
            };

            p.StartInfo = info;
            p.Start();

            using (StreamWriter sw = p.StandardInput) {
                if (sw.BaseStream.CanWrite) {
                    sw.WriteLine(Drive);
                    sw.WriteLine("cd " + EnvDirectory);
                    sw.WriteLine(FileSystemUtils.CONDA_ACTIVATE_PATH);
                    sw.WriteLine("conda activate " + EnvName);
                    sw.Write("ipython vdlc_create_labeled_video.py");

                    // Arguments
                    foreach (var video in videos.Values) sw.Write(" \"" + video.Path + "\"");
                    sw.WriteLine("");

                    if (!info.CreateNoWindow) { //for debug purposes
                        sw.WriteLine("ECHO WHEN YOU'RE DONE, CLOSE THIS WINDOW");
                        p.WaitForExit();
                        sw.WriteLine("Done, exiting.");
                    }
                }
            }
        }
    }
}
