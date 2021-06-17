using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static VisualGaitLab.SupportingClasses.MathUtils;


namespace VisualGaitLab.OtherWindows {
    /// <summary>
    /// Interaction logic for MeasureWindow.xaml
    /// </summary>
    public partial class MeasureWindow : Window {

        Regex FloatRegex = new Regex("^[0-9]+(?:\\.[0-9]+)?$");
        Regex ZeroRegex = new Regex("^[0]*(?:\\.[0]*)?$");

        private string StateFolder;
        private static string ImagePath;
        private static BitmapImage Bmap;
        private static Vector2 thumbie1_loc;
        private static Vector2 thumbie2_loc;
        private static string distance_txt;
        private static string speed_txt;


        public MeasureWindow(string imagePath, string stateFolder) { //set up canvas with a screenshot from the current video
            InitializeComponent();
            StateFolder = stateFolder;

            // Set up image
            if (imagePath != ImagePath)
            {
                ImagePath = imagePath;
                Console.WriteLine(imagePath);
                Bmap = new BitmapImage(new Uri(ImagePath, UriKind.Absolute));
            }
            CanvasBackground.Source = Bmap;

            LoadSettings();
        }

        

        // MARK: IO operations
        
        private void LoadSettings()
        { // Load previously saved settings if they exist, else use static data or use default empty values
            string settingsFile = StateFolder + "\\gaitSettings.txt";

            if (Directory.Exists(StateFolder) && File.Exists(settingsFile))
            {
                List<string> tempList = File.ReadAllLines(settingsFile).ToList();
                if (tempList.Count >= 6)
                {
                    thumbie1_loc = new Vector2 (float.Parse(tempList[0]), float.Parse(tempList[1]));
                    thumbie2_loc = new Vector2(float.Parse(tempList[2]), float.Parse(tempList[3]));
                    distance_txt = tempList[4];
                    speed_txt = tempList[5];
                }
            }

            SetThumbieLocations();
            SetStaticSettings();
            CheckInput();
        }


        private void SaveSettings()
        { // Save settings so the user doesn't have to redo them for the same video
            Directory.CreateDirectory(StateFolder);            // Make sure the directory exists

            List<string> tempList = new List<string>();
            tempList.Add(thumbie1_loc.X.ToString());
            tempList.Add(thumbie1_loc.Y.ToString());
            tempList.Add(thumbie2_loc.X.ToString());
            tempList.Add(thumbie2_loc.Y.ToString());
            tempList.Add(distance_txt);
            tempList.Add(speed_txt);

            System.IO.File.WriteAllLines(StateFolder + "\\gaitSettings.txt", tempList);
        }



        // MARK: Load static values onto the window 


        private void SetThumbieLocations()
        {
            if (thumbie1_loc == null || thumbie2_loc == null)
            {
                thumbie1_loc = new Vector2 ((float)Canvas.GetLeft(Thumbie1), (float)Canvas.GetTop(Thumbie1));
                thumbie2_loc = new Vector2 ((float)Canvas.GetLeft(Thumbie2), (float)Canvas.GetTop(Thumbie2));
            }
            else
            {
                Canvas.SetLeft(Thumbie1, thumbie1_loc.X);
                Canvas.SetTop(Thumbie1, thumbie1_loc.Y);
                Canvas.SetLeft(Thumbie2, thumbie2_loc.X);
                Canvas.SetTop(Thumbie2, thumbie2_loc.Y);
            }
        }

        private void SetStaticSettings()
        {
            if (distance_txt == null) distance_txt = DistanceTextBox.Text;
            else DistanceTextBox.Text = distance_txt;

            if (speed_txt == null) speed_txt = TreadmillSpeedTextBox.Text;
            else TreadmillSpeedTextBox.Text = speed_txt;

            if (speed_txt != "0")
            {
                AnalysisTypeRadioTreadmill.IsChecked = true;
                AnalysisTypeRadioFreeWalking.IsChecked = false;
            }
            else
            {
                AnalysisTypeRadioTreadmill.IsChecked = false;
                AnalysisTypeRadioFreeWalking.IsChecked = true;
            }
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e) {
            distance_txt = DistanceTextBox.Text;
            speed_txt = TreadmillSpeedTextBox.Text;
            SaveSettings();
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void DistanceTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            CheckInput();
        }


        private void TreadmillSpeedTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            CheckInput();
        }


        private void CheckInput() {
            if (DistanceTextBox != null && ContinueButton != null && TreadmillSpeedTextBox != null) {
                if ((bool)AnalysisTypeRadioTreadmill.IsChecked) {
                    if (FloatRegex.IsMatch(DistanceTextBox.Text) && !ZeroRegex.IsMatch(DistanceTextBox.Text) && FloatRegex.IsMatch(TreadmillSpeedTextBox.Text) && !ZeroRegex.IsMatch(TreadmillSpeedTextBox.Text)) {
                        ContinueButton.IsEnabled = true;
                    }
                    else {
                        ContinueButton.IsEnabled = false;
                    }
                }
                if((bool)AnalysisTypeRadioFreeWalking.IsChecked) {
                    if(FloatRegex.IsMatch(DistanceTextBox.Text) && !ZeroRegex.IsMatch(DistanceTextBox.Text)) {
                        ContinueButton.IsEnabled = true;
                    } else {
                        ContinueButton.IsEnabled = false;
                    }
                }
            }
        }

        private void Thumbie1_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        { //dragging logic for the first green dot
            double newXPos = Canvas.GetLeft(Thumbie1) + e.HorizontalChange;
            double newYPos = Canvas.GetTop(Thumbie1) + e.VerticalChange;

            if (newXPos > 0 && newYPos > 0 && newXPos < MeasuringCanvas.ActualWidth - Thumbie1.ActualWidth && newYPos < MeasuringCanvas.ActualHeight - Thumbie1.ActualHeight)
            {
                Canvas.SetLeft(Thumbie1, newXPos);
                Canvas.SetTop(Thumbie1, newYPos);
                thumbie1_loc.X = (float)newXPos;
                thumbie1_loc.Y = (float)newYPos;
            }
        }

        private void Thumbie2_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e) { //dragging logic for the second green dot
            double newXPos = Canvas.GetLeft(Thumbie2) + e.HorizontalChange;
            double newYPos = Canvas.GetTop(Thumbie2) + e.VerticalChange;

            if (newXPos > 0 && newYPos > 0 && newXPos < MeasuringCanvas.ActualWidth - Thumbie2.ActualWidth && newYPos < MeasuringCanvas.ActualHeight - Thumbie2.ActualHeight) {
                Canvas.SetLeft(Thumbie2, newXPos);
                Canvas.SetTop(Thumbie2, newYPos);
                thumbie2_loc.X = (float) newXPos;
                thumbie2_loc.Y = (float) newYPos;
            }
        }

        public double getSinglePixelSize() { //return the number of milimeters corresponding to a single pixel based on the current position of the two green dots and the user inputted distance between them
            
            // Need to get the true pixel length
            double pixelRatioX = Bmap.PixelWidth / MeasuringCanvas.ActualWidth; // image / onScreen
            double pixelRatioY = Bmap.PixelHeight / MeasuringCanvas.ActualHeight;

            //+ five cuz width and height are 10 -> to get the center of the point
            double firstPointActualX = (thumbie1_loc.X + 5) * pixelRatioX;
            double firstPointActualY = (thumbie1_loc.Y + 5) * pixelRatioY;
            double secondPointActualX = (thumbie2_loc.X + 5) * pixelRatioX;
            double secondPointActualY = (thumbie2_loc.Y + 5) * pixelRatioY;

            double pixelDistance = CalculateDistanceBetweenPoints(firstPointActualX, firstPointActualY, secondPointActualX, secondPointActualY);
            double multiplier = float.Parse(DistanceTextBox.Text) / pixelDistance;

            // Calculating error:
            Console.WriteLine("ERROR (W/H): " + (MeasuringCanvas.ActualWidth / MeasuringCanvas.ActualHeight));

            Console.WriteLine("RealWorld Multiplier: " + multiplier);
            return multiplier;
        }

        private void AnalysisTypeRadioFreeWalking_Checked(object sender, RoutedEventArgs e) { //free walking selected, bar the treadmill speed option
            TreadmillSpeedTextBox.IsEnabled = false;
            TreadmillSpeedTextBox.Text = "0";
            CheckInput();
        }

        private void AnalysisTypeRadioFreeWalking_Unchecked(object sender, RoutedEventArgs e) { //free walking unselected, open the treadmill speed option
            TreadmillSpeedTextBox.IsEnabled = true;
            CheckInput();
        }
    }
}
