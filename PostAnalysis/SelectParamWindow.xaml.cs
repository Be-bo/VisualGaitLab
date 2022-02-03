using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace VisualGaitLab.PostAnalysis
{
    public partial class PostAnalysisWindow : Window
    {
        public List<Parameter> ParamList = new List<Parameter>();

        private string WorkingDirectory;
        private string ScriptPath;
        private int ParamNum = 0;
        private Parameter edittingParam = null;


        public PostAnalysisWindow(string scriptPath, string scriptName, string workingDir)
        {
            InitializeComponent();
            Title = scriptName;
            ScriptPath = scriptPath;
            WorkingDirectory = workingDir;
            ParamListBox.ItemsSource = ParamList;
            ReadInfo();
        }


        // Read info.txt if it exists and fill in appropriate params
        private void ReadInfo()
        {
            string infoDir = ScriptPath.Replace(".py", "");
            string infoFile = Path.Combine(infoDir, "info.txt");

            if (Directory.Exists(infoDir) & File.Exists(infoFile))
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
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = WorkingDirectory;
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
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
            if (ParamListBox.SelectedItem == edittingParam)
            {
                return;
            }

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
