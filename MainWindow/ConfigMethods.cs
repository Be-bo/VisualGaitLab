using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using VisualGaitLab.SupportingClasses;

namespace VisualGaitLab {
    public partial class MainWindow : Window {







        // MARK: PoseCFG Related Methods

        private string GetPoseCfgPath() { //get path of pose_cfg.yaml file
            string trainPath = CurrentProject.ConfigPath.Substring(0, CurrentProject.ConfigPath.LastIndexOf("\\"));
            trainPath = trainPath + @"\dlc-models\iteration-0";
            if (Directory.Exists(trainPath)) {
                var dirs = Directory.EnumerateDirectories(trainPath);
                if (dirs != null && dirs.First() != null) {
                    trainPath = trainPath + "\\" + dirs.First().Substring(dirs.First().LastIndexOf("\\") + 1) + "\\train\\pose_cfg.yaml";
                    return trainPath;
                }
                else {
                    return null;
                }
            }
            else {
                return null;
            }
        }

        private void UpdatePoseCfg(string path) { //update pose_cfg.yaml file
            StreamReader sr = new StreamReader(path);
            String[] rows = Regex.Split(sr.ReadToEnd(), "\r\n");
            sr.Close();

            int multiStepStart = 0;
            int multiStepEnd = 0;
            for (int i = 0; i < rows.Length; i++) {
                if (rows[i].Contains("display_iters:")) rows[i] = "display_iters: " + CurrentProject.DisplayIters;
                if (rows[i].Contains("save_iters:")) rows[i] = "save_iters: " + CurrentProject.SaveIters;
                if (rows[i].Contains("global_scale:")) rows[i] = "global_scale: " + CurrentProject.GlobalScale.ToString();
                if (rows[i].Contains("multi_step:")) multiStepStart = i + 1;
                if (rows[i].Contains("net_type:")) multiStepEnd = i;
            }

            for (int i = multiStepStart; i < multiStepEnd; i++) {
                rows[i] = "";
            }

            rows[multiStepStart] = "- - 0.005";
            rows[multiStepStart + 1] = "  - " + CurrentProject.EndIters;

            StreamWriter sw = new StreamWriter(path);
            for (int i = 0; i < rows.Length; i++) {
                sw.WriteLine(rows[i]);
            }
            sw.Close();
        }

        private void GetPoseCfgSettings(string trainPath) { //get current pose_cfg.yaml settings
            if (File.Exists(trainPath + "\\pose_cfg.yaml")) {
                StreamReader sr = new StreamReader(trainPath + "\\pose_cfg.yaml");
                String[] rows = Regex.Split(sr.ReadToEnd(), "\r\n");
                sr.Close();

                for (int i = 0; i < rows.Length; i++) {
                    if (rows[i].Contains("save_iters:")) {
                        CurrentProject.SaveIters = rows[i].Substring(rows[i].IndexOf(": ") + 2);
                        CurrentProject.SaveIters.Replace(" ", string.Empty);
                    }
                    /* if (rows[i].Contains("display_iters:"))
                     {
                         CurrentProject.DisplayIters = rows[i].Substring(rows[i].IndexOf(": ") + 2);
                         CurrentProject.DisplayIters.Replace(" ", string.Empty);
                     }*/
                    if (rows[i].Contains("net_type:")) {
                        int counterIndex = 1;
                        string line = rows[i - counterIndex];
                        while (!line.Contains("-")) {
                            counterIndex++;
                            line = rows[i - counterIndex];
                        }
                        CurrentProject.EndIters = line.Substring(line.IndexOf("- ") + 2);
                        CurrentProject.EndIters.Replace(" ", string.Empty);
                    }

                    if (rows[i].Contains("global_scale:")) {
                        CurrentProject.GlobalScale = double.Parse(rows[i].Substring(rows[i].IndexOf(": ") + 2));
                    }
                }
            }
        }












        // MARK: Config.yaml Related Methods

        private void UpdateFramesToExtract() { //update how many frames to extract for the next training video
            StreamReader sr = new StreamReader(CurrentProject.ConfigPath);
            String[] rows = Regex.Split(sr.ReadToEnd(), "\r\n");
            sr.Close();

            for (int i = 0; i < rows.Count(); i++) {
                if (rows[i].Contains("numframes2pick:")) {
                    rows[i] = "numframes2pick: " + CurrentProject.FramesToExtract;
                    break;
                }
            }

            StreamWriter sw = new StreamWriter(CurrentProject.ConfigPath);
            for (int i = 0; i < rows.Count(); i++) {
                sw.WriteLine(rows[i]);
            }
            sw.Close();
        }

        private void EditDotSizeInConfig(int pos) { //update dot size for analyzed labeled videos
            AnalysisVideo video = CurrentProject.AnalysisVideos[pos];
            StreamReader sr = new StreamReader(video.Path.Substring(0, video.Path.LastIndexOf("\\")) + "\\settings.txt");
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

            StreamReader sr1 = new StreamReader(CurrentProject.ConfigPath);
            String[] configRows = Regex.Split(sr1.ReadToEnd(), "\r\n");
            for (int i = 0; i < configRows.Count(); i++) {
                if (configRows[i].Contains("dotsize:")) {
                    configRows[i] = "dotsize: " + dotsize;
                    break;
                }
            }
            sr1.Close();

            StreamWriter sw = new StreamWriter(CurrentProject.ConfigPath);
            for (int i = 0; i < configRows.Count(); i++) {
                sw.WriteLine(configRows[i]);
            }
            sw.Close();
        }

        private void SetBodyParts(Project project) { //set body parts in the config file
            // read the config file
            List<string> configFile = new List<string>();
            int bodypartsTitlePos = 0;
            int endOfBodypartsPos = 0;
            using (var reader = new StreamReader(project.ConfigPath)) {
                int index = 0;
                string line;
                while (!reader.EndOfStream) {
                    line = reader.ReadLine();
                    configFile.Add(line);
                    if (line.Contains("bodyparts:")) bodypartsTitlePos = index;
                    if (line.Contains("start:")) {
                        endOfBodypartsPos = index;
                    }
                    index++;
                }
            }

            // remove the existing bodyparts
            for (int i = bodypartsTitlePos + 1; i < endOfBodypartsPos; i++) {
                configFile.RemoveAt(bodypartsTitlePos + 1);
            }

            // add this project's bodyparts
            for (int i = 0; i < project.BodyParts.Count; i++) {
                configFile.Insert(bodypartsTitlePos + 1, project.BodyParts[i]);
            }

            // write into the config file
            using (var writer = new StreamWriter(project.ConfigPath)) {
                for (int i = 0; i < configFile.Count; i++) {
                    writer.WriteLine(configFile[i]);
                }
            }
        }

        private void DeleteVidFromConfig(string configPath, string vidPath) { //delete video path from the config file so that DeepLabCut works properly
            StreamReader sr = new StreamReader(configPath);
            String[] rows = Regex.Split(sr.ReadToEnd(), "\r\n");
            List<string> listRows = new List<string>(rows);
            sr.Close();

            StreamWriter sw = new StreamWriter(configPath);
            for (int i = 0; i < listRows.Count; i++) {
                if (listRows[i].Contains(vidPath)) {
                    listRows.RemoveAt(i + 1); //the name itself
                    listRows.RemoveAt(i); //the crop info that's on the next line
                }
                sw.WriteLine(listRows[i]);
            }
            sw.Close();
        }

        private void UpdateTrainingSplitInConfig() { //update training split (0 - 1 with two decimals suggesting the percentage of training frames to allocate, the rest will go into the testing dataset)
            StreamReader sr1 = new StreamReader(CurrentProject.ConfigPath);
            String[] configRows = Regex.Split(sr1.ReadToEnd(), "\r\n");
            for (int i = 0; i < configRows.Count(); i++) {
                if (configRows[i].Contains("TrainingFraction:")) {
                    configRows[i + 1] = "- " + CurrentProject.TrainingSplit;
                    break;
                }
            }
            sr1.Close();

            StreamWriter sw = new StreamWriter(CurrentProject.ConfigPath);
            for (int i = 0; i < configRows.Count(); i++) {
                sw.WriteLine(configRows[i]);
            }
            sw.Close();
        }











        // MARK: VGL Project Settings Methods

        private void ReadVGLConfig() { //read VGL's config for this project, this file keeps track of which vids (if any) the network was trained with, if it's gait only and save and end iters
            string vdlcConfigPath = CurrentProject.ConfigPath.Substring(0, CurrentProject.ConfigPath.LastIndexOf("\\")) + "\\vdlc_config.txt";
            if (File.Exists(vdlcConfigPath)) {
                StreamReader sr = new StreamReader(vdlcConfigPath);
                String[] rows = Regex.Split(sr.ReadToEnd(), "\r\n");
                List<string> listRows = new List<string>(rows);
                sr.Close();


                for(int i = 0; i<listRows.Count; i++) {
                    string currentLine = listRows[i];

                    if (currentLine.Contains("gaitonly:")) {
                        if (currentLine.Contains("False")) CurrentProject.IsGaitOnly = false;
                        else CurrentProject.IsGaitOnly = true;
                    } else if ((currentLine.Length) > (currentLine.IndexOf(":") + 2) && currentLine.Contains("saveiters:")) CurrentProject.SaveIters = currentLine.Substring(currentLine.IndexOf(":") + 2, (currentLine.Length) - (currentLine.IndexOf(":") + 2));
                    else if ((currentLine.Length) > (currentLine.IndexOf(":") + 2) && currentLine.Contains("enditers:")) CurrentProject.EndIters = currentLine.Substring(currentLine.IndexOf(":") + 2, (currentLine.Length) - (currentLine.IndexOf(":") + 2));
                    else if ((currentLine.Length) > (currentLine.IndexOf(":") + 2) && currentLine.Contains("globalscale:")) CurrentProject.GlobalScale = double.Parse(currentLine.Substring(currentLine.IndexOf(":") + 2, (currentLine.Length) - (currentLine.IndexOf(":") + 2)));
                    else if (currentLine.Contains("trainedwith:")) {
                        int vidStart = i;
                        CurrentProject.TrainedWith = new List<string>();
                        for (int j = vidStart + 1; j < listRows.Count; j++) if (listRows[j].Length > 3) CurrentProject.TrainedWith.Add(listRows[j]);
                    }
                }
            }
        }

        private void UpdateVGLConfig() { //update vdlc config based on the current state of the project data
            string vdlcConfigPath = CurrentProject.ConfigPath.Substring(0, CurrentProject.ConfigPath.LastIndexOf("\\")) + "\\vdlc_config.txt";
            StreamWriter sw = new StreamWriter(vdlcConfigPath);
            List<string> listRows = new List<string>();

            sw.WriteLine("gaitonly: " + CurrentProject.IsGaitOnly);
            sw.WriteLine("saveiters: " + CurrentProject.SaveIters);
            sw.WriteLine("enditers: " + CurrentProject.EndIters);
            sw.WriteLine("globalscale: " + CurrentProject.GlobalScale);
            sw.WriteLine("trainedwith:");

            for (int i = 0; i < CurrentProject.TrainedWith.Count; i++) {
                sw.WriteLine(CurrentProject.TrainedWith[i]);
            }
            sw.Close();
        }







        //MARK: Other Methods

        private bool ReadShowDebugConsole() {
            string settingsPath = FileSystemUtils.ExtendPath(ProgramFolder, "settings.txt");
            bool retVal = false;
            if (File.Exists(settingsPath)) {
                StreamReader sr = new StreamReader(settingsPath);
                String[] rows = Regex.Split(sr.ReadToEnd(), "\r\n");
                List<string> listRows = new List<string>(rows);
                sr.Close();

                for (int i = 0; i < listRows.Count; i++) {
                    string currentLine = listRows[i];
                    if (currentLine.Contains("showdebugconsole: ") && currentLine.Contains("True"))  retVal = true;
                }
            }
            return retVal;
        }

        private void UpdateSettings(bool showDebugConsole) {
            string settingsPath = FileSystemUtils.ExtendPath(ProgramFolder, "settings.txt");
            StreamWriter sw = new StreamWriter(settingsPath);
            List<string> listRows = new List<string>(); //for later, now we only have one option
            sw.WriteLine("showdebugconsole: " + showDebugConsole.ToString());
            sw.Close();
        }
    }
}
