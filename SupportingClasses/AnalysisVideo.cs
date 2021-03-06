﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
