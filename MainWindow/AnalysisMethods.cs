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

        private void AnalyzedVideoClicked(object sender, RoutedEventArgs e) { //show the video in its folder in Windows Explorer
            var selectedVideo = AnalyzedListBox.SelectedItem as AnalysisVideo;
            if (selectedVideo != null && selectedVideo.IsAnalyzed) {
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















        // MARK: Mirroring Operation Methods

        private void MirrorAnalyzeVideo_Click(object sender, RoutedEventArgs e) { //mirror the selected analysis video using cmd and FFmpeg
            var selectedVideo = (AnalysisVideo)AnalyzedListBox.SelectedItem;
            if (selectedVideo != null && !selectedVideo.IsAnalyzed) {
                BarInteraction();

                string mirroredVidNameWithExtension = selectedVideo.Name + "_mirrored" + selectedVideo.Path.Substring(selectedVideo.Path.LastIndexOf("."));
                string mirroredVidFullPath = selectedVideo.Path.Substring(0, selectedVideo.Path.LastIndexOf("\\") + 1) + mirroredVidNameWithExtension;

                Process p = new Process();
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = "cmd.exe";
                info.RedirectStandardInput = true;
                info.UseShellExecute = false;
                info.Verb = "runas";
                info.CreateNoWindow = true;

                p.EnableRaisingEvents = true;
                p.Exited += (sender1, e1) => { //once mirroring done, create a new thumbnail and sync ui
                    //CreateThumbnailForVideo(selectedVideo.Path, selectedVideo.Path.Substring(0, selectedVideo.Path.LastIndexOf("\\")) + "\\thumbnail.png");
                    SetUpAnalysisVideo(mirroredVidFullPath);
                    SyncUI();
                    EnableInteraction();
                };

                p.StartInfo = info;
                p.Start();

                using (StreamWriter sw = p.StandardInput) {
                    if (sw.BaseStream.CanWrite) { //mirror the video using ffmpeg
                        sw.WriteLine(Drive);
                        sw.WriteLine("cd " + selectedVideo.Path.Substring(0, selectedVideo.Path.LastIndexOf("\\")));
                        sw.WriteLine("ffmpeg -y -i \"" + selectedVideo.Path.Substring(selectedVideo.Path.LastIndexOf("\\") + 1) + "\" -qscale 0 -map_metadata 0 -crf 0 -vf \"hflip\" \"" + mirroredVidNameWithExtension + "\"");
                    }
                }
            }
            else {
                MessageBox.Show("The video cannot be mirrored because it's already been analyzed.", "Invalid Action");
            }
        }












        // MARK: Results Viewing Functions

        private void ResultsClicked(object sender, RoutedEventArgs e) //see the analyzed video folder containing the results of its analysis
        {
            var selectedVideo = (AnalysisVideo)AnalyzedListBox.SelectedItem;
            if (selectedVideo != null && selectedVideo.IsAnalyzed) {
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
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select a video";
            if (openFileDialog.ShowDialog() == true) {
                SetUpAnalysisVideo(openFileDialog.FileName);
            }
            else {
                EnableInteraction();
            }
        }

        private void SetUpAnalysisVideo(String fullPath) { //add the new analysis video to the project
            if (fullPath.Contains(".mp4") || fullPath.Contains(".avi")) {
                AnalysisVideo newVideo = new AnalysisVideo();
                newVideo.Name = fullPath.Substring(fullPath.LastIndexOf("\\") + 1, fullPath.LastIndexOf(".") - fullPath.LastIndexOf("\\") - 1);
                if (newVideo.Name.Length < 36) {
                    string hypotheticalPath = CurrentProject.ConfigPath.Substring(0, CurrentProject.ConfigPath.LastIndexOf("\\")) + "\\analyzed-videos\\" + newVideo.Name;
                    if ((!File.Exists(hypotheticalPath + "\\" + newVideo.Name + ".mp4")) && !File.Exists(hypotheticalPath + "\\" + newVideo.Name + ".avi")) {
                        newVideo.Path = CurrentProject.ConfigPath.Substring(0, CurrentProject.ConfigPath.LastIndexOf("\\")) + "\\analyzed-videos\\" + newVideo.Name + "\\" + fullPath.Substring(fullPath.LastIndexOf("\\") + 1);
                        string targetPath = /*Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)*/ newVideo.Path;
                        System.IO.Directory.CreateDirectory(targetPath.Substring(0, targetPath.LastIndexOf("\\")));
                        string fileName = targetPath;

#pragma warning disable CA2008 // Do not create tasks without passing a TaskScheduler MY NOTE: VS Studio was shouting at me, this stopped the shouting...
                        _ = Task.Run(() => File.Copy(fullPath, fileName, true)).ContinueWith((task) => { //copy the video to the analyzed-videos folder and only once it's done do the rest of the setup
                            string settingsPath = targetPath.Substring(0, targetPath.LastIndexOf("\\")) + "\\settings.txt";
                            StreamWriter sw1 = new StreamWriter(settingsPath);
                            sw1.WriteLine("dotsize: " + "5");
                            sw1.Close();

                            newVideo.ThumbnailPath = "thumbnail.png";
                            newVideo.IsAnalyzed = false;
                            newVideo.AnalyzedImageName = "cross.png";
                            if (CurrentProject.AnalysisVideos == null) CurrentProject.AnalysisVideos = new List<AnalysisVideo>();

                            newVideo.ThumbnailPath = newVideo.Path.Substring(0, newVideo.Path.LastIndexOf("\\")) + "\\thumbnail.png";
                            CreateThumbnailForVideo(newVideo.Path, newVideo.ThumbnailPath);
                            CurrentProject.AnalysisVideos.Add(newVideo);
                            SyncUI();
                            EnableInteraction();
                        });
#pragma warning restore CA2008 // Do not create tasks without passing a TaskScheduler
                    }
                    else {
                        EnableInteraction();
                        MessageBox.Show("Your video cannot be added because it has already been added to this project.", "Video Cannot Be Added", MessageBoxButton.OK);
                    }
                }
                else {
                    EnableInteraction();
                    MessageBox.Show("Video name length cannot exceed 35 characters.", "Video Cannot Be Added", MessageBoxButton.OK);
                }


            }
            else {
                MessageBox.Show("Video cannot be added. Only .mp4 and .avi file types are supported. Note: If you have .MP4 videos, convert them to H.264 codec (and potentially rename to .mp4).", "Unsupported Action");
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
            else {
                EnableInteraction();
                MessageBox.Show("Analysis settings are applied to individual analysis videos. Select a video and then click the button again.", "No video selected", MessageBoxButton.OK);
            }
        }

        private void AnalyzeVideo(int pos) //analyze the video at the position in the list box (passed in as param)
        {
            AnalysisVideo video = CurrentProject.AnalysisVideos[pos];
            string filePath = EnvDirectory + "\\vdlc_analyze_video.py";
            MurderPython();
            RenewScript(filePath, AllScripts.AnalyzeVideo); //prepare script
            ReplaceStringInFile(filePath, "config_path_identifier", CurrentProject.ConfigPath);
            ReplaceStringInFile(filePath, "video_path_identifier", video.Path);

            Process p = new Process(); //prepare cmd process
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "cmd.exe";
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;
            info.Verb = "runas";
            info.CreateNoWindow = true;

            string progressMax = "0";
            string progressValue = "0";
            this.Dispatcher.Invoke(() => //once done close the loading window
            {
                LoadingWindow = new LoadingWindow();
                LoadingWindow.Title = "Analyzing";
                LoadingWindow.Show();
                LoadingWindow.Closed += LoadingClosed;
            });

            p.OutputDataReceived += new DataReceivedEventHandler((sender, e) => //feed cmd output to the loading window so the user knows the progress of the analysis
            {
                if (!String.IsNullOrEmpty(e.Data)) {
                    string line = e.Data;

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


            p.EnableRaisingEvents = true;
            p.Exited += (sender1, e1) => {
                this.Dispatcher.Invoke(() => {
                    LoadingWindow.ProgressLabel.Content = "Finalizing (might take a while)...";
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
                }
            }

            p.BeginOutputReadLine();
        }
















        // MARK: Creating Labeled Video Methods

        private void CreateLabeledVideo(AnalysisVideo video) //create a labeled video for the corresponding analysis video using DLC's built in create_labeled_video function
        {
            string filePath = EnvDirectory + "\\vdlc_create_labeled_video.py";
            MurderPython();
            RenewScript(filePath, AllScripts.CreateLabeledVideo);
            ReplaceStringInFile(filePath, "config_path_identifier", CurrentProject.ConfigPath);
            ReplaceStringInFile(filePath, "video_path_identifier", video.Path);
            Process p = new Process();
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "cmd.exe";
            info.RedirectStandardInput = true;
            info.UseShellExecute = false;
            info.Verb = "runas";
            info.CreateNoWindow = true;

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
                }
            }
        }
    }
}
