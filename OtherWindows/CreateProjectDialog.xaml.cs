using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VisualGaitLab.SupportingClasses;

namespace VisualGaitLab.OtherWindows {
    /// <summary>
    /// Interaction logic for CreateProjectDialog.xaml
    /// </summary>
    public partial class CreateProjectDialog : Window {
        public List<string> bodyParts = new List<string>();
        private BrushConverter converter = new BrushConverter();
        public string projectFolder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\VisualGaitLab\\Projects";
        private static string dateIdentifier = "";

        public CreateProjectDialog(bool gaitOnly) //set up body parts for the new project, if it's a gait project then leave the Add Bodypart feature hidden
        {
            InitializeComponent();
            if (gaitOnly) {
                foreach (var bodypart in GaitBodyParts.names)
                {
                    bodyParts.Add(bodypart);
                }
            }
            else {
                CreateAddButton.Visibility = Visibility.Visible; //make the Add Bodypart feature for custom tracking available if the user didn't choose gait analysis
                bodyParts.Add("HindRight");
                bodyParts.Add("HindLeft");
            }
            bodyPartListBox.ItemsSource = bodyParts;

            // Initialize Project Path
            string month = DateTime.Now.Month.ToString();
            string day = DateTime.Now.Day.ToString();
            if (month.Length < 2) month = "0" + month;
            if (day.Length < 2) day = "0" + day;
            dateIdentifier = DateTime.Now.Year.ToString() + "-" + month + "-" + day;
            updateProjectPath();
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
            updateProjectPath();
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

        private void projectPathTextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    projectFolder = dialog.SelectedPath;
                    updateProjectPath();
                }
            }
        }

        private void updateProjectPath()
        {
            projectPathTextBox.Text =
                projectFolder + "\\" +
                projectNameTextBox.Text + "-" +
                authorTextBox.Text + "-" +
                dateIdentifier;
        }
    }
}
