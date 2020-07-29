using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using VisualGaitLab.OtherWindows;
using VisualGaitLab.SupportingClasses;

namespace VisualGaitLab {
    public partial class MainWindow : Window {




        // MARK: Project Creation Methods
        private void NewProjectClicked(object sender, RoutedEventArgs e) {
            if (MessageBox.Show("Are you planning to do Gait Analysis (rodents, bottom view)? If so you won't be able to use custom markers (we need specific bodyparts tracked to calculate gait metrics correctly). ", "Gait Project?", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                CreateProjectDialog newProjectDialog = new CreateProjectDialog(true);
                if (newProjectDialog.ShowDialog() == true) {
                    BarInteraction();
                    CreateNewProject(newProjectDialog, true);
                }
            }
            else {
                CreateProjectDialog newProjectDialog = new CreateProjectDialog(false);
                if (newProjectDialog.ShowDialog() == true) {
                    BarInteraction();
                    CreateNewProject(newProjectDialog, false);
                }
            }
        }

        private bool ProjectExists(Project proj) {
            if (File.Exists(proj.ConfigPath)) return true;
            else return false;
        }


        private void CreateNewProject(CreateProjectDialog dialog, bool gaitOnly) //call create_new_project function from DeepLabCut
        {
            Project newProject = new Project();
            newProject.IsGaitOnly = gaitOnly;
            newProject.TrainedWith = new List<string>();
            newProject.DisplayIters = "10";
            newProject.BodyParts = dialog.bodyParts;
            for (int i = 0; i < newProject.BodyParts.Count; i++) newProject.BodyParts[i] = "- " + newProject.BodyParts[i];
            string month = DateTime.Now.Month.ToString();
            string day = DateTime.Now.Day.ToString();
            if (month.Length < 2) month = "0" + month;
            if (day.Length < 2) day = "0" + day;
            newProject.DateIdentifier = DateTime.Now.Year.ToString() + "-" + month + "-" + day;
            newProject.Name = dialog.projectNameTextBox.Text;
            newProject.Scorer = dialog.authorTextBox.Text;
            newProject.TrainingVideos = new List<TrainingVideo>();
            string filePath = EnvDirectory + "\\vdlc_create_new_project.py";
            string copyVideosBool = "True";
            newProject.ConfigPath = WorkingDirectory + "\\" + newProject.Name + "-" + newProject.Scorer + "-" + newProject.DateIdentifier + "\\" + "config.yaml";
            if (!ProjectExists(newProject)) {
                FileSystemUtils.MurderPython();
                FileSystemUtils.RenewScript(filePath, AllScripts.CreateProject); //prepare the script for creating a new project
                FileSystemUtils.ReplaceStringInFile(filePath, "project_name_identifier", newProject.Name);
                FileSystemUtils.ReplaceStringInFile(filePath, "scorer_identifier", newProject.Scorer);
                FileSystemUtils.ReplaceStringInFile(filePath, "working_directory_identifier", WorkingDirectory);
                FileSystemUtils.ReplaceStringInFile(filePath, "copy_videos_identifier", copyVideosBool);

                Process p = new Process(); //prepare the process to run the script
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = "cmd.exe";
                info.RedirectStandardInput = true;
                info.UseShellExecute = false;
                info.Verb = "runas";
                info.CreateNoWindow = true;

                p.EnableRaisingEvents = true;
                p.Exited += (sender1, e1) => //once finished, update bodyparts, update training split, vdlc config and sync ui
                {
                    CurrentProject = newProject;
                    this.Dispatcher.Invoke(() => {
                        TrainButton.Visibility = Visibility.Visible;
                        TrainingAddDock.Visibility = Visibility.Visible;
                    });
                    SetBodyParts(CurrentProject);
                    UpdateTrainingSplitInConfig();
                    UpdateVdlcConfig();
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
                        sw.WriteLine("ipython vdlc_create_new_project.py");
                    }
                }
            }
            else {
                EnableInteraction();
                MessageBox.Show("Your project cannot be created because a project with that name already exists. Go to \\Program Files (x86)\\VDLC\\Projects and delete the old project folder.", "Cannot Create Project", MessageBoxButton.OK);
            }

        }












        // MARK: Loading Project - Training Videos

        private void OpenProjectClicked(object sender, RoutedEventArgs e) {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = WorkingDirectory;
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok) {
                CurrentProject = new Project();
                CurrentProject.DisplayIters = "10";
                CurrentProject.ConfigPath = dialog.FileName + "\\config.yaml";
                TrainingListBox.ItemsSource = null;
                AnalyzedListBox.ItemsSource = null;
                MainTabControl.SelectedIndex = 0; //reset to the first tab
            }
            if (CurrentProject != null) {
                SyncUI();
            }
        }

        private void LoadProject(string projectPath) {
            string folderName = projectPath.Substring(projectPath.LastIndexOf("\\") + 1);
            int dashCount = 0;
            for (int i = 0; i < folderName.Length; i++) if (folderName.ElementAt(i).Equals('-')) dashCount++;

            // Check if the folder's legit
            if (!folderName.Contains(".") && dashCount == 4) {
                string configPath = projectPath + "\\config.yaml";
                // Check whether the config file exists or not
                if (File.Exists(configPath)) {
                    // Set the basic properties of the project
                    CurrentProject.ConfigPath = configPath;
                    CurrentProject.Name = folderName.Substring(0, folderName.IndexOf("-"));
                    this.Title = CurrentProject.Name;
                    folderName = folderName.Substring(folderName.IndexOf("-") + 1);
                    CurrentProject.Scorer = folderName.Substring(0, folderName.IndexOf("-"));
                    CurrentProject.DateIdentifier = folderName.Substring(folderName.IndexOf("-") + 1);
                    CurrentProject.TrainingVideos = new List<TrainingVideo>();

                    // Check for the training videos and whether each has been labeled or not
                    string videosPath = projectPath + "\\videos";
                    var videoFiles = Directory.GetFiles(videosPath);
                    for (int i = 0; i < videoFiles.Length; i++) {
                        if (videoFiles[i].Contains(".mp4") || videoFiles[i].Contains(".avi")) {
                            TrainingVideo newVid = new TrainingVideo();
                            newVid.Name = videoFiles[i].Substring(videoFiles[i].LastIndexOf("\\") + 1, videoFiles[i].LastIndexOf(".") - videoFiles[i].LastIndexOf("\\") - 1);
                            newVid.Path = videoFiles[i];
                            newVid.FramesExtracted = false;
                            newVid.ExtractedImageName = "cross.png";
                            newVid.FramesLabeled = false;
                            newVid.LabeledImageName = "cross.png";
                            newVid.Frames = 0;

                            string labeledDataSingleVideoPath = projectPath + "\\labeled-data\\" + newVid.Name;
                            if (Directory.Exists(labeledDataSingleVideoPath)) //labeled folder exists, check if the frames have been extracted & labeled | just extracted
                            {
                                var allFiles = Directory.GetFiles(labeledDataSingleVideoPath);
                                int frameCount = 0;
                                for (int j = 0; j < allFiles.Length; j++) {
                                    if (allFiles[j].Contains(".png")) { frameCount++; }
                                    if (allFiles[j].Contains(".h5")) { newVid.FramesLabeled = true; newVid.LabeledImageName = "check.png"; }
                                }
                                newVid.Frames = frameCount;
                                if (newVid.Frames > 0) { newVid.FramesExtracted = true; newVid.ExtractedImageName = "check.png"; }

                            }
                            else { //labeled folder doesn't exists, on the video has been added
                                newVid.Frames = 0;
                                newVid.FramesLabeled = false;
                                newVid.FramesExtracted = false;
                            }
                            newVid.ThumbnailPath = newVid.Path.Substring(0, newVid.Path.LastIndexOf(".")) + ".png";
                            CurrentProject.TrainingVideos.Add(newVid);
                        }
                    }

                    // Read project's bodyparts and num of frames from the config file
                    List<string> configFile = new List<string>();
                    CurrentProject.BodyParts = new List<string>();
                    CurrentProject.FramesToExtract = "5";
                    int bodypartsTitlePos = 0;
                    int endOfBodypartsPos = 0;
                    using (var reader = new StreamReader(configPath)) {
                        int index = 0;
                        string line;
                        while (!reader.EndOfStream) {
                            line = reader.ReadLine();
                            configFile.Add(line);
                            if (line.Contains("numframes2pick: ")) {
                                string strFrames = line;
                                strFrames = strFrames.Replace("numframes2pick: ", "");
                                CurrentProject.FramesToExtract = Regex.Replace(strFrames, @"\t|\n|\r", "");
                            }
                            if (line.Contains("bodyparts:")) bodypartsTitlePos = index;
                            if (line.Contains("start:")) endOfBodypartsPos = index;
                            index++;
                        }
                    }
                    for (int i = bodypartsTitlePos + 1; i < endOfBodypartsPos; i++) {
                        CurrentProject.BodyParts.Add(Regex.Replace(configFile[i], @"\t|\n|\r", ""));
                    }

                    //Check if the project's been trained
                    CheckIfTrained();
                    //Load Analysis videos
                    LoadAnalysisVideos();
                    if (CurrentProject.AnalysisVideos.Count > 0) EnableAnalysisPart();
                    //Load VDLC Config
                    ReadVdlcConfig();
                }
                else {

                }
            }
            else {

            }
        }



















        // MARK: Loading Project - Analysis Videos

        private void LoadAnalysisVideos() //load analysis video into the CurrentProject variable from the "analyzed-videos" folder
        {
            string analyzedVideosPath = CurrentProject.ConfigPath.Substring(0, CurrentProject.ConfigPath.LastIndexOf("\\"));
            analyzedVideosPath = analyzedVideosPath + @"\analyzed-videos";
            CurrentProject.AnalysisVideos = new List<AnalysisVideo>();
            if (Directory.Exists(analyzedVideosPath)) {
                var dirs = Directory.EnumerateDirectories(analyzedVideosPath);
                foreach (var dir in dirs) {
                    AnalysisVideo newVideo = new AnalysisVideo();
                    newVideo.Name = dir.Substring(dir.LastIndexOf("\\") + 1);
                    newVideo.ThumbnailPath = "thumbnail.png";
                    var files = Directory.GetFiles(analyzedVideosPath + "\\" + newVideo.Name);
                    newVideo.IsAnalyzed = false;
                    newVideo.AnalyzedImageName = "cross.png";
                    foreach (var file in files) {
                        if ((file.Contains(".mp4") || file.Contains(".avi")) && (!file.Contains("labeled"))) { newVideo.Path = file; }
                        if (file.Contains(".h5")) { newVideo.IsAnalyzed = true; newVideo.AnalyzedImageName = "check.png"; } //check if teh video is analyzed
                        if (file.Contains(".png")) newVideo.ThumbnailPath = file;
                    }
                    if (newVideo.Path != null) CurrentProject.AnalysisVideos.Add(newVideo);
                }
            }
        }








        // MARK: Network Related Methods

        private void CheckIfTrained() //check if the current project's network is trained and load training settings
        {
            BarAnalysisPart();
            string trainPath = CurrentProject.ConfigPath.Substring(0, CurrentProject.ConfigPath.LastIndexOf("\\")) + @"\dlc-models\iteration-0\";
            if (Directory.Exists(trainPath)) {
                var allFolders = Directory.EnumerateDirectories(trainPath);
                if (allFolders != null && allFolders.First().Contains(CurrentProject.Name)) {
                    string tempDir = allFolders.First().Substring(allFolders.First().LastIndexOf("\\") + 1);
                    trainPath = trainPath + tempDir + "\\train";
                    GetPoseCfgSettings(trainPath);
                    var trainFiles = Directory.GetFiles(trainPath);
                    for (int i = 0; i < trainFiles.Length; i++) {
                        if (trainFiles[i].Contains(".meta") && trainFiles[i].Contains(CurrentProject.EndIters)) {
                            string trainedWithPath = CurrentProject.ConfigPath.Substring(0, CurrentProject.ConfigPath.LastIndexOf("\\")) + "\\trainedwith.txt";
                            if (File.Exists(trainedWithPath)) {
                                bool trained = true;
                                StreamReader sr = new StreamReader(trainedWithPath);
                                String[] rows = Regex.Split(sr.ReadToEnd(), "\r\n");
                                foreach (TrainingVideo vid in CurrentProject.TrainingVideos) {
                                    if (!rows.Contains(vid.Name)) {
                                        CurrentProject.IsTrained = false;
                                        trained = false;
                                        sr.Close();
                                        break;
                                    }
                                }
                                if (CurrentProject.TrainingVideos.Count < 1) trained = false;
                                if (trained) {
                                    sr.Close();
                                    CurrentProject.IsTrained = true;
                                    EnableAnalysisPart();
                                    SetHint("Your project's network is trained, it's time to analyze some videos. Click 'Add Video' on the right, once it's added, select it and click 'Analyze Video'.");
                                }
                                break;
                            }
                            else {
                                CurrentProject.IsTrained = true;
                                EnableAnalysisPart();
                                SetHint("Your project's network is trained, it's time to analyze some videos. Click 'Add Video' on the right, once it's added, select it and click 'Analyze Video'.");
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
