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
    /// Interaction logic for FramesToExtractDialog.xaml
    /// </summary>
    public partial class FramesToExtractDialog : Window {

        private BrushConverter converter = new System.Windows.Media.BrushConverter();
        Regex numberRegex = new Regex("^[0-9]*$");
        Regex ZeroRegex = new Regex("^[0]*$");

        public FramesToExtractDialog() {
            InitializeComponent();
        }

        private void FramesToExtractTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            if (StartExtractionButton != null && FramesToExtractTextBox != null) {
                if (!FramesToExtractTextBox.Text.Equals("Bodypart") && FramesToExtractTextBox.Text.Length > 1 && numberRegex.IsMatch(FramesToExtractTextBox.Text) && !ZeroRegex.IsMatch(FramesToExtractTextBox.Text)) {
                    StartExtractionButton.IsEnabled = true;
                }
                else {
                    StartExtractionButton.IsEnabled = false;
                }
            }
        }

        private void FramesToExtractTextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            FramesToExtractTextBox.Text = "";
            var brush = (Brush)converter.ConvertFromString("#000000");
            FramesToExtractTextBox.Foreground = brush;
        }

        private void StartExtractionButton_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
        }
    }
}
