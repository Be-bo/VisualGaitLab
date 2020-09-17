using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VisualGaitLab.OtherWindows;
using VisualGaitLab.SupportingClasses;

namespace VisualGaitLab {
    public partial class MainWindow : Window {


        // MARK: Listbox Related Methods

        private void AnalyzedListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { //listen to selected item to know which object to target when right mouse button clicked
            AnalysisVideo selectedVideo = (AnalysisVideo)AnalyzedListBox.SelectedItem;
            if (selectedVideo != null) {
                if (selectedVideo.IsAnalyzed) {
                    AnalyzeButton.IsEnabled = false;
                }
                else {
                    AnalyzeButton.IsEnabled = true;
                }
                AnalyzeButton.IsEnabled = true;
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
            AddVideoForAnalysis();
        }













        // MARK: Add Video Functions

        private void AddVideoForAnalysis() //add a new video to analyze
        {
            OpenFileDialog openFileDialog = new OpenFileDialog(); //open a file dialog to let the user choose which video to add
            openFileDialog.Title = "Select a video";
            if (openFileDialog.ShowDialog() == true) {
                string fullPath = openFileDialog.FileName;
                if (fullPath.ToLower().EndsWith(".avi") || fullPath.ToLower().EndsWith(".mp4") || fullPath.ToLower().EndsWith(".wmv") || fullPath.ToLower().EndsWith(".mov")) {
                    if (!FileSystemUtils.NameAlreadyInDir(FileSystemUtils.ExtendPath(FileSystemUtils.GetParentFolder(CurrentProject.ConfigPath), "analyzed-videos", FileSystemUtils.GetFileName(fullPath)), FileSystemUtils.GetFileNameWithExtension(fullPath))) {

                        if (FileSystemUtils.FileNameOk(fullPath)) {
                            ImportWindow window = new ImportWindow(fullPath, CurrentProject.ConfigPath, true, EnvDirectory, EnvName, Drive, ProgramFolder);
                            if (window.ShowDialog() == true) {
                                SyncUI();
                                EnableInteraction();
                            }
                            else {
                                EnableInteraction();
                            }
                        }
                        else {
                            MessageBox.Show("File names must be 25 characters or less, with only alphanumeric characters, dashes, and underscores allowed.", "Invalid Name", MessageBoxButton.OK, MessageBoxImage.Error);
                            EnableInteraction();
                        }
                    }
                    else {
                        MessageBox.Show("Video with a similar or an identical name has already been added. Please rename your new video.", "Name Already Taken", MessageBoxButton.OK, MessageBoxImage.Error);
                        EnableInteraction();
                    }
                }
                else {
                    MessageBox.Show("Video cannot be added. Your video format is not supported.", "Unsupported Action", MessageBoxButton.OK, MessageBoxImage.Error);
                    EnableInteraction();
                }
            }
            else {
                EnableInteraction();
            }
        }














        // MARK: Video Deletion Functions

        private void AnalyzeDeleteClicked(object sender, RoutedEventArgs e) //delete the selected video
        {
            BarInteraction();
            if (MessageBox.Show("Are you sure?", "Delete Video", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) {
                EnableInteraction();
            }
            else {
                AnalysisVideo selectedVideo = (AnalysisVideo)AnalyzedListBox.SelectedItem;
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

        private void AnalyzeButton_Click(object sender, RoutedEventArgs e) //analyze the currently selected video
        {
            BarInteraction();
            var selectedVideo = (AnalysisVideo)AnalyzedListBox.SelectedItem;
            if (selectedVideo != null) {
                if (selectedVideo.IsAnalyzed) {
                    MessageBox.Show("Please delete the old video and add it again, or add its copy under a different name.", "Re-analysis Not Supported", MessageBoxButton.OK);
                    EnableInteraction();
                }
                else {
                    StreamReader sr = new StreamReader(selectedVideo.Path.Substring(0, selectedVideo.Path.LastIndexOf("\\")) + "\\settings.txt"); //set this video's setting for labeled video dot size to default = 5
                    String[] rows = Regex.Split(sr.ReadToEnd(), "\r\n");
                    string dotsize = "5";
                    foreach (string row in rows) {
                        if (row.Contains("dotsize:")) {
                            dotsize = row.Substring(row.IndexOf(":") + 2);
                            dotsize = dotsize.Replace(" ", String.Empty);
                            break;
                        }
                    }
                    sr.Close();

                    if (File.Exists(selectedVideo.ThumbnailPath)) { //MediaToolKit not as reliable when it comes to thumbnails, in case it fails we just analyze with default label size
                        AnalysisSettings settingsDialog = new AnalysisSettings(selectedVideo.ThumbnailPath, selectedVideo.Name, dotsize); //display a window with a thumbnail where the user can choose the label size they find appropriate
                        if (settingsDialog.ShowDialog() == true) {
                            string settingsPath = selectedVideo.Path.Substring(0, selectedVideo.Path.LastIndexOf("\\")) + "\\settings.txt";
                            StreamWriter sw = new StreamWriter(settingsPath);
                            sw.WriteLine("dotsize: " + settingsDialog.CurrentLabelSize.Text);
                            sw.Close();
                            EditDotSizeInConfig(AnalyzedListBox.SelectedIndex); //update the dotsize in DLC's config file
                            AnalyzeVideo(AnalyzedListBox.SelectedIndex);
                        }
                        else EnableInteraction();
                    }
                    else { //thumbnail doesn't exist (its creation failed)
                        AnalyzeVideo(AnalyzedListBox.SelectedIndex);
                    } 
                }
            }
        }

        private void AnalyzeVideo(int pos) //analyze the video at the position in the list box (passed in as param)
        {
            bool errorDuringAnalysis = false;
            string errorMessage = "No Error";
            AnalysisVideo video = CurrentProject.AnalysisVideos[pos];
            string filePath = EnvDirectory + "\\vdlc_analyze_video.py";
            FileSystemUtils.MurderPython();
            FileSystemUtils.RenewScript(filePath, AllScripts.AnalyzeVideo); //prepare script
            FileSystemUtils.ReplaceStringInFile(filePath, "config_path_identifier", CurrentProject.ConfigPath);
            FileSystemUtils.ReplaceStringInFile(filePath, "video_path_identifier", video.Path);

            Process p = new Process(); //prepare cmd process
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "cmd.exe";
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = !ReadShowDebugConsole();
            info.UseShellExecute = false;
            info.Verb = "runas";
            info.CreateNoWindow = !ReadShowDebugConsole(); //if show debug console = true, then create no window has to be false

            string progressMax = "0";
            string progressValue = "0";
            this.Dispatcher.Invoke(() => //once done close the loading window
            {
                LoadingWindow = new LoadingWindow();
                LoadingWindow.Title = "Analyzing";
                LoadingWindow.Show();
                LoadingWindow.Closed += LoadingClosed;
            });


            //NONDEBUG -----------------------------------------------------------------------------------------------
            if (info.CreateNoWindow) { //redirect output if debug mode disabled
                p.OutputDataReceived += new DataReceivedEventHandler((sender, e) => //feed cmd output to the loading window so the user knows the progress of the analysis
                {
                    if (!String.IsNullOrEmpty(e.Data)) {
                        string line = e.Data;
                        Console.WriteLine(line);

                        if (line.Contains("OOM")) {
                            errorMessage = "Analysis failed due to insufficient GPU memory. Try importing the video again and reducing its resolution, and/or cropping it.";
                            errorDuringAnalysis = true;
                            FileSystemUtils.MurderPython();
                        }

                        if (line.Contains("progress_maximum")) {
                            progressMax = line.Substring(line.IndexOf(":") + 1, line.IndexOf("#") - line.IndexOf(":") - 1);
                            this.Dispatcher.Invoke(() => {
                                LoadingWindow.ProgressBar.Maximum = int.Parse(progressMax);
                            });
                        }

                        if (line.Contains("progress_value")) {
                            progressValue = line.Substring(line.IndexOf(":") + 1, line.IndexOf("#") - line.IndexOf(":") - 1);
                            this.Dispatcher.Invoke(() => {
                                string progressInfo = progressValue + " / " + progressMax + " frames";
                                LoadingWindow.ProgressLabel.Content = progressInfo;
                                LoadingWindow.ProgressBar.Value = int.Parse(progressValue);
                            });
                        }
                    }
                });
            }
            //NONDEBUG -----------------------------------------------------------------------------------------------


            p.EnableRaisingEvents = true;
            p.Exited += (sender1, e1) => {
                if (errorDuringAnalysis) {
                    this.Dispatcher.Invoke(() => {
                        LoadingWindow.Close();
                        EnableInteraction();
                    });
                    UpdateVGLConfig(); //update VGLS's config file
                    MessageBox.Show(errorMessage, "Error Occurred", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                this.Dispatcher.Invoke(() => {
                    LoadingWindow.ProgressLabel.Content = "Creating labeled video (will take a while)...";
                });
                CreateLabeledVideo(video);
            };

            p.StartInfo = info;
            p.Start();

            using (StreamWriter sw = p.StandardInput) {
                if (sw.BaseStream.CanWrite) {
                    sw.WriteLine(Drive);
                    sw.WriteLine("cd " + EnvDirectory);
                    sw.WriteLine("\"C:\\Program Files (x86)\\VisualGaitLab\\Miniconda3\\Scripts\\activate.bat\"");
                    sw.WriteLine("conda activate " + EnvName);
                    sw.WriteLine("ipython vdlc_analyze_video.py");

                    if (info.CreateNoWindow == false) { //for debug purposes
                        sw.WriteLine("");
                        sw.WriteLine("");
                        sw.WriteLine("");
                        sw.WriteLine("ECHO WHEN YOU'RE DONE, CLOSE THIS WINDOW");
                        p.WaitForExit();
                        sw.WriteLine("Done, exiting.");
                    }
                }
            }

            if(info.CreateNoWindow) p.BeginOutputReadLine();//only redirect output if debug enabled
        }
















        // MARK: Creating Labeled Video Methods

        private void CreateLabeledVideo(AnalysisVideo video) //create a labeled video for the corresponding analysis video using DLC's built in create_labeled_video function
        {
            string filePath = EnvDirectory + "\\vdlc_create_labeled_video.py";
            FileSystemUtils.MurderPython();
            FileSystemUtils.RenewScript(filePath, AllScripts.CreateLabeledVideo);
            FileSystemUtils.ReplaceStringInFile(filePath, "config_path_identifier", CurrentProject.ConfigPath);
            FileSystemUtils.ReplaceStringInFile(filePath, "video_path_identifier", video.Path);
            Process p = new Process();
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "cmd.exe";
            info.RedirectStandardInput = true;
            info.UseShellExecute = false;
            info.Verb = "runas";
            info.CreateNoWindow = !ReadShowDebugConsole(); //if show debug console = true, then create no window has to be false

            p.EnableRaisingEvents = true;
            p.Exited += (sender1, e1) => {
                this.Dispatcher.Invoke(() => {
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
                    sw.WriteLine("\"C:\\Program Files (x86)\\VisualGaitLab\\Miniconda3\\Scripts\\activate.bat\"");
                    sw.WriteLine("conda activate " + EnvName);
                    sw.WriteLine("ipython vdlc_create_labeled_video.py");

                    if (info.CreateNoWindow == false) { //for debug purposes
                        sw.WriteLine("ECHO WHEN YOU'RE DONE, CLOSE THIS WINDOW");
                        p.WaitForExit();
                        sw.WriteLine("Done, exiting.");
                    }
                }
            }
        }
    }
}
