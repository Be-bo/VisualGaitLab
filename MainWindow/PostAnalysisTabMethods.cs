using System.Windows;
using VisualGaitLab.SupportingClasses;
using System.IO;
using System.Collections.Generic;
using System;
using Microsoft.Win32;
using System.Linq;
using VisualGaitLab.PostAnalysis;
using System.Windows.Controls;
using System.Diagnostics;
using VisualGaitLab.OtherWindows;
using System.Globalization;
using System.Windows.Data;

namespace VisualGaitLab
{
    public class AddOneConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (int)value + 1;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }


    public partial class MainWindow : Window
    {


        private void PreparePostAnalysisTab()
        { //set up the Post Analysis tab = grab the list of python scripts from the scripts folder and make them show up here
            PAScripts = new List<CustomScript>();
            DraggedScripts = new List<CustomScript>();
            string scriptsListFilePath = Path.Combine(ScriptsFolder, ScriptsListFile);

            if (!Directory.Exists(ScriptsFolder))
            { // Create directory
                Directory.CreateDirectory(ScriptsFolder);
                Console.WriteLine("CUSTOM SCRIPTS FOLDER NOT FOUND");
            }
            if (!File.Exists(scriptsListFilePath))
            { // Create script list file
                using (File.Create(Path.Combine(ScriptsFolder, ScriptsListFile))) { }
                Console.WriteLine("SCRIPT LIST FILE NOT FOUND");
            }

            // Read all scripts listed in the list
            HashSet<string> scriptPaths = new HashSet<string>();
            using (StreamReader file = new StreamReader(scriptsListFilePath))
            {
                while (file.Peek() >= 0)
                {
                    string line = file.ReadLine().Trim();
                    if (line != "")
                    {
                        scriptPaths.Add(line);
                    }
                }
            }
            // Extract all Python files in Directory
            foreach (string s in Directory.GetFiles(ScriptsFolder, "*.py"))
            {
                scriptPaths.Add(Path.Combine(ScriptsFolder, s));
            }

            // Create script objects
            List<string> notfound = new List<string>();
            foreach (string s in scriptPaths)
            {
                if (File.Exists(s))
                {
                    PAScripts.Add(new CustomScript(s));
                }
                else
                {
                    notfound.Add(s);
                }
            }

            // Some scripts not found
            if (notfound.Count > 0)
            { 
                MessageBoxResult result = MessageBox.Show("The following custom scripts were moved or removed:\n\t- "
                    + string.Join("\n\t- ", notfound)
                    + "\n\nRemove from List?","Scripts Not Found", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                { // Remove from scriptList
                    RewriteScriptsList();
                }
            }
            ScriptListBox.ItemsSource = null;
            ScriptListBox.ItemsSource = PAScripts;

            DragScriptsListBox.ItemsSource = null;
            DragScriptsListBox.ItemsSource = DraggedScripts;
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Script List Box Functions
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        private void RemoveScriptClicked(object sender, RoutedEventArgs e)
        {
            CustomScript selectedScript = (CustomScript)ScriptListBox.SelectedItem;
            if (selectedScript.Path.Contains(ScriptsFolder))
            { // Scripts in the script folder are permanent and shouldn't be removed
                MessageBox.Show("VGL's built-in post analysis scripts cannot be removed. \n\n" + selectedScript.Path, "Script Cannot Be Removed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int selectedIndex = ScriptListBox.SelectedIndex;
            PAScripts.RemoveAt(selectedIndex);
            ScriptListBox.Items.Refresh();
            RewriteScriptsList();
        }




        private void RewriteScriptsList()
        {
            using (StreamWriter sw = new StreamWriter(Path.Combine(ScriptsFolder, ScriptsListFile), false))
            {
                foreach (CustomScript s in PAScripts)
                {
                    sw.WriteLine(s.Path);
                }
            }
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Drag And Drop List Box Functions
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        private void DragScriptsListBox_Drop(object sender, DragEventArgs e)
        {
            DragTarget = (ListBox)sender;
            CustomScript draggedData = (CustomScript)(DragSource.SelectedItem);
            DraggedScripts.Add(draggedData);
            DragScriptsListBox.Items.Refresh();
            DragScriptsListBox.AlternationCount = DragScriptsListBox.Items.Count;
            ScriptDragNDropLabel.Visibility = Visibility.Collapsed;
        }


        private void RemoveFromScriptDropClicked(object sender, RoutedEventArgs e)
        {
            int selectedIndex = DragScriptsListBox.SelectedIndex;
            DraggedScripts.RemoveAt(selectedIndex);
            DragScriptsListBox.Items.Refresh();

            if (DraggedScripts.Count == 0) 
                ScriptDragNDropLabel.Visibility = Visibility.Visible;
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Buttons Functions
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void NewScriptClicked(object sender, RoutedEventArgs e)
        { // Add script path to scriptList.txt
            BarInteraction();
            //open a file dialog to let the user choose which video to add
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Python Script | *.py";
            openFileDialog.InitialDirectory = ScriptsFolder;
            openFileDialog.Multiselect = true;
            openFileDialog.Title = "Select Custom Script(s)";

            if (openFileDialog.ShowDialog() == true)
            {
                // Create a list of current script paths
                foreach (var fullPath in openFileDialog.FileNames)
                {
                    if (PAScripts.Any(s => s.Path == fullPath))
                    {
                        MessageBox.Show("The script file, \"" + fullPath + "\" has already been added.", "Script Already Added", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        PAScripts.Add(new CustomScript(fullPath));
                    }
                }

                // Add path to scripts list
                RewriteScriptsList();
            }
            ScriptListBox.Items.Refresh();
            EnableInteraction();
        }



        private void RefreshScriptsButton_Click(object sender, RoutedEventArgs e)
        { // Reload scripts list
            PreparePostAnalysisTab();
        }



        private void RunScriptButton_Click(object sender, RoutedEventArgs e)
        {
            var items = DragScriptsListBox.Items.Count > 0 ? DragScriptsListBox.Items : ScriptListBox.SelectedItems;
            List<CustomScript> scripts = new List<CustomScript>();
            List<string> args = new List<string>();
            bool runScript = false;

            for (int i = 0; i < items.Count; i++)
            {
                var script = (CustomScript)items[i];

                BarInteraction();
                string addon = '(' + (i+1).ToString() + " / " + items.Count + ')';
                SelectParamWindow paWindow = new SelectParamWindow(script.Path, script.Name, GaitVideos, WorkingDirectory, addon);
                if (paWindow.ShowDialog() == true)
                {
                    // Build parameters
                    string parameters = "";
                    foreach (Parameter p in paWindow.ParamList)
                    {
                        parameters += " \"" + p.Txt + '\"';
                    }

                    // Only run this script if parameters were found
                    scripts.Add(script);
                    args.Add(parameters);
                    runScript = true;
                }
            }

            if (runScript) RunScripts(scripts, args); 
            EnableInteraction();
        }



        private void RunScripts(List<CustomScript> scripts, List<string> args)
        {
            // Prepare cmd process
            Process process = new Process(); //prepare a cmd process to run the script
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "cmd.exe";
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = !ReadShowDebugConsole();
            info.UseShellExecute = false;
            info.Verb = "runas";
            info.CreateNoWindow = !ReadShowDebugConsole(); //if show debug console = true, then create no window has to be false

            Dispatcher.Invoke(() => //once done close the loading window
            {
                LoadingWindow = new LoadingWindow();
                LoadingWindow.Title = "Running Scripts";
                LoadingWindow.Show();
                LoadingWindow.Closed += LoadingClosed;
                LoadingWindow.ProgressBar.Maximum = scripts.Count;
            });

            bool errorDuringAnalysis = false;
            string errorMessage = "No Error";
            int scriptProgValue = 0;

            //NONDEBUG -----------------------------------------------------------------------------------------------
            if (info.CreateNoWindow)
            {
                process.OutputDataReceived += new DataReceivedEventHandler((sender, e) => //feed cmd output to the loading window so the user knows the progress of the analysis
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        string line = e.Data;
                        Console.WriteLine(line);

                        if (line.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            errorMessage = line;
                            errorDuringAnalysis = true;
                            FileSystemUtils.MurderPython();
                        }

                        if (line.Contains("python"))
                        {
                            string scriptName = scripts[scriptProgValue].Name;
                            scriptProgValue++;
                            Dispatcher.Invoke(() => {
                                LoadingWindow.ProgressLabel.Content = "Running " + scriptName + " (" + scriptProgValue + "/" + scripts.Count + ")";
                                LoadingWindow.ProgressBar.Value = scriptProgValue;
                            });
                        }
                    }
                });
            }
            //NONDEBUG -----------------------------------------------------------------------------------------------

            process.EnableRaisingEvents = true;
            process.Exited += (sender1, e1) =>
            {
                if (errorDuringAnalysis)
                {
                    MessageBox.Show(errorMessage, "Error Occurred", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                Dispatcher.Invoke(() =>
                {
                    LoadingWindow.Close();
                    EnableInteraction();
                });
            };

            process.StartInfo = info;
            process.Start();

            using (StreamWriter sw = process.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    sw.WriteLine(Drive);
                    sw.WriteLine("cd " + EnvDirectory);
                    sw.WriteLine(FileSystemUtils.CONDA_ACTIVATE_PATH);
                    sw.WriteLine("conda activate " + EnvName);


                    // Multiple Scripts
                    for (int i = 0; i < scripts.Count; i++)
                    {
                        sw.WriteLine("python \"" + scripts[i].Path +  "\"" + args[i]);
                    }

                    if (!info.CreateNoWindow)
                    { //for debug purposes
                        sw.WriteLine("\n\n\nECHO WHEN YOU'RE DONE, CLOSE THIS WINDOW");
                        process.WaitForExit();
                        sw.WriteLine("Done, exiting.");
                    }
                }
            }
            //only redirect output if debug enabled
            if (info.CreateNoWindow)
                process.BeginOutputReadLine();
        }
    }
}
