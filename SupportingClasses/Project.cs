﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualGaitLab.SupportingClasses {
    [Serializable]
    public class Project {

        // For Opening an existing project
        public Project() {
            IsTrained = false;
            DisplayIters = "10";
            SaveIters = "5000";
            EndIters = "100000";
            TrainingVideos = new List<TrainingVideo>();
            AnalysisVideos = new List<AnalysisVideo>();
            TrainedWith = new List<string>();
            Name = "";
            DateIdentifier = "";
            Scorer = "";
            ConfigPath = "";
            BodyParts = new List<string>();
            FramesToExtract = "";
            IsGaitOnly = false;
            TrainError = "";
            TestError = "";
            PCutoff = "";
            TrainTime = "";
            TrainingSplit = "0.80";
            GlobalScale = 0.8;
        }

        // For Creating a new project
        public Project(bool gaitOnly, string name, string scorer, List<string> bodyparts) : this()
        {
            IsGaitOnly = gaitOnly;
            Name = name;
            Scorer = scorer;
            BodyParts = bodyparts.Select(x => "- " + x).ToList();   // Add a '-' in front of each body part

            // Date Identifier
            string month = DateTime.Now.Month.ToString();
            string day = DateTime.Now.Day.ToString();
            if (month.Length < 2) month = "0" + month;
            if (day.Length < 2) day = "0" + day;
            DateIdentifier = DateTime.Now.Year.ToString() + "-" + month + "-" + day;
        }

        public List<TrainingVideo> TrainingVideos { get; set; }
        public List<AnalysisVideo> AnalysisVideos { get; set; }
        public List<string> TrainedWith { get; set; }
        public string Name { get; set; }
        public string DateIdentifier { get; set; }
        public string Scorer { get; set; }
        public string ConfigPath { get; set; }
        //public int Frames { get; set; }
        public List<string> BodyParts { get; set; }
        public string SaveIters { get; set; }
        public string DisplayIters { get; set; }
        public string EndIters { get; set; }
        public bool IsTrained { get; set; }
        public string FramesToExtract { get; set; }
        public bool IsGaitOnly { get; set; }
        public string TrainError { get; set; }
        public string TestError { get; set; }
        public string PCutoff { get; set; }
        public string TrainTime { get; set; }
        public string TrainingSplit { get; set; }
        public double GlobalScale { get; set; }
    }
}
