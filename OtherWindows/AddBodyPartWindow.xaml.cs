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
    /// Interaction logic for AddBodyPartWindow.xaml
    /// </summary>
    public partial class AddBodyPartWindow : Window {

        Regex letterAndNumberRegex = new Regex("^[a-zA-Z0-9]+$");
        private BrushConverter converter = new System.Windows.Media.BrushConverter();

        public AddBodyPartWindow() {
            InitializeComponent();
        }



        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
        }

        private void AddBodyPartButton_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
        }

        private void NewBodyPartTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            if (AddBodyPartButton != null && NewBodyPartTextBox != null) {
                if (!NewBodyPartTextBox.Text.Equals("Bodypart") && NewBodyPartTextBox.Text.Length > 1 && letterAndNumberRegex.IsMatch(NewBodyPartTextBox.Text)) {
                    AddBodyPartButton.IsEnabled = true;
                }
                else {
                    AddBodyPartButton.IsEnabled = false;
                }
            }
        }

        private void NewBodyPartTextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            NewBodyPartTextBox.Text = "";
            var brush = (Brush)converter.ConvertFromString("#000000");
            NewBodyPartTextBox.Foreground = brush;
        }
    }
}
