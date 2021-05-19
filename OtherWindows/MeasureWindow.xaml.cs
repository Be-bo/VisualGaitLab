using System;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace VisualGaitLab.OtherWindows {
    /// <summary>
    /// Interaction logic for MeasureWindow.xaml
    /// </summary>
    public partial class MeasureWindow : Window {

        Regex FloatRegex = new Regex("^[0-9]+(?:\\.[0-9]+)?$");
        Regex ZeroRegex = new Regex("^[0]*(?:\\.[0]*)?$");

        private static string ImagePath;
        private static BitmapImage Bmap;
        private static Vector2 thumbie1_loc;
        private static Vector2 thumbie2_loc;
        private static string distance_txt;
        private static string speed_txt;
        private static bool isFreeRun;


        public MeasureWindow(string imagePath) { //set up canvas with a screenshot from the current video
            InitializeComponent();

            // Set up image
            if (imagePath != ImagePath)
            {
                ImagePath = imagePath;
                Console.WriteLine(imagePath);
                Bmap = new BitmapImage(new Uri(ImagePath, UriKind.Absolute));
            }
            CanvasBackground.Source = Bmap;

            SetThumbieLocations();
            SetStaticSettings();
        }


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
            Console.WriteLine("\nbool: " + isFreeRun);

            if (isFreeRun) AnalysisTypeRadioFreeWalking_Checked(null, null);
            else AnalysisTypeRadioFreeWalking_Unchecked(null, null);

            if (distance_txt == null) distance_txt = DistanceTextBox.Text;
            else DistanceTextBox.Text = distance_txt;

            if (speed_txt == null) speed_txt = TreadmillSpeedTextBox.Text;
            else TreadmillSpeedTextBox.Text = speed_txt;
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e) {
            distance_txt = DistanceTextBox.Text;
            speed_txt = TreadmillSpeedTextBox.Text;
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
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
            double onScreenHeight = MeasuringCanvas.ActualHeight;
            double onScreenWidth = MeasuringCanvas.ActualHeight;
            int imageHeight = Bmap.PixelHeight;
            int imageWidth = Bmap.PixelWidth;
            double firstPointX = Canvas.GetLeft(Thumbie1) + 5; //+ five cuz width and height are 10 -> to get the center of the point
            double firstPointY = Canvas.GetTop(Thumbie1) + 5;
            double secondPointX = Canvas.GetLeft(Thumbie2) + 5;
            double secondPointY = Canvas.GetTop(Thumbie2) + 5;

            double firstPointActualX = (double)firstPointX * (imageWidth / onScreenWidth);
            double firstPointActualY = (double)firstPointY * (imageHeight / onScreenHeight);

            double secondPointActualX = (double)secondPointX * (imageWidth / onScreenWidth);
            double secondPointActualY = (double)secondPointY * (imageHeight / onScreenHeight);

            double pixelDistance = Math.Sqrt(Math.Pow(secondPointActualX - firstPointActualX, 2) + Math.Pow(secondPointActualY - firstPointActualY, 2));
            return float.Parse(DistanceTextBox.Text) / pixelDistance;
        }

        private void AnalysisTypeRadioFreeWalking_Checked(object sender, RoutedEventArgs e) { //free walking selected, bar the treadmill speed option
            TreadmillSpeedTextBox.IsEnabled = false;
            TreadmillSpeedTextBox.Text = "0";
            isFreeRun = true;
        }

        private void AnalysisTypeRadioFreeWalking_Unchecked(object sender, RoutedEventArgs e) { //free walking unselected, open the treadmill speed option
            TreadmillSpeedTextBox.IsEnabled = true;
            isFreeRun = false;
        }
    }
}
