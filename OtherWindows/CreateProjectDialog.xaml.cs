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
    /// Interaction logic for CreateProjectDialog.xaml
    /// </summary>
    public partial class CreateProjectDialog : Window {
        public List<string> bodyParts = new List<string>();
        private BrushConverter converter = new System.Windows.Media.BrushConverter();
        private bool isGateOnly = false;

        public CreateProjectDialog(bool gaitOnly) //set up body parts for the new project, if it's a gait project then leave the Add Bodypart feature hidden
        {
            InitializeComponent();
            isGateOnly = gaitOnly;
            if (isGateOnly) {
                bodyParts.Add("HindLeft1");
                bodyParts.Add("HindLeft2");
                bodyParts.Add("HindRight1");
                bodyParts.Add("HindRight2");
                bodyParts.Add("FrontLeft1");
                bodyParts.Add("FrontLeft2");
                bodyParts.Add("FrontRight1");
                bodyParts.Add("FrontRight2");
                bodyParts.Add("Nose");
                bodyParts.Add("Butt");
                bodyParts.Add("MidPointLeft");
                bodyParts.Add("MidPointRight");
            }
            else {
                CreateAddDock.Visibility = Visibility.Visible; //make the Add Bodypart feature for custom tracking available if the user didn't choose gait analysis
                bodyParts.Add("HindRight");
                bodyParts.Add("HindLeft");
            }
            bodyPartListBox.ItemsSource = bodyParts;
        }

        private void ContentChanged(object sender, TextChangedEventArgs e) //check user's input and allow them to press "Create" if everything checks out
        {
            if (projectNameTextBox != null && authorTextBox != null && CreateProjectButton != null) {

                string currentProjName = projectNameTextBox.Text;
                string currentAuthor = authorTextBox.Text;
                bool projOk = false;
                bool authorOk = false;

                Regex letterRegex = new Regex("^[a-zA-Z]+$");
                Regex letterAndNumberRegex = new Regex("^[a-zA-Z0-9]+$");
                Regex numberRegex = new Regex("^[0-9]*$");

                if (currentProjName.Length > 1 && currentProjName.Length < 21 && letterAndNumberRegex.IsMatch(currentProjName)) projOk = true;
                if (currentAuthor.Length > 1 && currentAuthor.Length < 21 && letterAndNumberRegex.IsMatch(currentAuthor)) authorOk = true;
                if (projOk && authorOk && bodyParts.Count > 0) CreateProjectButton.IsEnabled = true;
                else CreateProjectButton.IsEnabled = false;
                if (sender != null) TextBoxTextChanged(sender, e);
            }
        }

        private void OkButtonClicked(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e) //adding a custom bodypart logic
        {
            AddBodyPartWindow addWindow = new AddBodyPartWindow();
            if (addWindow.ShowDialog() == true) {
                string newBodypart = addWindow.NewBodyPartTextBox.Text;
                if (!bodyParts.Contains(newBodypart)) bodyParts.Add(newBodypart);
                bodyPartListBox.ItemsSource = null;
                bodyPartListBox.ItemsSource = bodyParts;
                ContentChanged(null, null);
            }
        }

        private void TextBoxTextChanged(object sender, TextChangedEventArgs e) {
            TextBox box = sender as TextBox;
            var brush = (Brush)converter.ConvertFromString("#000000");
            box.Foreground = brush;
        }

        private void ProjectNameTextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            TextBox box = sender as TextBox;
            box.Text = "";
            var brush = (Brush)converter.ConvertFromString("#000000");
            box.Foreground = brush;
        }

        private void ProjectNameTextBox_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
            if (projectNameTextBox.Text.Equals("")) {
                projectNameTextBox.Text = "Project Name";
                var brush = (Brush)converter.ConvertFromString("#909090");
                projectNameTextBox.Foreground = brush;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e) { //if the delete bodypart button is pressed
            string item = (string)bodyPartListBox.SelectedItem;
            bodyParts.Remove(item);
            bodyPartListBox.ItemsSource = null;
            bodyPartListBox.ItemsSource = bodyParts;
            ContentChanged(null, null);
        }
    }
}
