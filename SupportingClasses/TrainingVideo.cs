using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualGaitLab.SupportingClasses {
    [Serializable]
    public class TrainingVideo {
        public string Name { get; set; }
        public int Frames { get; set; }
        public string Path { get; set; }
        public bool FramesLabeled { get; set; }
        public bool FramesExtracted { get; set; }
        public string ThumbnailPath { get; set; }
        public string ExtractedImageName { get; set; }
        public string LabeledImageName { get; set; }
    }
}
