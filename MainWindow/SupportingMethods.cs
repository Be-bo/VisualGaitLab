﻿using Microsoft.Win32;
using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using VisualGaitLab.OtherWindows;
using VisualGaitLab.SupportingClasses;

namespace VisualGaitLab {
    public partial class MainWindow : Window {

        public void LoadingClosed(object sender, System.EventArgs e) {
            FileSystemUtils.MurderPython();
            EnableInteraction();
        }

        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            FileSystemUtils.MurderPython();
        }

        public static void CreateThumbnailForVideo(String vidPath, String targetPath) {
            using (var engine = new Engine()) {
                var vid = new MediaFile { Filename = vidPath };
                var options = new ConversionOptions { Seek = TimeSpan.FromSeconds(0) };
                var outputFile = new MediaFile { Filename = string.Format(targetPath) };
                engine.GetThumbnail(vid, outputFile, options);
            }
        }

        public static void GetEvalResultsSaveTime(ref Project proj) { //right after training we get the .csv evaluation results file generated by DLC and save our StopWatch time to it of how long it took to train
            if (proj != null) {
                string evalFolder = proj.ConfigPath.Substring(0, proj.ConfigPath.LastIndexOf("\\")) + "\\evaluation-results\\iteration-0";
                string evalFile = "";
                if (Directory.Exists(evalFolder)) {
                    var folders = Directory.EnumerateDirectories(evalFolder);
                    foreach (var folder in folders) { //only one folder, just grab it
                        var files = Directory.EnumerateFiles(folder);
                        foreach (var file in files) {
                            if (file.Contains(".csv")) { //the only distinguishing characteristic is that it's a csv file
                                evalFile = file;
                                break;
                            }
                        }
                        break;
                    }
                    if (!evalFile.Equals("")) {
                        StreamReader sr = new StreamReader(evalFile);
                        String[] rows = Regex.Split(sr.ReadToEnd(), "\r\n");
                        sr.Close();
                        rows[0] = rows[0] + ",Train Time"; //append current train time to the file lines
                        rows[1] = rows[1] + "," + proj.TrainTime;
                        string[] secondRow = rows[1].Split(','); //second row contains actual nums, so we split using comma
                        if (secondRow.Length >= 7) {
                            proj.TrainError = secondRow[4];
                            proj.TestError = secondRow[5];
                            proj.PCutoff = secondRow[6];
                        }

                        StreamWriter sw = new StreamWriter(evalFile);
                        for (int i = 0; i < rows.Length; i++) { //and we just write all the lines back into the file like a good boi
                            sw.WriteLine(rows[i]);
                        }
                        sw.Close();
                    }
                }
            }
        }

        public static void GetAllEvalResults(ref Project proj) { //just read the evaluation .csv file
            if (proj != null) {
                string evalFolder = proj.ConfigPath.Substring(0, proj.ConfigPath.LastIndexOf("\\")) + "\\evaluation-results\\iteration-0";
                string evalFile = "";
                if (Directory.Exists(evalFolder)) {
                    var folders = Directory.EnumerateDirectories(evalFolder);
                    foreach (var folder in folders) { //only one folder, just grab it
                        var files = Directory.EnumerateFiles(folder);
                        foreach (var file in files) {
                            if (file.Contains(".csv")) { //the only distinguishing characteristic is that it's a csv file
                                evalFile = file;
                                break;
                            }
                        }
                        break;
                    }
                    if (!evalFile.Equals("")) {
                        StreamReader sr = new StreamReader(evalFile);
                        String[] rows = Regex.Split(sr.ReadToEnd(), "\r\n");
                        sr.Close();
                        rows[0] = rows[0] + ",Train Time"; //append current train time to the file lines
                        rows[1] = rows[1] + "," + proj.TrainTime;
                        string[] secondRow = rows[1].Split(','); //second row contains actual nums, so we split using comma
                        if (secondRow.Length >= 10) {
                            proj.TrainError = secondRow[4];
                            proj.TestError = secondRow[5];
                            proj.PCutoff = secondRow[6];
                            proj.TrainTime = secondRow[9];
                        }
                    }
                }
            }
        }




        // MARK: Add Video function


        private void AddNewVideo(String vidoePath, bool isAnalysisVid) //logic for adding a training/analysis video
        {
            //open a file dialog to let the user choose which video to add
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Title = "Select videos";

            if (openFileDialog.ShowDialog() == true)
            {
                bool syncUI = false;

                foreach (var fullPath in openFileDialog.FileNames)
                {
                    if (fullPath.ToLower().EndsWith(".avi") || fullPath.ToLower().EndsWith(".mp4") || fullPath.ToLower().EndsWith(".wmv") || fullPath.ToLower().EndsWith(".mov"))
                    {
                        if (!FileSystemUtils.NameAlreadyInDir(FileSystemUtils.ExtendPath(FileSystemUtils.GetParentFolder(CurrentProject.ConfigPath), vidoePath), FileSystemUtils.GetFileNameWithExtension(fullPath)))
                        {
                            if (FileSystemUtils.FileNameOk(fullPath))
                            {
                                ImportWindow window = new ImportWindow(fullPath, CurrentProject.ConfigPath, isAnalysisVid, EnvDirectory, EnvName, Drive, ProgramFolder);
                                if (window.ShowDialog() == true) syncUI = true;
                            }
                            else
                                MessageBox.Show("File names must be 25 characters or less, with only alphanumeric characters, dashes, and underscores allowed.", "Invalid Name", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                            MessageBox.Show("Video with a similar or an identical name has already been added. Please rename your new video.", "Name Already Taken", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                        MessageBox.Show("Video cannot be added. Your video format is not supported.", "Unsupported Action", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                if (syncUI) SyncUI();
            }
            EnableInteraction();
        }
    }
}
