using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace VisualGaitLab.SupportingClasses {
    static class FileSystemUtils {

        public const string CONDA_ACTIVATE_PATH = "\"C:\\Program Files (x86)\\VisualGaitLab\\Miniconda3\\Scripts\\activate.bat\"";


        public static string GetFileName(string inputPath) {
            var withExtension = inputPath.Substring(inputPath.LastIndexOf("\\")+1);
            return withExtension.Substring(0, withExtension.LastIndexOf("."));
        }

        public static string GetFileNameWithExtension(string inputPath) {
            return inputPath.Substring(inputPath.LastIndexOf("\\")+1);
        }

        public static string GetParentFolder(string inputPath) {
            return inputPath.Substring(0, inputPath.LastIndexOf("\\"));
        }

        public static string ExtendPath(string originPath, params string[] list) {
            var finalPath = originPath.TrimEnd('\\');
            for(int i = 0; i<list.Length; i++) {
                finalPath = string.Concat(finalPath, "\\", list[i]);
            }
            return finalPath;
        }

        public static string GetFileExtension(string filePath) {
            return filePath.Substring(filePath.LastIndexOf("."));
        }

        public static bool NameAlreadyInDir(string targetDir, string fileNameWithExtension) {
            if (Directory.Exists(targetDir)) {
                foreach(var current in Directory.EnumerateFiles(targetDir)) {
                    if (current.ToLower().Contains(GetFileName(fileNameWithExtension).ToLower()) && GetFileExtension(current).ToLower().Equals(GetFileExtension(fileNameWithExtension).ToLower())) return true;
                }
            }
            return false;
        }

        public static bool FileNameOk(string filePath) {
            var name = GetFileName(filePath);
            var regex = new Regex("^[a-zA-Z0-9-_]*$");
            if (regex.IsMatch(name) && name.Length < 26) {
                return true;
            }
            return false;
        }

        public static void RenewScript(string fullFileName, List<string> originalScript) { //renew (i.e. create a py file containing specific lines from "PythonScripts" object) the script passed in as param

            if (File.Exists(fullFileName)) {
                String[] rows;
                using (StreamReader sr = new StreamReader(fullFileName)) {
                    rows = Regex.Split(sr.ReadToEnd(), "\r\n");
                }

                using (StreamWriter sw = new StreamWriter(fullFileName)) {
                    for (int i = 0; i < rows.Length; i++) {
                        sw.WriteLine("");
                    }
                }
            }

            using (StreamWriter sw1 = new StreamWriter(fullFileName)) {
                for (int i = 0; i < originalScript.Count; i++) {
                    sw1.WriteLine(originalScript[i]);
                }
            }
        }

        public static void ReplaceStringInFile(String filename, String search, String replace) { //replace string in a text file
            StreamReader sr = new StreamReader(filename);
            String[] rows = Regex.Split(sr.ReadToEnd(), "\r\n");
            sr.Close();

            StreamWriter sw = new StreamWriter(filename);
            for (int i = 0; i < rows.Length; i++) {
                if (rows[i].Contains(search)) {
                    rows[i] = rows[i].Replace(search, replace);
                }
                sw.WriteLine(rows[i]);
            }
            sw.Close();
        }

        public static bool IsFileReady(string filename) { //if the file can be opened for exclusive access it means that the file is no longer locked by another process
            try {
                using (FileStream inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None))
                    return inputStream.Length > 0;
            }
            catch (Exception) {
                return false;
            }
        }

        public static void WaitForFile(string filename) { //this will lock the execution until the file is ready
            while (!IsFileReady(filename)) { }
        }

        public static void MurderPython() // brutally!
        {
            var procs = Process.GetProcesses().Where(pr => pr.ProcessName.Contains("python"));
            foreach (var process in procs) {
                try {
                    process.Kill();
                }
                catch (Exception e) {
                    Console.WriteLine("Error killing Python: " + e);
                }

            }
        }

        public static void RecursiveDelete(DirectoryInfo baseDir) {
            if (!baseDir.Exists)
                return;

            foreach (var dir in baseDir.EnumerateDirectories()) {
                RecursiveDelete(dir);
            }
            File.SetAttributes(baseDir.FullName, FileAttributes.Normal);
            try { baseDir.Delete(true); }
            catch { }
        }
    }
}
