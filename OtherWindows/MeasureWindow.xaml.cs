using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace VisualGaitLab.OtherWindows {
    /// <summary>
    /// Interaction logic for MeasureWindow.xaml
    /// </summary>
    public partial class MeasureWindow : Window {

        private string ImagePath;
        private BitmapImage Bmap;
        Regex NumberRegex = new Regex("^[0-9]*$");
        Regex ZeroRegex = new Regex("^[0]*$");

        public MeasureWindow(string imagePath) { //set up canvas with a screenshot from the current video
            InitializeComponent();
            ImagePath = imagePath;
            Bmap = new BitmapImage(new Uri(ImagePath, UriKind.Absolute));
            CanvasBackground.Source = Bmap;
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e) {
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
                    if (NumberRegex.IsMatch(DistanceTextBox.Text) && !ZeroRegex.IsMatch(DistanceTextBox.Text) && NumberRegex.IsMatch(TreadmillSpeedTextBox.Text) && !ZeroRegex.IsMatch(TreadmillSpeedTextBox.Text)) {
                        ContinueButton.IsEnabled = true;
                    }
                    else {
                        ContinueButton.IsEnabled = false;
                    }
                }
                
                if((bool)AnalysisTypeRadioFreeWalking.IsChecked) {
                    if(NumberRegex.IsMatch(DistanceTextBox.Text) && !ZeroRegex.IsMatch(DistanceTextBox.Text)) {
                        ContinueButton.IsEnabled = true;
                    } else {
                        ContinueButton.IsEnabled = false;
                    }
                }
            }
        }

        private void Thumbie2_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e) { //dragging logic for the second green dot
            double newXPos = Canvas.GetLeft(Thumbie2) + e.HorizontalChange;
            double newYPos = Canvas.GetTop(Thumbie2) + e.VerticalChange;

            if (newXPos > 0 && newYPos > 0 && newXPos < MeasuringCanvas.ActualWidth - Thumbie2.ActualWidth && newYPos < MeasuringCanvas.ActualHeight - Thumbie2.ActualHeight) {
                Canvas.SetLeft(Thumbie2, Canvas.GetLeft(Thumbie2) + e.HorizontalChange);
                Canvas.SetTop(Thumbie2, Canvas.GetTop(Thumbie2) + e.VerticalChange);
            }
        }

        private void Thumbie1_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e) { //dragging logic for the first green dot
            double newXPos = Canvas.GetLeft(Thumbie1) + e.HorizontalChange;
            double newYPos = Canvas.GetTop(Thumbie1) + e.VerticalChange;

            if (newXPos > 0 && newYPos > 0 && newXPos < MeasuringCanvas.ActualWidth - Thumbie1.ActualWidth && newYPos < MeasuringCanvas.ActualHeight - Thumbie1.ActualHeight) {
                Canvas.SetLeft(Thumbie1, Canvas.GetLeft(Thumbie1) + e.HorizontalChange);
                Canvas.SetTop(Thumbie1, Canvas.GetTop(Thumbie1) + e.VerticalChange);
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
            return int.Parse(DistanceTextBox.Text) / pixelDistance;
        }

        private void AnalysisTypeRadioFreeWalking_Checked(object sender, RoutedEventArgs e) { //free walking selected, bar the treadmill speed option
            TreadmillSpeedTextBox.IsEnabled = false;
            TreadmillSpeedTextBox.Text = "0";
        }

        private void AnalysisTypeRadioFreeWalking_Unchecked(object sender, RoutedEventArgs e) { //free walking unselected, open the treadmill speed option
            TreadmillSpeedTextBox.IsEnabled = true;
        }
    }
}
