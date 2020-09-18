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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using VisualGaitLab.SupportingClasses;

namespace VisualGaitLab.OtherWindows {
    /// <summary>
    /// Interaction logic for ImportWindow.xaml
    /// </summary>
    public partial class ImportWindow : Window {


        


        // MARK: Variables

        Line SliderLine = new Line();
        List<double> AllCuts = new List<double>();
        MediaFile InputFile = new MediaFile();
        MediaFile OutputFile = new MediaFile();
        DispatcherTimer DispatchTimer = new DispatcherTimer();
        Point MouseDownPos; // the point where the mouse button was clicked down.


        string VideoCachePath = "";
        string CachePath = "";
        string EnvDirectory = "";
        string EnvName = "";
        string Drive = "";
        string ProjectPath = "";
        string InitialFilePath = "";
        string ProgramFolder = "";

        double VideoDuration = 0; //in seconds
        int ConversionCounter = 1;
        int TimerType = 0;
        int VideoBitrate = 50000; //in kbps
        bool VideoNotMp4OrAvi = false;
        int VideoResolution = (int)VideoSize.Default;
        
        bool SliderInsideSegment = false;
        bool IsAnalysisVideo = false;
        bool HintAndBoxInitialized = false;
        bool CroppingFinished = false;
        bool StopBool = false;
        bool MouseDwn = false; // set to 'true' when mouse is held down.
        bool MirroringFinished = false;













        // MARK: Setup Functions

        public ImportWindow(string inputFileName, string configPath, bool isAnalysisVid, string envDir, string envNam, string driv, string progFolder) {
            InitializeComponent();
            ProjectPath = FileSystemUtils.GetParentFolder(configPath);
            CachePath = FileSystemUtils.ExtendPath(ProjectPath, "cache");
            FileSystemUtils.RecursiveDelete(new DirectoryInfo(CachePath)); //clear the cache before doing anything
            InitialFilePath = inputFileName;
            IsAnalysisVideo = isAnalysisVid;
            EnvDirectory = envDir;
            EnvName = envNam;
            Drive = driv;
            ProgramFolder = progFolder;
            StartCheckingForCompletion(1); //have to wait for InitializeComponent to finish (to show the loading wheel right away)
        }

        private void StartCheckingForCompletion(int timerType) {
            TimerType = timerType;
            DispatchTimer = new DispatcherTimer();
            DispatchTimer.Tick += new EventHandler(TimedCheck); //declare a timer that checks if it's time to start the next conversion every 5 seconds
            DispatchTimer.Interval = new TimeSpan(0, 0, 3); //initially we want the check to happen right away
            DispatchTimer.Start();
        }

        private void TimedCheck(object sender, EventArgs e) {
            if (TimerType == 0) { //type 1, ALL CONVERSIONS (when fixing a video) COMPLETE
                if (ConversionCounter > 2) { //check if we're actually done here
                    DispatchTimer.Stop();
                    if (VideoNotMp4OrAvi) VideoCachePath = InputFile.Filename;
                    SetUpVideo();
                }
            }
            else if (TimerType == 1) { //type 2, InitializeComponent DONE
                if (ImportPrimaryPanel != null) { //done loading, primary panel elements have been rendered
                    DispatchTimer.Stop();
                    PrepareBitrate(); //start the UX with letting the user choose the conversion bitrate
                }
            }
            else if (TimerType == 2) { //type 3, VIDEO LOADING PROGRESS
                if (ImportVideoElement.BufferingProgress >= 1) {
                    DispatchTimer.Stop();
                    AdjustWindowSize();
                    ResetSelectionBox();
                    EnableInteraction();

                    /*
                    if (!ImportVideoElement.IsInitialized || ImportVideoElement.RenderSize.Height == 0 || ImportVideoElement.RenderSize.Width == 0) { //the video sometimes loads but doesn't get displayed
                        Console.WriteLine("video not good");
                        StartConversion(); //convert the video to a format that will work
                    }
                    else {
                        Console.WriteLine("ALL DONE, SUCCESS");
                        EnableInteraction(); //finally, when everything is loaded, allow the user to interact with the window
                    }*/
                }
            }else if (TimerType == 3) { //cropping done, now either mirror and split, or just split
                if (CroppingFinished) {
                    DispatchTimer.Stop();
                    if (MessageBox.Show("Do you want to mirror the video horizontally? (For all gait analysis videos, the mouse has to be facing the right side of the video.)", "Mirror Video?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                        StartCheckingForCompletion(4); //mirror, then split
                        Thread addVideoThread = new Thread(MirrorVideo);
                        addVideoThread.IsBackground = true;
                        addVideoThread.Start();
                    }
                    else { //split right away
                        Thread splitThread = new Thread(SplitVideo);
                        splitThread.IsBackground = true;
                        splitThread.Start();
                    }
                }
            }else if (TimerType == 4) {
                if (MirroringFinished) {
                    DispatchTimer.Stop();
                    Thread splitThread = new Thread(SplitVideo);
                    splitThread.IsBackground = true;
                    splitThread.Start();
                }
            }
        }

        private void PrepareBitrate() { //gets the bitrate of the video and reduces it based on user input from "BitrateWindow"
            var inputFile = new MediaFile { Filename = InitialFilePath };
            using (var engine = new Engine()) {
                engine.GetMetadata(inputFile);
            }
            BitrateWindow bitrateWindow = new BitrateWindow();
            if (bitrateWindow.ShowDialog() == true) {
                if(inputFile.Metadata != null && inputFile.Metadata.VideoData != null && inputFile.Metadata.VideoData.BitRateKbs != null) VideoBitrate = (int)inputFile.Metadata.VideoData.BitRateKbs;
                if ((bool)bitrateWindow.Import1080RadioButton.IsChecked) VideoResolution = (int)VideoSize.Hd1080; //get the correct
                else if ((bool)bitrateWindow.Import720RadioButton.IsChecked) VideoResolution = (int)VideoSize.Hd720;
                else if ((bool)bitrateWindow.Import480RadioButton.IsChecked) VideoResolution = (int)VideoSize.Hd480;
                else if ((bool)bitrateWindow.Import360RadioButton.IsChecked) VideoResolution = (int)VideoSize.Nhd;
                else if ((bool)bitrateWindow.ImportNothingRadioButton.IsChecked) VideoResolution = (int)VideoSize.Default;

                if (!Directory.Exists(CachePath)) Directory.CreateDirectory(CachePath);
                string outputPath = FileSystemUtils.ExtendPath(CachePath, FileSystemUtils.GetFileName(InitialFilePath) + FileSystemUtils.GetFileExtension(InitialFilePath).ToLower());
                CopyVideo(InitialFilePath, outputPath);
                VideoCachePath = outputPath;
                StartCheckingForCompletion(0); //start listening to when thread done converting the video
                Thread conversionThread = new Thread(StartConversion); //start the conversion on a separate thread so the UI doesn't freeze up
                conversionThread.IsBackground = true;
                conversionThread.Start();
            }
        }

        private void SetUpVideo() { //display the video in UI
            AddSliderLine();

            ImportVideoElement.Source = new Uri(VideoCachePath, UriKind.Absolute);
            ImportVideoElement.MediaFailed += MediaFailedEvent;
            StartCheckingForCompletion(2); //start listening to when the video is done loading
            ImportVideoElement.Pause();
            GetVideoDuration(); //get videos duration
            TimeSpan time = TimeSpan.FromSeconds(0); //go to the beginning of of the video
            ImportVideoElement.Position = time;

            SetTimelineImages();
        }

        private void MediaFailedEvent(object sender, ExceptionRoutedEventArgs args) { //failed to load video
            MessageBox.Show("Failed to display video. Try to convert the video to a different format, or contact support at rfiker@ucalgary.ca.", "Failed to Load Video", MessageBoxButton.OK, MessageBoxImage.Error);
            this.Close();
        }

        private void GetVideoDuration() {
            TimeSpan ts; //use Windows API Codepack to determine the length of the video
            using (var shell = ShellObject.FromParsingName(VideoCachePath)) {
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
                var vid = new MediaFile { Filename = VideoCachePath };
                var cacheLocation = CachePath;
                Directory.CreateDirectory(cacheLocation);

                engine.GetMetadata(vid);
                var segment = VideoDuration / 15; //divide the duration by 15
                Random r = new Random(); //we use random ints because if the user adds more than one vid in a session the old thumbnails can't be deleted and are out of date on the timeline
                var imageNames = new List<string>();

                for (int i = 0; i < 15; i++) { //and keep capturing the thumbnail at the correct location (Eg. thumbnail at loc 3 is 3*segment where segment is one 15th of the video)
                    var options = new ConversionOptions { Seek = TimeSpan.FromSeconds(i * segment) };
                    var thumbName = r.Next(0, 10000000).ToString() + ".jpg";
                    var outputFile = new MediaFile { Filename = cacheLocation + "\\" + thumbName };
                    imageNames.Add(thumbName);
                    engine.GetThumbnail(vid, outputFile, options);
                }

                ImportTimeLineImage1.Source = new BitmapImage(new Uri(cacheLocation + "\\" + imageNames[0], UriKind.Absolute));
                ImportTimeLineImage2.Source = new BitmapImage(new Uri(cacheLocation + "\\" + imageNames[1], UriKind.Absolute));
                ImportTimeLineImage3.Source = new BitmapImage(new Uri(cacheLocation + "\\" + imageNames[2], UriKind.Absolute));
                ImportTimeLineImage4.Source = new BitmapImage(new Uri(cacheLocation + "\\" + imageNames[3], UriKind.Absolute));
                ImportTimeLineImage5.Source = new BitmapImage(new Uri(cacheLocation + "\\" + imageNames[4], UriKind.Absolute));
                ImportTimeLineImage6.Source = new BitmapImage(new Uri(cacheLocation + "\\" + imageNames[5], UriKind.Absolute));
                ImportTimeLineImage7.Source = new BitmapImage(new Uri(cacheLocation + "\\" + imageNames[6], UriKind.Absolute));
                ImportTimeLineImage8.Source = new BitmapImage(new Uri(cacheLocation + "\\" + imageNames[7], UriKind.Absolute));
                ImportTimeLineImage9.Source = new BitmapImage(new Uri(cacheLocation + "\\" + imageNames[8], UriKind.Absolute));
                ImportTimeLineImage10.Source = new BitmapImage(new Uri(cacheLocation + "\\"+ imageNames[9], UriKind.Absolute));
                ImportTimeLineImage11.Source = new BitmapImage(new Uri(cacheLocation + "\\"+ imageNames[10], UriKind.Absolute));
                ImportTimeLineImage12.Source = new BitmapImage(new Uri(cacheLocation + "\\"+ imageNames[11], UriKind.Absolute));
                ImportTimeLineImage13.Source = new BitmapImage(new Uri(cacheLocation + "\\"+ imageNames[12], UriKind.Absolute));
                ImportTimeLineImage14.Source = new BitmapImage(new Uri(cacheLocation + "\\"+ imageNames[13], UriKind.Absolute));
                ImportTimeLineImage15.Source = new BitmapImage(new Uri(cacheLocation + "\\"+ imageNames[14], UriKind.Absolute));
            }
        }



















        // MARK: Video Conversion (to change bitrate and make sure the video is going to work within VGL)

        private void StartConversion() {
            if (VideoCachePath.ToLower().EndsWith(".avi")) { //convert to mp4 and back to avi
                InputFile = new MediaFile { Filename = VideoCachePath };
                string outputFileName = FileSystemUtils.ExtendPath(CachePath, FileSystemUtils.GetFileName(VideoCachePath) + ".mp4");
                OutputFile = new MediaFile { Filename = outputFileName };
            }
            else if (VideoCachePath.ToLower().EndsWith(".mp4")) { //convert to avi and back to mp4
                InputFile = new MediaFile { Filename = VideoCachePath };
                string outputFileName = FileSystemUtils.ExtendPath(CachePath, FileSystemUtils.GetFileName(VideoCachePath) + ".avi");
                OutputFile = new MediaFile { Filename = outputFileName };
            }
            else { //convert to avi, then to mp4, then to avi
                ConversionCounter = 0;
                VideoNotMp4OrAvi = true;
                string inputFileName = FileSystemUtils.ExtendPath(CachePath, FileSystemUtils.GetFileName(VideoCachePath) + ".avi"); //we'll treat the video as if it came in as .avi (to which we will convert first)
                InputFile = new MediaFile { Filename = inputFileName };
                OutputFile = new MediaFile { Filename = FileSystemUtils.ExtendPath(CachePath, FileSystemUtils.GetFileName(VideoCachePath) + ".mp4") }; //and then will convert to mp4 and back
            }
            ConvertVideo();
        }
        private void ConvertVideo() {
            if (OutputFile.Filename != null && InputFile.Filename != null) {
                using (var engine = new Engine()) {
                    engine.ConversionCompleteEvent += ConversionCompleteEvent;
                    engine.ConvertProgressEvent += ProgressEvent;
                    var conversionOptions = new ConversionOptions {
                        VideoBitRate = VideoBitrate,
                        VideoSize = (VideoSize)VideoResolution
                    };

                    if (VideoNotMp4OrAvi) { // if original vid not mp4 or avi, gotta convert it from its format to .avi (else clause of StartConversion)
                        engine.Convert(new MediaFile { Filename = VideoCachePath }, InputFile, conversionOptions);
                    } //and then it's treated as if it was imported as a .avi video
                    engine.Convert(InputFile, OutputFile, conversionOptions); //convert to the other format (mp4->avi & avi->mp4)
                    engine.Convert(OutputFile, InputFile, conversionOptions); //convert back to the original format (because this way of conversion fixes any problems with the original video so now it can be displayed using MediaToolkit)
                }
            }
            else {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    MessageBox.Show("Failed to load video.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                }));

                this.Dispatcher.Invoke(() => {
                    MessageBox.Show("Failed to load video.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                });

            }
        }

        private void ProgressEvent(object sender, ConvertProgressEventArgs args) {
            Console.WriteLine(args.ProcessedDuration);
        }


        private void ConversionCompleteEvent(object sender, ConversionCompleteEventArgs args) {
            ConversionCounter++;
        }
















        //MARK: UI Methods

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e) {
            ImportSelectWindowHint.Visibility = Visibility.Collapsed;
            ImportVideoElement.Opacity = 1;
            MouseDwn = true;
            MouseDownPos = e.GetPosition(ImportVideoGrid);
            ImportVideoGrid.CaptureMouse();

            Canvas.SetLeft(ImportSelectionBox, MouseDownPos.X); //initial placement of the drag selection box
            Canvas.SetTop(ImportSelectionBox, MouseDownPos.Y);
            ImportSelectionBox.Width = 0;
            ImportSelectionBox.Height = 0;

            ImportSelectionBox.Visibility = Visibility.Visible; //make the drag selection box visible during selection
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e) {
            HintAndBoxInitialized = true;
            MouseDwn = false; //release the mouse capture and stop tracking it
            ImportVideoGrid.ReleaseMouseCapture();
            //selectionBox.Visibility = Visibility.Collapsed; //stop showing drag selection
            Point mouseUpPos = e.GetPosition(ImportVideoGrid);
            if (mouseUpPos.X > ImportVideoGrid.ActualWidth || mouseUpPos.Y > ImportVideoGrid.ActualHeight || MouseDownPos.X < 0 || MouseDownPos.Y < 0
                || ImportSelectionBox.ActualWidth < 10 || ImportSelectionBox.ActualHeight < 10) {
                ResetSelectionBox();
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e) {
            if (MouseDwn) { //when the mouse is held down, reposition the drag selection box.
                Point mousePos = e.GetPosition(ImportVideoGrid);

                if (MouseDownPos.X < mousePos.X) {
                    Canvas.SetLeft(ImportSelectionBox, MouseDownPos.X);
                    ImportSelectionBox.Width = mousePos.X - MouseDownPos.X;
                }
                else {
                    Canvas.SetLeft(ImportSelectionBox, mousePos.X);
                    ImportSelectionBox.Width = MouseDownPos.X - mousePos.X;
                }

                if (MouseDownPos.Y < mousePos.Y) {
                    Canvas.SetTop(ImportSelectionBox, MouseDownPos.Y);
                    ImportSelectionBox.Height = mousePos.Y - MouseDownPos.Y;
                }
                else {
                    Canvas.SetTop(ImportSelectionBox, mousePos.Y);
                    ImportSelectionBox.Height = MouseDownPos.Y - mousePos.Y;
                }
            }
        }

        private void AddSliderLine() { //add the default line that shows the user where they are on the timeline
            SliderLine.Stroke = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0d89ff"));
            SliderLine.X1 = ImportTimeLineSlider.Value;
            SliderLine.X2 = ImportTimeLineSlider.Value;
            SliderLine.Y1 = 0;
            SliderLine.Y2 = 50;
            SliderLine.StrokeThickness = 1;
            SliderLine.Tag = "SliderLine";
            ImportTimeLineCanvas.Children.Add(SliderLine);
        }

        private void ImportTimeLineSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (ImportTimeLineSlider != null && ImportVideoElement != null && ImportSelectWindowHint != null) {

                if (!HintAndBoxInitialized) { //when slider moves for the first time hide the hint image and remove opacity from the video element as well as draw the selection box around the entire video
                    ImportVideoElement.Opacity = 1.0;
                    ImportSelectWindowHint.Visibility = Visibility.Collapsed;
                    ResetSelectionBox();
                    HintAndBoxInitialized = true;
                }

                TimeSpan time = TimeSpan.FromSeconds((ImportTimeLineSlider.Value / ImportTimeLineSlider.Maximum) * VideoDuration); //move to the correct position in the video on slider value change
                ImportVideoElement.Position = time;

                if (ImportTimeLineCanvas.Children.Contains(SliderLine)) { //if the slider line already exists just edit it's location on the timeline
                    //int index = TimeLineCanvas.Children.IndexOf(SliderLine);
                    ((Line)ImportTimeLineCanvas.Children[0]).X1 = ImportTimeLineSlider.Value;
                    ((Line)ImportTimeLineCanvas.Children[0]).X2 = ImportTimeLineSlider.Value;
                }
                else {
                    AddSliderLine(); //if it hasn't been added yet, add it
                }

                bool previousInsideSegment = SliderInsideSegment; //we use these to determine if the current slider change resulted in the slider ending up in one of the cut segments

                for (int i = 0; i + 1 < AllCuts.Count; i = i + 2) {
                    if (ImportTimeLineSlider.Value <= Math.Max(AllCuts[i], AllCuts[i + 1]) && ImportTimeLineSlider.Value >= Math.Min(AllCuts[i], AllCuts[i + 1])) { //if it's inside a segment change the bool var
                        SliderInsideSegment = true;
                        break; //and break
                    }
                    if (i + 3 >= AllCuts.Count) SliderInsideSegment = false; //in case we're at the end of the loop and haven't broken it -> we're outside of the segment
                }

                if (previousInsideSegment != SliderInsideSegment) { //and here we can see if the bool var switched when going through the loop above
                    if (SliderInsideSegment) { //if it flipped from false to true color the canvas
                        SolidColorBrush shrub = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0d89ff"));
                        shrub.Opacity = 0.6;
                        ImportSelectionBox.Fill = shrub;
                    }
                    else { //if it flipped from true to false remove the opaque rectangle
                        ImportSelectionBox.Fill = null;
                    }
                }
            }
        }

        private void ResetSelectionBox() {
            Canvas.SetLeft(ImportSelectionBox, 0);
            Canvas.SetTop(ImportSelectionBox, 0);
            ImportSelectionBox.Width = ImportVideoElement.ActualWidth;
            ImportSelectionBox.Height = ImportVideoElement.ActualHeight;
        }

        private void AdjustWindowSize() {
            this.Width = ImportVideoElement.ActualWidth;
        }

        private void ImportWindow_SizeChanged(object sender, SizeChangedEventArgs e) {
            ResetSelectionBox();
            AdjustSlider();
        }

        private void AdjustSlider() { //set the slider's value to make sure the ui elems are always in the right places (we use this when the window gets resized and when the window's launched)
            if (ImportTimeLineSlider != null && this != null) {
                Point timeLineOrigin = ImportTimeLineCanvas.TransformToAncestor(this).Transform(new Point(0, 0));
                ImportTimeLineSlider.Minimum = timeLineOrigin.X - 5;
                ImportTimeLineSlider.Maximum = timeLineOrigin.X + ImportTimeLineCanvas.ActualWidth - 5;
            }
        }




















        // MARK: Button Clicks

        private void ImportUndoButton_Click(object sender, RoutedEventArgs e) {
            if (ImportTimeLineCanvas.Children.Count > 1) { //slider line is the first elem - never remove that one
                ImportTimeLineCanvas.Children.RemoveAt(ImportTimeLineCanvas.Children.Count - 1);
                AllCuts.RemoveAt(AllCuts.Count - 1);
            }

            if (AllCuts.Count < 2) ImportDoneButton.IsEnabled = false;
        }

        private void ImportCutButton_Click(object sender, RoutedEventArgs e) {
            double attemptedCut = ImportTimeLineSlider.Value; //remember the cut position
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
                    line.X1 = ImportTimeLineSlider.Value;
                    line.X2 = ImportTimeLineSlider.Value;
                    line.Y1 = 0;
                    line.Y2 = ImportTimeLineCanvas.ActualHeight;
                    line.Opacity = 0.5;
                    line.StrokeThickness = 2;
                    ImportTimeLineCanvas.Children.Add(line);
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
                        Rectangle rect;
                        rect = new Rectangle();
                        rect.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0d89ff"));
                        rect.Width = Math.Abs(AllCuts[AllCuts.Count - 1] - attemptedCut);
                        rect.Height = ImportTimeLineCanvas.ActualHeight;
                        rect.Opacity = 0.5;
                        if (AllCuts[AllCuts.Count - 1] < attemptedCut) { //if the end of the segment is being added after the previous line then the previous line's position is where we start drawing
                            Canvas.SetLeft(rect, AllCuts[AllCuts.Count - 1]);
                            Canvas.SetTop(rect, 0);
                        }
                        else { //else start drawing at the position of the line that was just added
                            Canvas.SetLeft(rect, attemptedCut);
                            Canvas.SetTop(rect, 0);
                        }
                        ImportTimeLineCanvas.Children.Add(rect);
                        AllCuts.Add(attemptedCut); //add the cut to our cut positions
                    }
                }
            }

            if (AllCuts.Count > 1) ImportDoneButton.IsEnabled = true;
            else ImportDoneButton.IsEnabled = false;
        }

        private void ImportDoneButton_Click(object sender, RoutedEventArgs e) {
            BarInteraction();
            MirrorOrCrop();
        }

        


















        // MARK: Croppin'n Splittin'n Mirrorin'

        private void MirrorOrCrop() {
            double boxWidth = ImportSelectionBox.ActualWidth;
            double boxHeight = ImportSelectionBox.ActualHeight;
            double vidWidth = ImportVideoElement.ActualWidth;
            double vidHeight = ImportVideoElement.ActualHeight;

            if (boxWidth == vidWidth && boxHeight == vidHeight) { //no cropping -> determine whether to mirror and split, or just split
                if (MessageBox.Show("Do you want to mirror the video horizontally? (For all gait analysis videos, the mouse has to be facing the right side of the video.)", "Mirror Video?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                    StartCheckingForCompletion(4); //mirror, then split
                    Thread addVideoThread = new Thread(MirrorVideo);
                    addVideoThread.IsBackground = true;
                    addVideoThread.Start();
                }
                else { //split right away
                    Thread splitThread = new Thread(SplitVideo);
                    splitThread.IsBackground = true;
                    splitThread.Start();
                }
            }
            else {
                StartCheckingForCompletion(3); //do crop the video, will still have to ask whether to mirror and then to either mirror and split, or just split
                Thread cropThread = new Thread(CropVideo);
                cropThread.IsBackground = true;
                cropThread.Start();
            }
        }

        private void MirrorVideo() {
            string mirrorOutputPath = FileSystemUtils.ExtendPath(CachePath, FileSystemUtils.GetFileName(VideoCachePath) + "_mr" + FileSystemUtils.GetFileExtension(VideoCachePath));
            string ffmpegCommand = "-y -i \"" + VideoCachePath + "\" -qscale 0 -map_metadata 0 -crf 0 -vf \"hflip\" \"" + mirrorOutputPath + "\"";
            VideoCachePath = mirrorOutputPath; //from now on we're dealing with the mirrored video
            using (var engine = new Engine()) {
                engine.ConversionCompleteEvent += MirroringCompleteEvent;
                engine.CustomCommand(ffmpegCommand);
            }
        }

        private void MirroringCompleteEvent(object sender, ConversionCompleteEventArgs args) {
            MirroringFinished = true;
            //TODO: analysis, testing
        }

        private void CropVideo() {
            double pixelConversion = 0;
            int actualX = 0, actualY = 0, selectionWidth = 0, selectionHeight = 0; 

            this.Dispatcher.Invoke(() => {
                pixelConversion = ImportVideoElement.NaturalVideoWidth / ImportVideoElement.ActualWidth;
                actualX = (int)(MouseDownPos.X * pixelConversion);
                actualY = (int)(MouseDownPos.Y * pixelConversion);
                selectionWidth = (int)(ImportSelectionBox.ActualWidth * pixelConversion);
                selectionHeight = (int)(ImportSelectionBox.ActualHeight * pixelConversion);

                if (selectionWidth > ImportVideoElement.NaturalVideoWidth) selectionWidth = ImportVideoElement.NaturalVideoWidth; //for weird edge cases, making sure that the selection box isn't bigger than the actual video
                if (selectionHeight > ImportVideoElement.NaturalVideoHeight) selectionHeight = ImportVideoElement.NaturalVideoHeight;
            });

            if(pixelConversion == 0 && actualX == 0 && actualY == 0 && selectionWidth == 0 && selectionHeight == 0) {
                this.Dispatcher.Invoke(() => {
                    MessageBox.Show("Failed to get cropping values. Please contact support at rfiker@ucalgary.ca", "Cropping Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                }); 
            }
            else {
                string cropOutputPath = FileSystemUtils.ExtendPath(CachePath, FileSystemUtils.GetFileName(VideoCachePath) + "_cr" + FileSystemUtils.GetFileExtension(VideoCachePath));
                string ffmpegCommand = "-i \"" + VideoCachePath + "\" -vf \"crop = " + selectionWidth + ":" + selectionHeight + ":" + actualX + ":" + actualY + "\" " + " -c:v libx264 -crf 0 -c:a copy " + "\"" + cropOutputPath + "\"";
                VideoCachePath = cropOutputPath; //from now on we're dealing with the cropped video
                using (var engine = new Engine()) {
                    engine.ConversionCompleteEvent += CroppingCompleteEvent;
                    engine.CustomCommand(ffmpegCommand);
                }
                
            }
        }

        private void CroppingCompleteEvent(object sender, ConversionCompleteEventArgs args) {
            CroppingFinished = true;
            //TODO: analysis, testing, recursive delete bug
        }

        private void SplitVideo() {
            if (AllCuts.Count < 2) { //0 or 1 cut -> don't split the video
                var inputFile = new MediaFile { Filename = VideoCachePath }; //grab the video and set the format extension
                var engine = new Engine();
                var options = new ConversionOptions { VideoBitRate = VideoBitrate };

                if (IsAnalysisVideo) {
                    var vidDirPath = FileSystemUtils.ExtendPath(ProjectPath, "analyzed-videos", FileSystemUtils.GetFileName(VideoCachePath));
                    Directory.CreateDirectory(vidDirPath);
                    CopyVideo(VideoCachePath, FileSystemUtils.ExtendPath(vidDirPath, FileSystemUtils.GetFileNameWithExtension(VideoCachePath))); //when splitting, the conversion engine takes care of copying the video into the correct destination, here we have to manually copy it to where it belongs
                    AddAnalysisVideo(engine, vidDirPath, inputFile, options, 0, 0);
                }
                else AddTrainingVideo(engine, inputFile, "", options, 0, 0); //no copying or folder creation needed, DLC will take care of it for us

            }
            else { //split the video -> create all the subvideos
                List<double> allCuts = AllCuts;
                double vidDur = VideoDuration;
                double max = 0;
                this.Dispatcher.Invoke(() => {
                    max = ImportTimeLineSlider.Maximum;
                });
                CreateVideos(allCuts, VideoDuration, max, VideoCachePath);
            }
        }

        private void CreateVideos(List<double> AllCuts, double VideoDuration, double SliderMax, string vidPath) {
            if (AllCuts.Count >= 2) { //need at least 2, i.e. one segment
                List<double> timeCutsInVideo = new List<double>();
                for (int i = 0; i < AllCuts.Count; i++) timeCutsInVideo.Add((AllCuts[i] / SliderMax) * VideoDuration); //convert slider values to times within the video
                if (timeCutsInVideo.Count % 2 != 0) timeCutsInVideo.RemoveAt(timeCutsInVideo.Count - 1);//if they started a segment but didn't complete it, disregard

                var inputFile = new MediaFile { Filename = vidPath }; //grab the video and set the format extension
                for (int i = 0; i + 1 < timeCutsInVideo.Count; i = i + 2) { //for each segment use time in seconds to tell the Media Engine where to cut
                    string idedVidName = FileSystemUtils.GetFileName(VideoCachePath) + "_" + (i / 2);
                    string targetVideoPath;

                    if (IsAnalysisVideo) { //video is an analysis video => an entire new folder has to be created inside of "analyzed-videos"
                        Directory.CreateDirectory(FileSystemUtils.ExtendPath(ProjectPath, "analyzed-videos", idedVidName));
                        targetVideoPath = FileSystemUtils.ExtendPath(ProjectPath, "analyzed-videos", idedVidName, idedVidName + FileSystemUtils.GetFileExtension(VideoCachePath));
                    }
                    else { //video is a training video => no extra folders need to be created
                        targetVideoPath = FileSystemUtils.ExtendPath(ProjectPath, "videos", idedVidName + FileSystemUtils.GetFileExtension(VideoCachePath));
                    }

                    var engine = new Engine();
                    engine.GetMetadata(inputFile);
                    var options = new ConversionOptions();
                    options.VideoBitRate = inputFile.Metadata.VideoData.BitRateKbs;
                    options.CutMedia(TimeSpan.FromSeconds(timeCutsInVideo[i]), TimeSpan.FromSeconds(timeCutsInVideo[i + 1]) - TimeSpan.FromSeconds(timeCutsInVideo[i]));
                    engine.Convert(inputFile, new MediaFile { Filename = targetVideoPath}, options); //actually makes the new video

                    if (!IsAnalysisVideo) { //training video
                        AddTrainingVideo(engine, inputFile, idedVidName, options, timeCutsInVideo.Count, i);
                    }
                    else { //analysis video
                        AddAnalysisVideo(engine, FileSystemUtils.ExtendPath(ProjectPath, "analyzed-videos", idedVidName), inputFile, options, timeCutsInVideo.Count, i); 
                    }
                }
            }
        }

        private void AddTrainingVideo(Engine engine, MediaFile inputFile, string idedVidName, ConversionOptions options, int timeCutsCount, int index) {
            StopBool = true;
            string labeledDataPath = FileSystemUtils.ExtendPath(ProjectPath, "labeled-data", idedVidName);
            Directory.CreateDirectory(labeledDataPath);
            var finalVidPath = VideoCachePath; //default path in cache (the video hasn't been split)
            string thumbPath = FileSystemUtils.ExtendPath(ProjectPath, "videos", FileSystemUtils.GetFileName(finalVidPath) + ".png");
            if (timeCutsCount > 0) { //video has been split or trimmed
                thumbPath = FileSystemUtils.ExtendPath(ProjectPath, "videos", idedVidName + ".png");
                finalVidPath = FileSystemUtils.ExtendPath(ProjectPath, "videos", idedVidName + FileSystemUtils.GetFileExtension(VideoCachePath)); //split engine would copy it directly into the projects videos folder with the indexed name
            }
            engine.GetThumbnail(inputFile, new MediaFile(thumbPath), options); //and create a thumbnail for the training video
            engine.Dispose();
            AddTrainingVideoInDLC(finalVidPath, timeCutsCount, index);
            while (StopBool) { //this will force the loop to wait for the above method call to finish
            }
        }

        private void AddAnalysisVideo(Engine engine, string outputVidFolder, MediaFile inputFile, ConversionOptions options, int timeCutsCount, int index) {
            engine.GetThumbnail(inputFile, new MediaFile { Filename = FileSystemUtils.ExtendPath(outputVidFolder, "thumbnail.png")}, options); //and create a thumbnail for the analysis video
            engine.Dispose();
            string settingsPath = FileSystemUtils.ExtendPath(outputVidFolder, "settings.txt"); //create default settings file for the new analysis video
            StreamWriter sw1 = new StreamWriter(settingsPath);
            sw1.WriteLine("dotsize: " + "5");
            sw1.Close();
            this.Dispatcher.Invoke(() => {
                if (index + 3 >= timeCutsCount) { //if we're at the last video notify the ui thread that we're done by enabling interaction and terminating the window
                    EnableInteraction();
                    this.DialogResult = true;
                }
            });
        }

        private void AddTrainingVideoInDLC(string vidPath, int count, int i) { //it's not enough for a newly added training video to be simply added to the folder, we need to add it to the config file through DeepLabCut

            PythonScripts allScripts = new PythonScripts();
            string filePath = FileSystemUtils.ExtendPath(EnvDirectory, "vdlc_add_video.py");
            FileSystemUtils.MurderPython();
            FileSystemUtils.RenewScript(filePath, allScripts.AddVideo);
            FileSystemUtils.ReplaceStringInFile(filePath, "copy_videos_identifier", "True");
            FileSystemUtils.ReplaceStringInFile(filePath, "config_path_identifier", FileSystemUtils.ExtendPath(ProjectPath, "config.yaml"));
            FileSystemUtils.ReplaceStringInFile(filePath, "video_path_identifier", vidPath);
            Process p = new Process();
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "cmd.exe";
            info.RedirectStandardInput = true;
            info.UseShellExecute = false;
            info.Verb = "runas";
            info.CreateNoWindow = !ReadShowDebugConsole();

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
                    sw.WriteLine("\"C:\\Program Files (x86)\\VisualGaitLab\\Miniconda3\\Scripts\\activate.bat\"");
                    sw.WriteLine("conda activate " + EnvName);
                    sw.WriteLine("ipython vdlc_add_video.py");

                    if (info.CreateNoWindow == false) { //for debug purposes
                        sw.WriteLine("ECHO WHEN YOU'RE DONE, CLOSE THIS WINDOW");
                        p.WaitForExit();
                        sw.WriteLine("Done, exiting.");
                    }
                }
            }
        }

















        //MARK: Supporting Methods

        private void CopyVideo(string inputPath, string outputPath) {
            File.Copy(inputPath, outputPath, true);
            FileSystemUtils.WaitForFile(outputPath);
        }

        private void BarInteraction() {
            ImportPrimaryPanel.IsEnabled = false;
            ImportPrimaryPanel.Opacity = 0.3;
            ImportPRing.IsActive = true;
        }

        private void EnableInteraction() {
            this.Dispatcher.Invoke(() => {
                ImportPrimaryPanel.IsEnabled = true;
                ImportPrimaryPanel.Opacity = 1;
                ImportPRing.IsActive = false;
            });
        }

        private bool ReadShowDebugConsole() {
            string settingsPath = FileSystemUtils.ExtendPath(ProgramFolder, "settings.txt");
            bool retVal = false;
            if (File.Exists(settingsPath)) {
                StreamReader sr = new StreamReader(settingsPath);
                String[] rows = Regex.Split(sr.ReadToEnd(), "\r\n");
                List<string> listRows = new List<string>(rows);
                sr.Close();

                for (int i = 0; i < listRows.Count; i++) {
                    string currentLine = listRows[i];
                    if (currentLine.Contains("showdebugconsole: ") && currentLine.Contains("True")) retVal = true;
                }
            }
            return retVal;
        }
    }
}
