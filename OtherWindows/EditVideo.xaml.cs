using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VisualGaitLab.SupportingClasses;

namespace VisualGaitLab.OtherWindows {
    /// <summary>
    /// Interaction logic for EditVideo.xaml
    /// </summary>
    public partial class EditVideo : Window {





        // MARK: Variables and Constants

        Line SliderLine = new Line();
        List<double> AllCuts = new List<double>();
        string VideoPath;
        string VideoName;
        string EnvName;
        double VideoDuration = 0; //in seconds
        bool SliderInsideSegment = false;
        bool IsAnalysisVideo = false;
        Project CurrentProject;
        string EnvDirectory;
        string Drive;
        bool StopBool = false;









        // MARK: Base Functions

        public EditVideo(string vidPath, string vidName, bool isAnalysisVideo, Project curProj, string envDir, string drive, string envNam) { //passing in the vid's name and location and whether we're cropping a training video or an analysis video
            InitializeComponent();
            VideoPath = vidPath;
            VideoName = vidName;
            IsAnalysisVideo = isAnalysisVideo;
            CurrentProject = curProj;
            Drive = drive;
            EnvDirectory = envDir;
            EnvName = envNam;
            AddSliderLine();
            SetUpVideo();
            SetTimelineImages();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e) {
            AdjustSlider();
        }

        private void SetUpVideo() { //get the incoming video object, load it into MediaElement for playback and pause it
            VideoElement.Source = new Uri(VideoPath, UriKind.Absolute);
            VideoElement.MediaFailed += MediaFailed;
            VideoElement.Loaded += MediaLoaded;
            VideoElement.Pause();
            GetVideoDuration(); //get videos duration
            TimeSpan time = TimeSpan.FromSeconds(0); //go to the beginning of of the video
            VideoElement.Position = time;
        }


        private void MediaFailed(object sender, ExceptionRoutedEventArgs errArgs) {
            Console.WriteLine(errArgs.ErrorException.ToString()+" msg");
            MessageBox.Show("Failed to load the video. Try converting the video to either avi or mp4 (H.264) before importing it into VGL.", "Video Load Unsuccessful", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void MediaLoaded(object sender, RoutedEventArgs args) {
            if(!VideoElement.IsInitialized || VideoElement.RenderSize.Height == 0 || VideoElement.RenderSize.Width == 0) {
                MessageBox.Show("Failed to load the video. Try converting the video to either avi or mp4 (H.264) before importing it into VGL.", "Video Load Unsuccessful", MessageBoxButton.OK, MessageBoxImage.Error);
                //TODO: converting actually might not fix shit
            }
        }



        private void GetVideoDuration() {
            TimeSpan ts; //use Windows API Codepack to determine the length of the video
            using (var shell = ShellObject.FromParsingName(VideoPath)) {
                IShellProperty prop = shell.Properties.System.Media.Duration;
                var t = (ulong)prop.ValueAsObject;
                ts = TimeSpan.FromTicks((long)t);
                VideoDuration = ParseDuration(ts.ToString());
            }
        }

        private double ParseDuration(String stringDuration) { //parse the duration from the format (HH:MM:SS) that the API Code Pack returns to a floating point number in seconds
            string hours = stringDuration.Substring(0, 2);
            string minutes = stringDuration.Substring((stringDuration.IndexOf(":") + 1), 2);
            string seconds = stringDuration.Substring(stringDuration.LastIndexOf(":") + 1, (stringDuration.Length - (stringDuration.LastIndexOf(":") + 1)));
            double totalInSeconds = double.Parse(hours) * 3600 + double.Parse(minutes) * 60 + double.Parse(seconds);
            return totalInSeconds;
        }

        private void SetTimelineImages() { //grab the video that's being editted and extract 15 images for timeline thumbnails out of it
            using (var engine = new Engine()) {
                var vid = new MediaFile { Filename = VideoPath };
                var cacheLocation = VideoPath.Substring(0, VideoPath.LastIndexOf("\\")) + "\\cache";
                Directory.CreateDirectory(cacheLocation);

                engine.GetMetadata(vid);
                var segment = VideoDuration / 15; //divide the duration by 15

                for (int i = 0; i < 15; i++) { //and keep capturing the thumbnail at the correct location (Eg. thumbnail at loc 3 is 3*segment where segment is one 15th of the video)
                    var options = new ConversionOptions { Seek = TimeSpan.FromSeconds(i * segment) };
                    var outputFile = new MediaFile { Filename = string.Format("{0}\\thumb{1}.jpg", cacheLocation, i) };
                    engine.GetThumbnail(vid, outputFile, options);
                }

                TimeLineImage1.Source = new BitmapImage(new Uri(cacheLocation + "\\thumb0.jpg", UriKind.Absolute));
                TimeLineImage2.Source = new BitmapImage(new Uri(cacheLocation + "\\thumb1.jpg", UriKind.Absolute));
                TimeLineImage3.Source = new BitmapImage(new Uri(cacheLocation + "\\thumb2.jpg", UriKind.Absolute));
                TimeLineImage4.Source = new BitmapImage(new Uri(cacheLocation + "\\thumb3.jpg", UriKind.Absolute));
                TimeLineImage5.Source = new BitmapImage(new Uri(cacheLocation + "\\thumb4.jpg", UriKind.Absolute));
                TimeLineImage6.Source = new BitmapImage(new Uri(cacheLocation + "\\thumb5.jpg", UriKind.Absolute));
                TimeLineImage7.Source = new BitmapImage(new Uri(cacheLocation + "\\thumb6.jpg", UriKind.Absolute));
                TimeLineImage8.Source = new BitmapImage(new Uri(cacheLocation + "\\thumb7.jpg", UriKind.Absolute));
                TimeLineImage9.Source = new BitmapImage(new Uri(cacheLocation + "\\thumb8.jpg", UriKind.Absolute));
                TimeLineImage10.Source = new BitmapImage(new Uri(cacheLocation + "\\thumb9.jpg", UriKind.Absolute));
                TimeLineImage11.Source = new BitmapImage(new Uri(cacheLocation + "\\thumb10.jpg", UriKind.Absolute));
                TimeLineImage12.Source = new BitmapImage(new Uri(cacheLocation + "\\thumb11.jpg", UriKind.Absolute));
                TimeLineImage13.Source = new BitmapImage(new Uri(cacheLocation + "\\thumb12.jpg", UriKind.Absolute));
                TimeLineImage14.Source = new BitmapImage(new Uri(cacheLocation + "\\thumb13.jpg", UriKind.Absolute));
                TimeLineImage15.Source = new BitmapImage(new Uri(cacheLocation + "\\thumb14.jpg", UriKind.Absolute));
            }
        }












        // MARK: Slider Related Methods

        private void PositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (TimeLineSlider != null) {

                TimeSpan time = TimeSpan.FromSeconds((TimeLineSlider.Value / TimeLineSlider.Maximum) * VideoDuration); //move to the correct position in the video on slider value change
                VideoElement.Position = time;

                if (TimeLineCanvas.Children.Contains(SliderLine)) { //if the slider line already exists just edit it's location on the timeline
                    //int index = TimeLineCanvas.Children.IndexOf(SliderLine);
                    ((Line)TimeLineCanvas.Children[0]).X1 = TimeLineSlider.Value;
                    ((Line)TimeLineCanvas.Children[0]).X2 = TimeLineSlider.Value;
                }
                else {
                    AddSliderLine(); //if it hasn't been added yet, add it
                }

                bool previousInsideSegment = SliderInsideSegment; //we use these to determine if the current slider change resulted in the slider ending up in one of the cut segments

                for (int i = 0; i + 1 < AllCuts.Count; i = i + 2) {
                    if (TimeLineSlider.Value <= Math.Max(AllCuts[i], AllCuts[i + 1]) && TimeLineSlider.Value >= Math.Min(AllCuts[i], AllCuts[i + 1])) { //if it's inside a segment change the bool var
                        SliderInsideSegment = true;
                        break; //and break
                    }
                    if (i + 3 >= AllCuts.Count) SliderInsideSegment = false; //in case we're at the end of the loop and haven't broken it -> we're outside of the segment
                }

                if (previousInsideSegment != SliderInsideSegment) { //and here we can see if the bool var switched when going through the loop above
                    if (SliderInsideSegment) { //if it flipped from false to true color the canvas
                        System.Windows.Shapes.Rectangle rect;
                        rect = new System.Windows.Shapes.Rectangle();
                        rect.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0d89ff"));
                        rect.Width = CurrentFrameCanvas.ActualWidth;
                        rect.Height = CurrentFrameCanvas.ActualHeight;
                        rect.Opacity = 0.5;
                        Canvas.SetLeft(rect, 0);
                        Canvas.SetTop(rect, 0);
                        CurrentFrameCanvas.Children.Add(rect);
                    }
                    else { //if it flipped from true to false remove the opaque rectangle
                        CurrentFrameCanvas.Children.RemoveAt(0);
                    }
                }
            }
        }

        private void AddSliderLine() { //add the default line that shows the user where they are on the timeline
            SliderLine.Stroke = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0d89ff"));
            SliderLine.X1 = TimeLineSlider.Value;
            SliderLine.X2 = TimeLineSlider.Value;
            SliderLine.Y1 = 0;
            SliderLine.Y2 = 50;
            SliderLine.StrokeThickness = 1;
            SliderLine.Tag = "SliderLine";
            TimeLineCanvas.Children.Add(SliderLine);
        }

        private void AdjustSlider() { //set the slider's value to make sure the ui elems are always in the right places (we use this when the window gets resized and when the window's launched)
            if (TimeLineSlider != null && this != null) {
                Point timeLineOrigin = TimeLineCanvas.TransformToAncestor(this).Transform(new Point(0, 0));
                TimeLineSlider.Minimum = timeLineOrigin.X - 5;
                TimeLineSlider.Maximum = timeLineOrigin.X + TimeLineCanvas.ActualWidth - 5;
            }
        }












        // MARK: Button Methods

        private void CutButton_Click(object sender, RoutedEventArgs e) {

            double attemptedCut = TimeLineSlider.Value; //remember the cut position
            bool violationPresent = false;

            if (AllCuts.Count % 2 == 0) { //if it's a solo line (i.e. first boundary of a new segment) just make a line

                for (int i = 0; i + 1 < AllCuts.Count; i = i + 2) {
                    if (attemptedCut <= Math.Max(AllCuts[i], AllCuts[i + 1]) && attemptedCut >= Math.Min(AllCuts[i], AllCuts[i + 1])) { //if the attempted cut is inside of any of the existing segments abort
                        violationPresent = true;
                        break;
                    }
                }

                if (!violationPresent) { //if there's no violation add the line
                    Line line = new Line();
                    line.Stroke = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0d89ff"));
                    line.X1 = TimeLineSlider.Value;
                    line.X2 = TimeLineSlider.Value;
                    line.Y1 = 0;
                    line.Y2 = TimeLineCanvas.ActualHeight;
                    line.Opacity = 0.5;
                    line.StrokeThickness = 2;
                    TimeLineCanvas.Children.Add(line);
                    AllCuts.Add(attemptedCut); //add the cut to our cut positions
                }

            }
            else { //if another solo line already exists (i.e. we're completing a segment) make a rectangle between the two lines instead

                if (attemptedCut > AllCuts[AllCuts.Count - 1]) {

                    for (int i = 0; i + 1 < AllCuts.Count - 1; i = i + 2) {
                        if (attemptedCut <= Math.Max(AllCuts[i], AllCuts[i + 1]) && attemptedCut >= Math.Min(AllCuts[i], AllCuts[i + 1])) { //if the attempted cut is inside of any of the existing segments abort
                            violationPresent = true;
                            break;
                        }
                        if (Math.Max(attemptedCut, AllCuts[AllCuts.Count - 1]) >= Math.Max(AllCuts[i], AllCuts[i + 1]) && Math.Min(attemptedCut, AllCuts[AllCuts.Count - 1]) <= Math.Min(AllCuts[i], AllCuts[i + 1])) {
                            violationPresent = true; //likewise if the new potential segment is about to encompass another segment abort the operation
                            break;
                        }
                    }

                    if (!violationPresent) { //if there's no violation add the rectangle
                        System.Windows.Shapes.Rectangle rect;
                        rect = new System.Windows.Shapes.Rectangle();
                        rect.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0d89ff"));
                        rect.Width = Math.Abs(AllCuts[AllCuts.Count - 1] - attemptedCut);
                        rect.Height = TimeLineCanvas.ActualHeight;
                        rect.Opacity = 0.5;
                        if (AllCuts[AllCuts.Count - 1] < attemptedCut) { //if the end of the segment is being added after the previous line then the previous line's position is where we start drawing
                            Canvas.SetLeft(rect, AllCuts[AllCuts.Count - 1]);
                            Canvas.SetTop(rect, 0);
                        }
                        else { //else start drawing at the position of the line that was just added
                            Canvas.SetLeft(rect, attemptedCut);
                            Canvas.SetTop(rect, 0);
                        }
                        TimeLineCanvas.Children.Add(rect);
                        AllCuts.Add(attemptedCut); //add the cut to our cut positions
                    }
                }
            }

            if (AllCuts.Count > 1) DoneButton.IsEnabled = true;
            else DoneButton.IsEnabled = false;
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e) { //remove the latest addition to the timeline canvas (doesn't matter wh it's a rectangle or a line)
            if (TimeLineCanvas.Children.Count > 1) { //slider line is the first elem - never remove that one
                TimeLineCanvas.Children.RemoveAt(TimeLineCanvas.Children.Count - 1);
                AllCuts.RemoveAt(AllCuts.Count - 1);
            }

            if (AllCuts.Count < 2) DoneButton.IsEnabled = false;
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
        }














        // MARK: Actual Video Cropping

        private void DoneButton_Click(object sender, RoutedEventArgs e) {
            BarInteraction();
            List<double> allCuts = AllCuts;
            double vidDur = VideoDuration;
            double max = TimeLineSlider.Maximum;
            string vidPath = VideoPath;
            string vidName = VideoName;
            Task.Run(() => CreateVideos(allCuts, vidDur, max, vidPath, vidName)); //make sure the video cropping stuff takes place on a different thread so that the ui doesn't freeze
        }

        private void CreateVideos(List<double> AllCuts, double VideoDuration, double SliderMax, string vidPath, string vidName) {
            if (AllCuts.Count >= 2) { //need at least 2, i.e. one segment
                List<double> timeCutsInVideo = new List<double>();
                for (int i = 0; i < AllCuts.Count; i++) {
                    timeCutsInVideo.Add((AllCuts[i] / SliderMax) * VideoDuration); //convert slider values to times within the video
                }
                if (timeCutsInVideo.Count % 2 != 0) { //if they started a segment but didn't complete it, disregard
                    timeCutsInVideo.RemoveAt(timeCutsInVideo.Count - 1);
                }


                string vidsFolder;
                if (IsAnalysisVideo) vidsFolder = vidPath.Substring(0, vidPath.Substring(0, vidPath.LastIndexOf("\\")).LastIndexOf("\\")); //grab the folder with analyzed videos
                else vidsFolder = vidPath.Substring(0, vidPath.LastIndexOf("\\")); //grab the training videos folder

                var inputFile = new MediaFile { Filename = vidPath }; //grab the original video and set the format extension
                var outputExtension = ".mp4";


                for (int i = 0; i + 1 < timeCutsInVideo.Count; i = i + 2) { //for each segment use time in seconds to tell the Media Engine where to cut
                    string outputVidFolder;
                    string outputVidPath;

                    if (IsAnalysisVideo) { //video is an analysis video => an entire new folder has to be created inside of "analyzed-videos"
                        outputVidFolder = vidsFolder + "\\" + vidName + "_" + i / 2;
                        Directory.CreateDirectory(outputVidFolder);
                        outputVidPath = outputVidFolder + "\\" + vidName + "_" + i / 2;
                    }
                    else {
                        outputVidFolder = vidsFolder; //video is a training video => no extra folders need to be created
                        outputVidPath = outputVidFolder + "\\" + vidName + "_" + i / 2;
                    }


                    var engine = new Engine();
                    engine.GetMetadata(inputFile);
                    var options = new ConversionOptions();
                    options.VideoBitRate = inputFile.Metadata.VideoData.BitRateKbs;

                    options.CutMedia(TimeSpan.FromSeconds(timeCutsInVideo[i]), TimeSpan.FromSeconds(timeCutsInVideo[i + 1]) - TimeSpan.FromSeconds(timeCutsInVideo[i]));
                    engine.Convert(inputFile, new MediaFile(outputVidPath + outputExtension), options); //actually makes the new video

                    if (!IsAnalysisVideo) {
                        String thumbPath = outputVidFolder + "\\" + vidName + "_" + (i / 2).ToString() + ".png";
                        engine.GetThumbnail(inputFile, new MediaFile(thumbPath), options); //and create a thumbnail for the training video
                        engine.Dispose();
                        StopBool = true;
                        string labeledDataFolder = outputVidFolder.Substring(0, outputVidFolder.LastIndexOf("\\")) + "\\labeled-data";
                        string labeledDataPath = labeledDataFolder + "\\" + vidName + "_" + (i / 2);
                        Directory.CreateDirectory(labeledDataPath);
                        AddTrainingVideo(outputVidPath + outputExtension, vidName + "_" + (i / 2), labeledDataPath, timeCutsInVideo.Count, i, thumbPath);
                        while (StopBool) { //this will force the loop to wait for the above method call to finish
                            //horrible practice, I know but I don't have enough time at this point
                        }
                    }
                    else {
                        engine.GetThumbnail(inputFile, new MediaFile(outputVidFolder + "\\thumbnail.png"), options); //and create a thumbnail for the analysis video
                        engine.Dispose();
                        string settingsPath = outputVidFolder + "\\settings.txt"; //create default settings file for the new analysis video
                        StreamWriter sw1 = new StreamWriter(settingsPath);
                        sw1.WriteLine("dotsize: " + "5");
                        sw1.Close();
                        this.Dispatcher.Invoke(() => {
                            if (i + 3 >= timeCutsInVideo.Count) { //if we're at the last video notify the ui thread that we're done by enabling interaction and terminating the window
                                EnableInteraction();
                                this.DialogResult = true;
                            }
                        });
                    }
                }
            }
        }








        // MARK: Adding the Cut Videos to the Project

        private void AddTrainingVideo(string vidPath, string vidName, string labeledDataPath, int count, int i, String thumbPath) { //it's not enough for a newly added training video to be simply added to the folder, we need to add it to the config file through DeepLabCut
            TrainingVideo newVideo = new TrainingVideo();
            newVideo.Name = vidName;
            string hypotheticalPath = labeledDataPath;
            if ((!Directory.Exists(vidPath)) || (Directory.Exists(vidPath))) {
                if (Directory.Exists(hypotheticalPath) && Directory.EnumerateFileSystemEntries(hypotheticalPath).Count() == 1) File.Delete(Directory.EnumerateFiles(hypotheticalPath).First());
                newVideo.Path = vidPath;
                newVideo.ThumbnailPath = thumbPath;
                newVideo.FramesExtracted = false;
                newVideo.ExtractedImageName = "cross.png";
                newVideo.FramesLabeled = false;
                newVideo.LabeledImageName = "cross.png";
                PythonScripts allScripts = new PythonScripts();

                string filePath = EnvDirectory + "\\vdlc_add_video.py";
                FileSystemUtils.MurderPython();
                FileSystemUtils.RenewScript(filePath, allScripts.AddVideo);
                FileSystemUtils.ReplaceStringInFile(filePath, "copy_videos_identifier", "True");
                FileSystemUtils.ReplaceStringInFile(filePath, "config_path_identifier", CurrentProject.ConfigPath);
                FileSystemUtils.ReplaceStringInFile(filePath, "video_path_identifier", vidPath);
                Process p = new Process();
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = "cmd.exe";
                info.RedirectStandardInput = true;
                info.UseShellExecute = false;
                info.Verb = "runas";
                info.CreateNoWindow = true;

                p.EnableRaisingEvents = true;
                p.Exited += (sender1, e1) => {
                    this.Dispatcher.Invoke(() => {
                        StopBool = false; //continue running the loop
                        if (i + 3 >= count) { //if we're at the last video notify the ui thread that we're done by enabling interaction and terminating the window
                            EnableInteraction();
                            this.DialogResult = true;
                        }
                    });
                };

                p.StartInfo = info;
                p.Start();

                using (StreamWriter sw = p.StandardInput) {
                    if (sw.BaseStream.CanWrite) {
                        sw.WriteLine(Drive);
                        sw.WriteLine("cd " + EnvDirectory);
                        sw.WriteLine(FileSystemUtils.CONDA_ACTIVATE_PATH);
                        sw.WriteLine("conda activate " + EnvName);
                        sw.WriteLine("ipython vdlc_add_video.py");
                    }
                }
            }
        }

        private void BarInteraction() {
            PrimaryPanel.IsEnabled = false;
            PrimaryPanel.Opacity = 0.3;
            PRing.IsActive = true;
        }

        private void EnableInteraction() {
            this.Dispatcher.Invoke(() => {
                PrimaryPanel.IsEnabled = true;
                PrimaryPanel.Opacity = 1;
                PRing.IsActive = false;
            });
        }
    }
}
