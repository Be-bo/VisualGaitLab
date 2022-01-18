using System.Windows;
using VisualGaitLab.SupportingClasses;
using System.IO;
using System.Windows.Controls;
using System.Collections.Generic;
using System;

namespace VisualGaitLab
{
    public partial class MainWindow : Window
    {


        private void PreparePostAnalysisTab()
        { //set up the Post Analysis tab = grab the list of python scripts from the scripts folder and make them show up here
            PAScripts = new List<CustomScript>();
            string scriptsFolder = Directory.GetCurrentDirectory() + "\\CustomScripts";
            string scriptsListFile = "scriptsList.txt";

            if (!Directory.Exists(scriptsFolder))
            { // Create directory
                Directory.CreateDirectory(scriptsFolder);
                Console.WriteLine("CUSTOM SCRIPTS FOLDER NOT FOUND");
            }
            if (!File.Exists(Path.Combine(scriptsFolder, scriptsListFile)))
            { // Create script list file
                using (File.Create(Path.Combine(scriptsFolder, scriptsListFile))) { }
                Console.WriteLine("SCRIPT LIST FILE NOT FOUND");
            }

            // Read all scripts listed in the list
            HashSet<string> scriptPaths = new HashSet<string>();
            using (StreamReader file = new StreamReader(Path.Combine(scriptsFolder, scriptsListFile)))
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
            foreach (string s in Directory.GetFiles(scriptsFolder, "*.py"))
            {
                scriptPaths.Add(s);
            }

            // Create scripts
            List<string> notfound = new List<string>();
            foreach (string s in scriptPaths)
            {
                if (File.Exists(s))
                {
                    CustomScript script = new CustomScript();
                    string[] temp = s.Split('\\');
                    script.Name = temp[temp.Length - 1].Replace(".py", "");
                    script.Path = scriptsFolder;
                    PAScripts.Add(script);
                    Console.WriteLine(s);
                }
                else
                {
                    notfound.Add(s);
                }
            }

            // Some scripts not found
            if (notfound.Count > 0)
            { 
                MessageBoxResult result = MessageBox.Show("The following custom scripts were moved or removed:\n\t-"
                    + string.Join("\n\t-", notfound)
                    + "\n\nRemove from List?","Scripts Not Found", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                { // Remove from scriptList
                    using (StreamWriter sw = new StreamWriter(Path.Combine(scriptsFolder, scriptsListFile), false))
                    {
                        foreach (CustomScript s in PAScripts)
                        {
                            sw.WriteLine(s.Path);
                        }
                    }
                }
            }

            if (PAScripts.Count > 0)
            {
                ScriptListBox.ItemsSource = null;
                ScriptListBox.ItemsSource = PAScripts;
            }

        }

        private void ScriptListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void NewScriptClicked(object sender, RoutedEventArgs e)
        {

        }

        private void RunScriptButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RefreshScriptsButton_Click(object sender, RoutedEventArgs e)
        {
            PreparePostAnalysisTab();
        }
    }
}
