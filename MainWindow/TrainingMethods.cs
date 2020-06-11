using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VisualGaitLab.OtherWindows;
using VisualGaitLab.SupportingClasses;

namespace VisualGaitLab {
    public partial class MainWindow : Window {






        // MARK: Context Menu Methods

        private void TrainingVideoContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e) {

        }

        private void TrainingVideoContextMenu_Opened(object sender, RoutedEventArgs e) { //dynamically change the context menu options for each video depending on the training stage
            TrainingVideo selectedVideo = (TrainingVideo)TrainingListBox.SelectedItem;
            ContextMenu contextMenu = (ContextMenu)sender;
            if (selectedVideo != null) {
                MenuItem labelItem = (MenuItem)contextMenu.Items[1];
                MenuItem framesItem = (MenuItem)contextMenu.Items[2];
                MenuItem mirrorItem = (MenuItem)contextMenu.Items[3];
                MenuItem editItem = (MenuItem)contextMenu.Items[4];

                if (!selectedVideo.FramesExtracted) {
                    framesItem.Header = "Extract & Label Frames";
                    labelItem.Visibility = Visibility.Collapsed;
                }
                else {
                    framesItem.Header = "Extract More Frames";
                    labelItem.Visibility = Visibility.Visible;
                }

                if (!selectedVideo.FramesLabeled) {
                    labelItem.Header = "Label Frames";
                }
                else {
                    labelItem.Header = "Edit Labels";
                }

                if (!selectedVideo.FramesExtracted && !selectedVideo.FramesLabeled) {
                    mirrorItem.Visibility = Visibility.Visible;
                    editItem.Visibility = Visibility.Visible;
                }
                else {
                    mirrorItem.Visibility = Visibility.Collapsed;
                    editItem.Visibility = Visibility.Collapsed;
                }
            }
        }

















        // MARK: ListBox Related Methods

        private void TrainingListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        }

        private void TrainingVideoClicked(object sender, RoutedEventArgs e) { //open training video with Windows Explorer
            var selectedVideo = (TrainingVideo)TrainingListBox.SelectedItem;
            if (selectedVideo != null) {
                string folderPath = selectedVideo.Path;
                Process process = Process.Start("explorer.exe", folderPath);
            }
        }

        private void ExtractFramesClicked(object sender, RoutedEventArgs e) { //old function
            var selectedVid = (TrainingVideo)TrainingListBox.SelectedItem;
            if (selectedVid != null) {
                BarInteraction();
                FramesToExtractDialog dialog = new FramesToExtractDialog();
                if (dialog.ShowDialog() == true) {
                    CurrentProject.FramesToExtract = dialog.FramesToExtractTextBox.Text;
                    UpdateFramesToExtract();
                    WaitForFile(CurrentProject.ConfigPath);
                    ExtractFrames(selectedVid);
                }
                else {
                    EnableInteraction();
                }
            }
        }












        // MARK: Add Video Functions

        private void AddClicked(object sender, RoutedEventArgs e) //add a new training video
        {
            BarInteraction();
            AddTrainingVideo();
        }

        private void AddTrainingVideo() //logic for adding a training video
        {
            OpenFileDialog openFileDialog = new OpenFileDialog(); //open a file dialog to let the user choose which video to add
            openFileDialog.Title = "Select a video";
            if (openFileDialog.ShowDialog() == true) {
                string fullPath = openFileDialog.FileName;
                if (fullPath.Contains(".mp4") || fullPath.Contains(".avi")) {
                    SetUpVideo(fullPath);
                }
                else {
                    MessageBox.Show("Video cannot be added. Only .mp4 and .avi file types are supported. Note: If you have .MP4 videos, convert them to H.264 codec (and potentially rename to .mp4).", "Unsupported Action");
                    EnableInteraction();
                }
            }
            else {
                EnableInteraction();
            }
        }

        private void SetUpVideo(String fullPath) { //prepare a video by adding its info to the current project, create a thumbnail for it and call DLC's script to add it to the underlying project structure
            TrainingVideo newVideo = new TrainingVideo();
            newVideo.Name = fullPath.Substring(fullPath.LastIndexOf("\\") + 1, fullPath.LastIndexOf(".") - fullPath.LastIndexOf("\\") - 1);
            string hypotheticalPath = CurrentProject.ConfigPath.Substring(0, CurrentProject.ConfigPath.LastIndexOf("\\")) + "\\labeled-data\\" + newVideo.Name;
            if ((!Directory.Exists(hypotheticalPath)) || (Directory.Exists(hypotheticalPath) && Directory.EnumerateFileSystemEntries(hypotheticalPath).Count() < 2)) {
                if (Directory.Exists(hypotheticalPath) && Directory.EnumerateFileSystemEntries(hypotheticalPath).Count() == 1) File.Delete(Directory.EnumerateFiles(hypotheticalPath).First());
                newVideo.Path = CurrentProject.ConfigPath.Substring(0, CurrentProject.ConfigPath.LastIndexOf("\\")) + "\\videos\\" + fullPath.Substring(fullPath.LastIndexOf("\\") + 1);
                newVideo.ThumbnailPath = "thumbnail.png";
                newVideo.FramesExtracted = false;
                newVideo.ExtractedImageName = "cross.png";
                newVideo.FramesLabeled = false;
                newVideo.LabeledImageName = "cross.png";

                string filePath = EnvDirectory + "\\vdlc_add_video.py";
                string copyVideosBool = "True";
                MurderPython();
                RenewScript(filePath, AllScripts.AddVideo); //create and run DeepLabCut's add_video function
                ReplaceStringInFile(filePath, "copy_videos_identifier", copyVideosBool); //make sure the function parameters are correct
                ReplaceStringInFile(filePath, "config_path_identifier", CurrentProject.ConfigPath);
                ReplaceStringInFile(filePath, "video_path_identifier", fullPath);

                Process p = new Process(); //prepare command line process
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = "cmd.exe";
                info.RedirectStandardInput = true;
                info.UseShellExecute = false;
                info.Verb = "runas";
                info.CreateNoWindow = true;

                p.EnableRaisingEvents = true;
                p.Exited += (sender1, e1) =>  //once cmd is finished running the script
                {
                    this.Dispatcher.Invoke(() => {
                        newVideo.ThumbnailPath = MakeTrainingVideoThumbnail(newVideo.Path); //create an actual thumbnail for the video
                        CurrentProject.TrainingVideos.Add(newVideo); //add it to the project
                        SyncUI();
                        EnableInteraction();
                    });
                };

                p.StartInfo = info;
                p.Start();

                using (StreamWriter sw = p.StandardInput) { //run the script using a hidden command line instance
                    if (sw.BaseStream.CanWrite) {
                        sw.WriteLine(Drive);
                        sw.WriteLine("cd " + EnvDirectory);
                        sw.WriteLine("\"C:\\Program Files (x86)\\VisualGaitLab\\Miniconda3\\Scripts\\activate.bat\"");
                        sw.WriteLine("conda activate " + EnvName);
                        sw.WriteLine("ipython vdlc_add_video.py");
                    }
                }
            }
            else {
                EnableInteraction();
                MessageBox.Show("Your video cannot be added because it has already been added to this project.", "Video Cannot Be Added", MessageBoxButton.OK);
            }

        }

        private void DeleteClicked(object sender, RoutedEventArgs e) //delete training video clicked
        {
            MurderPython();
            BarInteraction();
            if (CurrentProject.TrainingVideos.Count < 2) {
                MessageBox.Show("There needs to be at least one video.", "Invalid Action", MessageBoxButton.OK, MessageBoxImage.Warning); //there's a bug if the user adds videos and then decides to delete all of them, this prevents it from happening
                EnableInteraction();
            }
            else {
                if (MessageBox.Show("Are you sure?", "Delete Video", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) {
                    EnableInteraction();
                }
                else {
                    TrainingVideo selectedVideo = (TrainingVideo)TrainingListBox.SelectedItem; //delete the training video
                    int selectedIndex = TrainingListBox.SelectedIndex;
                    CurrentProject.TrainingVideos.RemoveAt(selectedIndex);
                    TrainingListBox.ItemsSource = CurrentProject.TrainingVideos;
                    TrainingListBox.UpdateLayout();
                    File.Delete(selectedVideo.Path);
                    //File.Delete(selectedVideo.ThumbnailPath); thumbnail is being used by the program so there's a lock on it and we can't delete it - //TODO
                    string dir = CurrentProject.ConfigPath.Substring(0, CurrentProject.ConfigPath.LastIndexOf("\\")) + "\\labeled-data\\" + selectedVideo.Name; //delete the associated labeled data if it exists
                    if (Directory.Exists(dir)) {
                        foreach (var file in Directory.GetFiles(dir)) {
                            File.Delete(file);
                        }
                    }

                    DeleteVidFromConfig(CurrentProject.ConfigPath, selectedVideo.Path);
                    if (CurrentProject.TrainedWith.Contains(selectedVideo.Path)) {
                        CurrentProject.TrainedWith.Remove(selectedVideo.Path);
                        UpdateVdlcConfig();
                    }

                    if (Directory.Exists(dir)) Directory.Delete(dir);
                    SyncUI();
                    EnableInteraction();
                }
            }
        }

        private String MakeTrainingVideoThumbnail(String videoPath) { //create thumbnail for the training video inside the training video folder under the name: "<vid_name>.png"
            String tgtPath = videoPath.Substring(0, videoPath.LastIndexOf(".")) + ".png";
            CreateThumbnailForVideo(videoPath, tgtPath);
            return tgtPath;
        }

















        // MARK: Extraction Methods

        private void EditFramesClicked(object sender, RoutedEventArgs e) {
            TrainingVideo vid = (TrainingVideo)TrainingListBox.SelectedItem;
            if (vid != null) {//check if the video has extracted frames, if not inform the user
                BarInteraction();
                ExtractFrames(vid);
            }
            else {
                MessageBox.Show("This video's project data seems to be corrupted. Try adding it again under a different name.", "Video Data Corrupted", MessageBoxButton.OK);
            }

        }

        private void ExtractFrames(TrainingVideo video) //extract frames from a particular training video
        {
            FramesToExtractDialog dialog = new FramesToExtractDialog(); //first ask the user how many frames to extract
            if (dialog.ShowDialog() == true) {
                CurrentProject.FramesToExtract = dialog.FramesToExtractTextBox.Text;
                UpdateFramesToExtract();
                WaitForFile(CurrentProject.ConfigPath);
                string filePath = EnvDirectory + "\\vdlc_extract_frames.py";
                MurderPython();
                RenewScript(filePath, AllScripts.ExtractFrames); //run DLC's extract_frames function using a script
                ReplaceStringInFile(filePath, "config_path_identifier", CurrentProject.ConfigPath); //set function parameters
                ReplaceStringInFile(filePath, "video_path_identifier", video.Path);

                Process p = new Process(); //prepare a cmd process to run the script
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = "cmd.exe";
                info.RedirectStandardInput = true;
                info.UseShellExecute = false;
                info.Verb = "runas";
                info.CreateNoWindow = true;

                p.EnableRaisingEvents = true;
                p.Exited += (sender1, e1) => //once done, continue to labeling the video automatically
                {
                    LabelFrames(video);
                };

                p.StartInfo = info;
                p.Start();

                using (StreamWriter sw = p.StandardInput) { //run the script using the command line
                    if (sw.BaseStream.CanWrite) {
                        sw.WriteLine(Drive);
                        sw.WriteLine("cd " + EnvDirectory);
                        sw.WriteLine("\"C:\\Program Files (x86)\\VisualGaitLab\\Miniconda3\\Scripts\\activate.bat\"");
                        sw.WriteLine("conda activate " + EnvName);
                        sw.WriteLine("ipython vdlc_extract_frames.py");
                    }
                }
            }
            else {
                EnableInteraction();
            }
        }
















        // MARK: Edit and Mirror Methods

        private void MirrorTrainingVideo_Click(object sender, RoutedEventArgs e) {
            var selectedVideo = (TrainingVideo)TrainingListBox.SelectedItem;
            if (selectedVideo != null && !selectedVideo.FramesExtracted && !selectedVideo.FramesLabeled) {
                BarInteraction();

                //mirrored vid name is the original name + "_mirrored" just before the video extension
                String outMirroredVidName = selectedVideo.Path.Substring(selectedVideo.Path.LastIndexOf("\\") + 1, selectedVideo.Path.LastIndexOf(".") - (selectedVideo.Path.LastIndexOf("\\") + 1)) + "_mirrored";
                outMirroredVidName = outMirroredVidName + selectedVideo.Path.Substring(selectedVideo.Path.LastIndexOf("."));

                Process p = new Process();
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = "cmd.exe";
                info.RedirectStandardInput = true;
                info.UseShellExecute = false;
                info.Verb = "runas";
                info.CreateNoWindow = true;

                p.EnableRaisingEvents = true;
                p.Exited += (sender1, e1) => { //once mirroring done, create a new thumbnail and sync ui
                    MakeTrainingVideoThumbnail(selectedVideo.Path.Substring(0, selectedVideo.Path.LastIndexOf("\\") + 1) + outMirroredVidName);
                    SetUpVideo(outMirroredVidName);
                    SyncUI();
                    EnableInteraction();
                };

                p.StartInfo = info;
                p.Start();

                using (StreamWriter sw = p.StandardInput) {
                    if (sw.BaseStream.CanWrite) { //mirror the video using ffmpeg
                        sw.WriteLine(Drive);
                        sw.WriteLine("cd " + selectedVideo.Path.Substring(0, selectedVideo.Path.LastIndexOf("\\")));
                        sw.WriteLine("ffmpeg -y -i \"" + selectedVideo.Path.Substring(selectedVideo.Path.LastIndexOf("\\") + 1) + "\" -qscale 0 -map_metadata 0 -crf 0 -vf \"hflip\" \"" + outMirroredVidName + "\"");
                        //sw.WriteLine("wait to finish line"); //the next line sometimes gets cut off, this is just to make sure it doesn't happen to the actual command below
                        //sw.WriteLine("ffmpeg -y -i out" + selectedVideo.Path.Substring(selectedVideo.Path.LastIndexOf(".")) + " -c copy -copyts -muxdelay 0 -max_delay 0 -map_metadata 0 \"" + selectedVideo.Path.Substring(selectedVideo.Path.LastIndexOf("\\") + 1) + "\"");
                    }
                }
            }
            else {
                MessageBox.Show("The video cannot be mirrored because its frames have already been extracted and/or labeled.", "Invalid Action");
            }
        }

        private void TrainingVideoEdit_Click(object sender, RoutedEventArgs e) {
            TrainingVideo selectedVideo = (TrainingVideo)TrainingListBox.SelectedItem;
            if (selectedVideo != null) {
                BarInteraction();
                EditVideo window = new EditVideo(selectedVideo.Path, selectedVideo.Name, false, CurrentProject, EnvDirectory, Drive, EnvName);
                if (window.ShowDialog() == true) {
                    SyncUI();
                    EnableInteraction();
                }
                else {
                    EnableInteraction();
                }
            }
        }


























        // MARK: Labeling Methods

        private void EditLabelsClicked(object sender, RoutedEventArgs e) {
            TrainingVideo vid = (TrainingVideo)TrainingListBox.SelectedItem;
            if (vid != null) { //check if the video has extracted frames, if not inform the user
                if (vid.FramesExtracted) {
                    BarInteraction();
                    LabelFrames(vid);
                }
                else {
                    MessageBox.Show("You haven't extracted any frames for this video. Choose the \"Edit Frames\" option instead.", "Frames Not Extracted", MessageBoxButton.OK);
                }
            }
            else {
                MessageBox.Show("This video's project data is corrupted. Try adding the video under a different name.", "Video Data Corrupted", MessageBoxButton.OK);
            }
        }

        private void LabelFrames(TrainingVideo video) //start DLC's labeling toolbox for a particular video
        {
            string filePath = EnvDirectory + "\\vdlc_label_frames.py";
            MurderPython();
            RenewScript(filePath, AllScripts.LabelFrames); //prepare to call DeepLabCut's labeling function
            ReplaceStringInFile(filePath, "config_path_identifier", CurrentProject.ConfigPath); //set params
            ReplaceStringInFile(filePath, "video_path_identifier", CurrentProject.ConfigPath.Substring(0, CurrentProject.ConfigPath.LastIndexOf("\\")) + "\\labeled-data\\" + video.Name);

            Process p = new Process(); //prepare the cmd backgroung process
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "cmd.exe";
            info.RedirectStandardInput = true;
            info.UseShellExecute = false;
            info.Verb = "runas";
            info.CreateNoWindow = true;

            p.EnableRaisingEvents = true;
            p.Exited += (sender1, e1) => {
                SyncUI();
                EnableInteraction();
            };

            p.StartInfo = info;
            p.Start();

            using (StreamWriter sw = p.StandardInput) //run the labeling script
            {
                if (sw.BaseStream.CanWrite) {
                    sw.WriteLine(Drive);
                    sw.WriteLine("cd " + EnvDirectory);
                    sw.WriteLine("\"C:\\Program Files (x86)\\VisualGaitLab\\Miniconda3\\Scripts\\activate.bat\"");
                    sw.WriteLine("conda activate " + EnvName);
                    sw.WriteLine("ipython vdlc_label_frames.py");
                }
            }
        }


















        // MARK: Training Methods

        private void TrainButton_Click(object sender, RoutedEventArgs e) //start training
        {
            BarInteraction();
            if (CurrentProject.IsTrained) {
                MessageBox.Show("The network has already been trained. If you choose to train again the previous network will be overwritten.", "Overwrite?");
            }
            TrainingSettings settingsDialog = new TrainingSettings();
            settingsDialog.saveItersTextBox.Text = CurrentProject.SaveIters;
            settingsDialog.endItersTextBox.Text = CurrentProject.EndIters;
            if (settingsDialog.ShowDialog() == true) {
                EditTrainingSettings(settingsDialog);
                CreateTrainingDataset();
            }
            else {
                EnableInteraction();
            }
        }

        private void CreateTrainingDataset() //before training create a training dataset
        {
            string filePath = EnvDirectory + "\\vdlc_create_dataset.py";
            MurderPython();
            RenewScript(filePath, AllScripts.CreateDataset);
            ReplaceStringInFile(filePath, "config_path_identifier", CurrentProject.ConfigPath);
            Process p = new Process();
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "cmd.exe";
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;
            info.Verb = "runas";
            info.CreateNoWindow = true;

            p.EnableRaisingEvents = true;
            p.Exited += (sender1, e1) => {
                string posePath = GetPoseCfgPath();
                if (posePath != null) UpdatePoseCfg(posePath);
                TrainNetwork();
            };

            p.StartInfo = info;
            p.Start();

            using (StreamWriter sw = p.StandardInput) {
                if (sw.BaseStream.CanWrite) {
                    sw.WriteLine(Drive);
                    sw.WriteLine("cd " + EnvDirectory);
                    sw.WriteLine("\"C:\\Program Files (x86)\\VisualGaitLab\\Miniconda3\\Scripts\\activate.bat\"");
                    sw.WriteLine("conda activate " + EnvName);
                    sw.WriteLine("ipython vdlc_create_dataset.py");
                }
            }
        }



        private void TrainNetwork() //when training dataset created we start the training by, again, calling DeepLabCut's function
        {
            string filePath = EnvDirectory + "\\vdlc_train_network.py";
            MurderPython();
            RenewScript(filePath, AllScripts.TrainNetwork);
            ReplaceStringInFile(filePath, "config_path_identifier", CurrentProject.ConfigPath);
            Process p = new Process();
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "cmd.exe";
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;
            info.Verb = "runas";
            info.CreateNoWindow = true;

            this.Dispatcher.Invoke(() => {
                LoadingWindow = new LoadingWindow(); //show a loading window during training that takes cmd's output (DeepLabCut's train_network function generated output) so the user knows how far along they are
                LoadingWindow.Title = "Training";
                LoadingWindow.Show();
                LoadingWindow.Closed += LoadingClosed;
                LoadingWindow.ProgressBar.Maximum = int.Parse(CurrentProject.EndIters);
            });

            int currentIter = 0;
            StringBuilder output = new StringBuilder();
            p.OutputDataReceived += new DataReceivedEventHandler((sender, e) => {
                if (!String.IsNullOrEmpty(e.Data)) {
                    string line = e.Data;
                    if (line.Contains("iteration:") && line.Contains("loss:")) //extracting the current iterations information from cmd's output
                    {
                        currentIter = currentIter + int.Parse(CurrentProject.DisplayIters);
                        this.Dispatcher.Invoke(() => {
                            string currentIters = line.Substring(line.IndexOf("n:") + 3, line.IndexOf("lo") - line.IndexOf("n:") - 4);
                            string progressInfo = currentIters + " / " + CurrentProject.EndIters + " iterations";
                            LoadingWindow.ProgressLabel.Content = progressInfo;
                            LoadingWindow.ProgressBar.Value = currentIter;
                        });
                    }
                }
            });

            p.EnableRaisingEvents = true;
            p.Exited += (sender1, e1) => {
                globalStopWatch.Stop();
                TimeSpan ts = globalStopWatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                this.Dispatcher.Invoke(() => {
                    LoadingWindow.Close();
                });
                CurrentProject.TrainedWith = new List<string>();
                foreach (TrainingVideo current in TrainingListBox.ItemsSource) CurrentProject.TrainedWith.Add(current.Path);
                UpdateVdlcConfig(); //update VDLC's config file
                DeleteEvalFiles(); //overwrite eval files
                EvalNetwork(elapsedTime);
            };

            p.StartInfo = info;
            p.Start();
            globalStopWatch = new Stopwatch();
            globalStopWatch.Start(); //time the training period

            using (
            StreamWriter sw = p.StandardInput) {
                if (sw.BaseStream.CanWrite) {
                    sw.WriteLine(Drive);
                    sw.WriteLine("cd " + EnvDirectory);
                    sw.WriteLine("\"C:\\Program Files (x86)\\VisualGaitLab\\Miniconda3\\Scripts\\activate.bat\"");
                    sw.WriteLine("conda activate " + EnvName);
                    sw.WriteLine("ipython vdlc_train_network.py");
                }
            }

            p.BeginOutputReadLine();

        }









        //MARK: Evaluation Methods
        private void EvalNetwork(string elapsedTime) { //evaluate the newly trained network using DeepLabCut's evaluate_network function
            BarInteraction();
            string filePath = EnvDirectory + "\\vdlc_eval_network.py";
            MurderPython();
            RenewScript(filePath, AllScripts.EvalNetwork);
            ReplaceStringInFile(filePath, "config_path_identifier", CurrentProject.ConfigPath);
            Process p = new Process();
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "cmd.exe";
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;
            info.Verb = "runas";
            info.CreateNoWindow = true;

            this.Dispatcher.Invoke(() => {
                LoadingWindow = new LoadingWindow();
                LoadingWindow.Title = "Evaluating";
                LoadingWindow.Show();
                LoadingWindow.Closed += LoadingClosed;
                LoadingWindow.ProgressBar.Maximum = 100;
                LoadingWindow.ProgressBar.Value = 100;
                LoadingWindow.ProgressLabel.Content = "Just a sec...";
            });

            p.EnableRaisingEvents = true;
            p.Exited += (sender1, e1) => {
                CurrentProject.TrainTime = elapsedTime;
                GetEvalResultsSaveTime(ref CurrentProject);
                this.Dispatcher.Invoke(() => {
                    LoadingWindow.Close();
                    EvalWindow evalWindow = new EvalWindow(CurrentProject.TrainTime, CurrentProject.TrainError, CurrentProject.TestError, CurrentProject.PCutoff); //once done evaluating, show a window with evaluation stats
                    evalWindow.Show();
                });
                SyncUI();
                EnableInteraction();
            };

            p.StartInfo = info;
            p.Start();
            globalStopWatch = new Stopwatch();
            globalStopWatch.Start();

            using (
            StreamWriter sw = p.StandardInput) { //run the evaluation script
                if (sw.BaseStream.CanWrite) {
                    sw.WriteLine(Drive);
                    sw.WriteLine("cd " + EnvDirectory);
                    sw.WriteLine("\"C:\\Program Files (x86)\\VisualGaitLab\\Miniconda3\\Scripts\\activate.bat\"");
                    sw.WriteLine("conda activate " + EnvName);
                    sw.WriteLine("ipython vdlc_eval_network.py");
                }
            }

            p.BeginOutputReadLine();
        }

        private void TrainStatsButton_Click(object sender, RoutedEventArgs e) { //show the evaluation window upon button click
            Process process = Process.Start("explorer.exe", CurrentProject.ConfigPath.Substring(0, CurrentProject.ConfigPath.LastIndexOf("\\")) + "\\evaluation-results");
            GetAllEvalResults(ref CurrentProject);
            EvalWindow evalWindow = new EvalWindow(CurrentProject.TrainTime, CurrentProject.TrainError, CurrentProject.TestError, CurrentProject.PCutoff);
            evalWindow.Show();
        }

        private void DeleteEvalFiles() { //delete evaluation files from the previous training session (DLC by default creates new files for each evaluation and because we identify the stats to display by ".csv" extension we can only have one to make sure the correct data is displayed)
            string evalDir = CurrentProject.ConfigPath.Substring(0, CurrentProject.ConfigPath.LastIndexOf("\\")) + "\\evaluation-results"; //delete the eval data files
            if (Directory.Exists(evalDir) && Directory.EnumerateDirectories(evalDir).Any()) {
                var firstNestedFolder = Directory.EnumerateDirectories(evalDir).First();
                if (Directory.Exists(firstNestedFolder)) {
                    var secondNestedFolder = Directory.EnumerateDirectories(firstNestedFolder).First();
                    if (Directory.Exists(secondNestedFolder)) {
                        var allFiles = Directory.EnumerateFiles(secondNestedFolder);
                        foreach (var file in allFiles) {
                            FileInfo fileInfo = new FileInfo(file);
                            File.SetAttributes(fileInfo.FullName, FileAttributes.Normal);
                            fileInfo.Delete();
                        }
                    }
                }
            }
        }










        // MARK: Training Settings Methods

        private void EditTrainingSettings(TrainingSettings dialog) {
            CurrentProject.SaveIters = dialog.saveItersTextBox.Text;
            CurrentProject.EndIters = dialog.endItersTextBox.Text;
            UpdateVdlcConfig();
            UpdateFramesToExtract();
        }
    }
}
