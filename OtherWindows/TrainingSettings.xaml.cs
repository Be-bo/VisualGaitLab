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
    /// Interaction logic for TrainingSettings.xaml
    /// </summary>
    public partial class TrainingSettings : Window {
        Regex LetterRegex = new Regex("^[a-zA-Z]+$");
        Regex LetterAndNumberRegex = new Regex("^[a-zA-Z0-9]+$");
        Regex NumberRegex = new Regex("^[0-9]*$");
        Regex ZeroRegex = new Regex("^[0]*$");
        BrushConverter converter = new System.Windows.Media.BrushConverter();

        public TrainingSettings() {
            InitializeComponent();
        }

        private void ContentChanged(object sender, TextChangedEventArgs e) {
            if (endItersTextBox != null && SaveButton != null) {
                string endIters = endItersTextBox.Text;

                if (NumberRegex.IsMatch(endIters) && !ZeroRegex.IsMatch(endIters)) {
                    SaveButton.IsEnabled = true;
                }

                if (sender != null) TextBoxTextChanged(sender, e);
            }
        }

        private void SaveButtonClicked(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void TextBoxTextChanged(object sender, TextChangedEventArgs e) {
            TextBox box = sender as TextBox;
            var brush = (Brush)converter.ConvertFromString("#000000");
            box.Foreground = brush;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void globalScalePos_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if(GlobalScaleNumberText != null) GlobalScaleNumberText.Text = GlobalScaleSlider.Value.ToString();
        }
    }
}
