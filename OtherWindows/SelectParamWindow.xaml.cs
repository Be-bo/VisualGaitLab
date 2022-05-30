using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using VisualGaitLab.OtherWindows;
using VisualGaitLab.SupportingClasses;

namespace VisualGaitLab.PostAnalysis
{
    public partial class SelectParamWindow : Window
    {
        public List<Parameter> ParamList = new List<Parameter>();

        private string WorkingDirectory;
        private string ScriptName;
        private string ScriptPath;
        private int ParamNum = 0;
        private Parameter edittingParam = null;
        private List<AnalysisVideo> gaitAnalyzedVideos;


        public SelectParamWindow(string scriptPath, string scriptName, List<AnalysisVideo> gaitVideos, string workingDir, string addon = "")
        {
            InitializeComponent();
            Title = scriptName + ' ' + addon;
            ScriptName = scriptName;
            ScriptPath = scriptPath;
            WorkingDirectory = workingDir;
            ParamListBox.ItemsSource = ParamList;
            gaitAnalyzedVideos = new List<AnalysisVideo>();

            if (gaitVideos != null && gaitVideos.Count > 0)
            {
                AddVideoParam.IsEnabled = true;
                foreach (AnalysisVideo video in gaitVideos)
                    if (video.IsGaitAnalyzed) gaitAnalyzedVideos.Add(video);
            }

            ReadInfo();
        }


        // Read info.txt if it exists and fill in appropriate params
        private void ReadInfo()
        {
            string infoPath = ScriptPath.Replace(ScriptName + ".py", "");
            string infoFile = Path.Combine(infoPath, ScriptName + "_info.txt");

            if (File.Exists(infoFile))
            {
                // Read all info
                string info = File.ReadAllText(infoFile);

                // Read file line by line to find the parameters
                using (StreamReader file = new StreamReader(infoFile))
                {
                    while (file.Peek() >= 0)
                    {
                        string line = file.ReadLine();

                        // Extract Param number if it exists
                        if (ParamNum == 0 & (line.Contains("Param") | line.Contains("param") | line.Contains("PARAM")))
                        {
                            try
                            {
                                ParamNum = int.Parse(Regex.Replace(line, "[^0-9]", ""));
                            }
                            catch
                            {
                                Console.WriteLine("Couldn't Parse Parameter Number from line \"" + line + "\"");
                            }
                        }
                    }
                }

                // Set info
                InfoBlock.FontStyle = FontStyles.Normal;
                InfoBlock.Text = info;

                CheckParams();
            }
            else
            {
                Console.WriteLine("Couldn't find \"" + infoFile + "\"");
            }
        }

        // Check if there are enough params, then enable run button
        private void CheckParams()
        {
            // Update Param count
            if (ParamNum > 0) ParamTitle.Text = "Parameters (" + ParamListBox.Items.Count + "/" + ParamNum + ")";

            // Check if enough parameters, enable run button if yes
            if (ParamListBox.Items.Count >= ParamNum)
            {
                RunButton.IsEnabled = true;
            }
            else RunButton.IsEnabled = false;
        }


        ////////////////////////////////////////////////////////////////////////////
        // Add Parameters
        ////////////////////////////////////////////////////////////////////////////


        // Add a file path to params
        private void AddFileParam_Click(object sender, RoutedEventArgs e)
        {
            // Open a file dialog to let the user select multiple file parameters
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = WorkingDirectory;
            openFileDialog.Multiselect = true;
            openFileDialog.Title = "Select Parameter File(s)";

            if (openFileDialog.ShowDialog() == true)
            {
                // Create a list of current script paths
                foreach (string fullPath in openFileDialog.FileNames)
                {
                    ParamList.Add(new Parameter(fullPath));
                }

                ParamListBox.Items.Refresh();
                CheckParams();
            }
        }

        // Add a directory path to params
        private void AddDirParam_Click(object sender, RoutedEventArgs e)
        {
            var window = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = WorkingDirectory;
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog(window) == CommonFileDialogResult.Ok)
            {
                ParamList.Add(new Parameter(dialog.FileName));
                ParamListBox.Items.Refresh();
                CheckParams();
            }
        }

        // Add a new string
        private void AddStrParam_Click(object sender, RoutedEventArgs e)
        {
            EndEdit();

            edittingParam = new Parameter("New Parameter");
            ParamList.Add(edittingParam);
            ParamListBox.Items.Refresh();
            ParamListBox.SelectedItem = edittingParam;
            edittingParam.StartEdit();
            CheckParams();
        }

        // Add the path to the new analyzed video
        private void AddVideoParam_Click(object sender, RoutedEventArgs e)
        {
            SelectVideoWindow vidWindow = new SelectVideoWindow(gaitAnalyzedVideos);// (script.Path, script.Name, GaitVideos, WorkingDirectory, addon);
            if (vidWindow.ShowDialog() == true)
            {
                foreach (AnalysisVideo video in vidWindow.selectedVideos)
                {
                    ParamList.Add(new Parameter(video.Path.Replace("\\" + video.Name + ".avi", "")));
                }
                ParamListBox.Items.Refresh();
                CheckParams();
            }
        }


        ////////////////////////////////////////////////////////////////////////////
        // Edit Parameter
        ////////////////////////////////////////////////////////////////////////////


        // End Edit on Enter
        private void Param_txtBox_Enter(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Return)
            {
                EndEdit();
            }
        }


        // Start editting
        private void Param_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ParamListBox.SelectedItem == edittingParam) return;

            EndEdit();

            if (e.ClickCount == 2 & ParamListBox.SelectedItem != null)
            {
                edittingParam = ParamListBox.SelectedItem as Parameter;
                edittingParam.StartEdit();
                ParamListBox.Items.Refresh();
            }
        }


        // End Edit Function
        private void EndEdit()
        {
            if (edittingParam != null)
            {
                edittingParam.EndEdit();
                ParamListBox.Items.Refresh();
                edittingParam = null;
            }
        }


        ////////////////////////////////////////////////////////////////////////////
        // Reorder / Remove Parameters
        ////////////////////////////////////////////////////////////////////////////


        // Move selected param up in list
        private void MoveUpParam_Click(object sender, RoutedEventArgs e)
        {
            if (ParamListBox.SelectedItem != null)
            {
                Parameter p = ParamListBox.SelectedItem as Parameter;
                int i = ParamList.IndexOf(p);
                if (i > 0)
                {
                    ParamList[i] = ParamList[i - 1];
                    ParamList[i - 1] = p;
                }
                ParamListBox.Items.Refresh();
            }
        }

        // Move selected param down in list
        private void MoveDownParam_Click(object sender, RoutedEventArgs e)
        {
            if (ParamListBox.SelectedItem != null)
            {
                Parameter p = ParamListBox.SelectedItem as Parameter;
                int i = ParamList.IndexOf(p);
                if (i < ParamList.Count - 1)
                {
                    ParamList[i] = ParamList[i + 1];
                    ParamList[i + 1] = p;
                }
                ParamListBox.Items.Refresh();
            }
        }

        // Remove selected param
        private void RemoveParam_Click(object sender, RoutedEventArgs e)
        {
            ParamList.Remove(ParamListBox.SelectedItem as Parameter);
            ParamListBox.Items.Refresh();
            CheckParams();
        }




        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
