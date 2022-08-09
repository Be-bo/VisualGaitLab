using System;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;

namespace VisualGaitLab.SupportingClasses {
    [Serializable]
    public class AnalysisVideo {
        public string Name { get; set; }
        public string Path { get; set; }
        public string ThumbnailPath { get; set; }
        public bool IsAnalyzed { get; set; }
        public bool IsGaitAnalyzed { get; set; }
        public string GaitAnalyzedImageName { get; set; }
        public string AnalyzedImageName { get; set; }

        public int CalculateFPS()
        {
            return CalculateFPS(Path);
        }

        public static int CalculateFPS(String path)
        {
            int FPS = 0;

            TimeSpan ts; //use Windows API Codepack to determine the length of the currently selected Gait video
            using (var shell = ShellObject.FromParsingName(path))
            {
                ShellProperty<uint?> rateProp = shell.Properties.GetProperty<uint?>("System.Video.FrameRate");
                double? framerate = (rateProp.Value / 1000.0);
                if (framerate != null) FPS = (int)framerate;
            }

            return FPS;
        }
    }
}
