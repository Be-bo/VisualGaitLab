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
                    FileSystemUtils.WaitForFile(CurrentProject.ConfigPath);
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
                if (fullPath.ToLower().EndsWith(".avi") || fullPath.ToLower().EndsWith(".mp4") || fullPath.ToLower().EndsWith(".wmv") || fullPath.ToLower().EndsWith(".mov")) {
                    if (!FileSystemUtils.NameAlreadyInDir(FileSystemUtils.ExtendPath(FileSystemUtils.GetParentFolder(CurrentProject.ConfigPath), "videos"), FileSystemUtils.GetFileNameWithExtension(fullPath))) {

                        if (FileSystemUtils.FileNameOk(fullPath)) {
                            ImportWindow window = new ImportWindow(fullPath, CurrentProject.ConfigPath, false, EnvDirectory, EnvName, Drive, ProgramFolder);
                            if (window.ShowDialog() == true) {
                                SyncUI();
                                EnableInteraction();
                            }else{
                            EnableInteraction();
                            }
                        }
                        else {
                            MessageBox.Show("File names must be 25 characters or less, with only alphanumeric characters, dashes, and underscores allowed.", "Invalid Name", MessageBoxButton.OK, MessageBoxImage.Error);
                            EnableInteraction();
                        }
                    } else {
                        MessageBox.Show("Video with a similar or an identical name has already been added. Please rename your new video.", "Name Already Taken", MessageBoxButton.OK, MessageBoxImage.Error);
                        EnableInteraction();
                    }
                } else {
                    MessageBox.Show("Video cannot be added. Your video format is not supported.", "Unsupported Action", MessageBoxButton.OK, MessageBoxImage.Error);
                    EnableInteraction();
                }
            }
            else {
                EnableInteraction();
            }
        }

        private void DeleteClicked(object sender, RoutedEventArgs e) //delete training video clicked
        {
            FileSystemUtils.MurderPython();
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
                        UpdateVGLConfig();
                    }

                    if (Directory.Exists(dir)) Directory.Delete(dir);
                    SyncUI();
                    EnableInteraction();
                }
            }
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
                FileSystemUtils.WaitForFile(CurrentProject.ConfigPath);
                string filePath = EnvDirectory + "\\vdlc_extract_frames.py";
                FileSystemUtils.MurderPython();
                FileSystemUtils.RenewScript(filePath, AllScripts.ExtractFrames); //run DLC's extract_frames function using a script
                FileSystemUtils.ReplaceStringInFile(filePath, "config_path_identifier", CurrentProject.ConfigPath); //set function parameters
                FileSystemUtils.ReplaceStringInFile(filePath, "video_path_identifier", video.Path);

                Process p = new Process(); //prepare a cmd process to run the script
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = "cmd.exe";
                info.RedirectStandardInput = true;
                info.UseShellExecute = false;
                info.Verb = "runas";
                info.CreateNoWindow = !ReadShowDebugConsole(); //if show debug console = true, then create no window has to be false

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

                        if (info.CreateNoWindow == false) { //for debug purposes
                            sw.WriteLine("ECHO WHEN YOU'RE DONE, CLOSE THIS WINDOW");
                            p.WaitForExit();
                            sw.WriteLine("Done, exiting.");
                        }
                    }
                }
            }
            else {
                EnableInteraction();
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
            FileSystemUtils.MurderPython();
            FileSystemUtils.RenewScript(filePath, AllScripts.LabelFrames); //prepare to call DeepLabCut's labeling function
            FileSystemUtils.ReplaceStringInFile(filePath, "config_path_identifier", CurrentProject.ConfigPath); //set params
            FileSystemUtils.ReplaceStringInFile(filePath, "video_path_identifier", CurrentProject.ConfigPath.Substring(0, CurrentProject.ConfigPath.LastIndexOf("\\")) + "\\labeled-data\\" + video.Name);

            Process p = new Process(); //prepare the cmd backgroung process
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "cmd.exe";
            info.RedirectStandardInput = true;
            info.UseShellExecute = false;
            info.Verb = "runas";
            info.CreateNoWindow = !ReadShowDebugConsole(); //if show debug console = true, then create no window has to be false

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

                    if (info.CreateNoWindow == false) { //for debug purposes
                        sw.WriteLine("ECHO WHEN YOU'RE DONE, CLOSE THIS WINDOW");
                        p.WaitForExit();
                        sw.WriteLine("Done, exiting.");
                    }
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
            settingsDialog.endItersTextBox.Text = CurrentProject.EndIters;
            settingsDialog.GlobalScaleSlider.Value = CurrentProject.GlobalScale;
            settingsDialog.GlobalScaleNumberText.Text = CurrentProject.GlobalScale.ToString();
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
            Console.WriteLine("CREATE TRAINING DATASET");
            string filePath = EnvDirectory + "\\vdlc_create_dataset.py";
            FileSystemUtils.MurderPython();
            FileSystemUtils.RenewScript(filePath, AllScripts.CreateDataset);
            FileSystemUtils.ReplaceStringInFile(filePath, "config_path_identifier", CurrentProject.ConfigPath);
            Process p = new Process();
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "cmd.exe";
            info.RedirectStandardInput = true;
            info.UseShellExecute = false;
            info.Verb = "runas";
            info.CreateNoWindow = !ReadShowDebugConsole(); //if show debug console = true, then create no window has to be false

            p.EnableRaisingEvents = true;
            p.Exited += (sender1, e1) => {
                Console.WriteLine("CREATING DATASET DONE");
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

                    if (info.CreateNoWindow == false) { //for debug purposes
                        sw.WriteLine("ECHO WHEN YOU'RE DONE, CLOSE THIS WINDOW");
                        p.WaitForExit();
                        sw.WriteLine("Done, exiting.");
                    }
                }
            }
        }



        private void TrainNetwork() //when training dataset created we start the training by, again, calling DeepLabCut's function
        {
            Console.WriteLine("TRAIN NETWROK");
            bool errorDuringTraining = false;
            string errorMessage = "No Error";
            string filePath = EnvDirectory + "\\vdlc_train_network.py";
            FileSystemUtils.MurderPython();
            FileSystemUtils.RenewScript(filePath, AllScripts.TrainNetwork);
            FileSystemUtils.ReplaceStringInFile(filePath, "config_path_identifier", CurrentProject.ConfigPath);
            Process p = new Process();
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "cmd.exe";
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = !ReadShowDebugConsole(); //when we want to show debug console, we don't want to redirect output
            info.UseShellExecute = false;
            info.Verb = "runas";
            info.CreateNoWindow = !ReadShowDebugConsole(); //if show debug console = true, then create no window has to be false

            this.Dispatcher.Invoke(() => {
                LoadingWindow = new LoadingWindow(); //show a loading window during training that takes cmd's output (DeepLabCut's train_network function generated output) so the user knows how far along they are
                LoadingWindow.Title = "Training";
                LoadingWindow.Show();
                LoadingWindow.Closed += LoadingClosed;
                LoadingWindow.ProgressBar.Maximum = int.Parse(CurrentProject.EndIters);
            });


            //NONDEBUG -----------------------------------------------------------------------------------------------
            if (info.CreateNoWindow) { //if not in debug mode
                int currentIter = 0;
                StringBuilder output = new StringBuilder();
                p.OutputDataReceived += new DataReceivedEventHandler((sender, e) => {
                    if (!String.IsNullOrEmpty(e.Data)) {
                        string line = e.Data;
                        //Console.WriteLine(line);
                        if (line.Contains("OOM")) {
                            errorMessage = "Training failed due to insufficient GPU memory. Try setting \"Global Scale\" to a lower value, and/or reducing training videos' resolution during import.";
                            errorDuringTraining = true;
                            FileSystemUtils.MurderPython();
                        }

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
            }
            //NONDEBUG -----------------------------------------------------------------------------------------------



            p.EnableRaisingEvents = true;
            p.Exited += (sender1, e1) => {

                if (errorDuringTraining) {
                    this.Dispatcher.Invoke(() => {
                        LoadingWindow.Close();
                        EnableInteraction();
                    });
                    UpdateVGLConfig(); //update VDLC's config file
                    MessageBox.Show(errorMessage, "Error Occurred", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else {
                    globalStopWatch.Stop();
                    TimeSpan ts = globalStopWatch.Elapsed;
                    string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                    this.Dispatcher.Invoke(() => {
                        LoadingWindow.Close();
                    });
                    CurrentProject.TrainedWith = new List<string>();
                    foreach (TrainingVideo current in TrainingListBox.ItemsSource) CurrentProject.TrainedWith.Add(current.Path);
                    UpdateVGLConfig(); //update VDLC's config file
                    DeleteEvalFiles(); //overwrite eval files
                    EvalNetwork(elapsedTime);
                }
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

                    if (info.CreateNoWindow == false) { //for debug purposes
                        sw.WriteLine("ECHO WHEN YOU'RE DONE, CLOSE THIS WINDOW");
                        p.WaitForExit();
                        sw.WriteLine("Done, exiting.");
                    }
                }
            }

            if(info.CreateNoWindow) p.BeginOutputReadLine();
        }











        //MARK: Evaluation Methods
        private void EvalNetwork(string elapsedTime) { //evaluate the newly trained network using DeepLabCut's evaluate_network function
            BarInteraction();
            string filePath = EnvDirectory + "\\vdlc_eval_network.py";
            FileSystemUtils.MurderPython();
            FileSystemUtils.RenewScript(filePath, AllScripts.EvalNetwork);
            FileSystemUtils.ReplaceStringInFile(filePath, "config_path_identifier", CurrentProject.ConfigPath);
            Process p = new Process();
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "cmd.exe";
            info.RedirectStandardInput = true;
            info.UseShellExecute = false;
            info.Verb = "runas";
            info.CreateNoWindow = !ReadShowDebugConsole(); //if show debug console = true, then create no window has to be false

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

                    if (info.CreateNoWindow == false) { //for debug purposes
                        sw.WriteLine("ECHO WHEN YOU'RE DONE, CLOSE THIS WINDOW");
                        p.WaitForExit();
                        sw.WriteLine("Done, exiting.");
                    }
                }
            }
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
            int saveIters = int.Parse(dialog.endItersTextBox.Text);
            CurrentProject.SaveIters = (saveIters / 5).ToString(); //save every 20%
            CurrentProject.EndIters = dialog.endItersTextBox.Text;
            CurrentProject.GlobalScale = dialog.GlobalScaleSlider.Value;
            UpdateVGLConfig();
            UpdateFramesToExtract();
        }
    }
}
