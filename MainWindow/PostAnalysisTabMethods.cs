using System.Windows;
using VisualGaitLab.SupportingClasses;
using System.IO;
using System.Collections.Generic;
using System;
using Microsoft.Win32;
using System.Linq;
using VisualGaitLab.PostAnalysis;
using System.Windows.Controls;

namespace VisualGaitLab
{
    public partial class MainWindow : Window
    {


        private void PreparePostAnalysisTab()
        { //set up the Post Analysis tab = grab the list of python scripts from the scripts folder and make them show up here
            PAScripts = new List<CustomScript>();
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
        }



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



        private void ScriptBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RunScriptButton.IsEnabled = true;
        }



        private void RefreshScriptsButton_Click(object sender, RoutedEventArgs e)
        { // Reload scripts list
            PreparePostAnalysisTab();
        }



        private void RunScriptButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO: multiple selection
            CustomScript script = (CustomScript)ScriptListBox.SelectedItem;
            if (script != null)
            {
                BarInteraction();
                PostAnalysisWindow paWindow = new PostAnalysisWindow(script.Path, script.Name);
                if (paWindow.ShowDialog() == true)
                {
                    //SyncUI(); //TODO: is it needed?
                }
                EnableInteraction();
            }
        }
    }
}
