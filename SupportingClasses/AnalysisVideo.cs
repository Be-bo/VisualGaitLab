using System;

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
    }
}
