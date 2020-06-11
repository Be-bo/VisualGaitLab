using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualGaitLab.SupportingClasses {
    public class PythonScripts //pre-prepared scripts, we use them to interact with DeepLabCut (each script is saved as a string array and when needed, it is created and run through a CMD session)
    {
        public List<string> CreateProject { get; set; }
        public List<string> ExtractFrames { get; set; }
        public List<string> LabelFrames { get; set; }
        public List<string> AddVideo { get; set; }
        public List<string> CreateDataset { get; set; }
        public List<string> TrainNetwork { get; set; }
        public List<string> CreateLabeledVideo { get; set; }
        public List<string> AnalyzeVideo { get; set; }
        public List<string> ExtractAll { get; set; }

        public List<string> EvalNetwork { get; set; }
        public List<string> GetAreas { get; set; }

        public PythonScripts() {
            CreateProject = new List<string>();
            CreateProject.Add("import deeplabcut");
            CreateProject.Add("deeplabcut.create_new_project('project_name_identifier', 'scorer_identifier', [], working_directory='working_directory_identifier',copy_videos=copy_videos_identifier)");

            ExtractFrames = new List<string>();
            ExtractFrames.Add("import deeplabcut");
            ExtractFrames.Add("config_path = r'config_path_identifier'");
            ExtractFrames.Add("deeplabcut.extract_frames(config_path, 'automatic', videoPath=r'video_path_identifier')");

            LabelFrames = new List<string>();
            LabelFrames.Add("import deeplabcut");
            LabelFrames.Add("config_path = r'config_path_identifier'");
            LabelFrames.Add("deeplabcut.label_frames(config_path, r'video_path_identifier')");

            AddVideo = new List<string>();
            AddVideo.Add("import deeplabcut");
            AddVideo.Add("config_path = r'config_path_identifier'");
            AddVideo.Add("deeplabcut.add_new_videos(config_path,[r'video_path_identifier'],copy_videos=copy_videos_identifier)");

            CreateDataset = new List<string>();
            CreateDataset.Add("import deeplabcut");
            CreateDataset.Add("config_path = r'config_path_identifier'");
            CreateDataset.Add("deeplabcut.create_training_dataset(config_path)");

            TrainNetwork = new List<string>();
            TrainNetwork.Add("import deeplabcut");
            TrainNetwork.Add("config_path = r'config_path_identifier'");
            TrainNetwork.Add("deeplabcut.train_network(config_path)");

            EvalNetwork = new List<string>();
            EvalNetwork.Add("import deeplabcut");
            EvalNetwork.Add("config_path = r'config_path_identifier'");
            EvalNetwork.Add("deeplabcut.evaluate_network(config_path, plotting=True)");

            CreateLabeledVideo = new List<string>();
            CreateLabeledVideo.Add("import deeplabcut");
            CreateLabeledVideo.Add("config_path = r'config_path_identifier'");
            CreateLabeledVideo.Add("deeplabcut.create_labeled_video(config_path,[r'video_path_identifier'],save_frames=True)");

            AnalyzeVideo = new List<string>();
            AnalyzeVideo.Add("import deeplabcut");
            AnalyzeVideo.Add("config_path = r'config_path_identifier'");
            AnalyzeVideo.Add("deeplabcut.analyze_videos(config_path,[r'video_path_identifier'],save_as_csv=True)");

            ExtractAll = new List<string>();
            ExtractAll.Add("import deeplabcut");
            ExtractAll.Add("config_path = r'config_path_identifier'");
            ExtractAll.Add("deeplabcut.extract_frames(config_path)");

        }
    }
}
