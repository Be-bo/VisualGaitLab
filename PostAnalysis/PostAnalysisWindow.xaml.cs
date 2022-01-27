using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VisualGaitLab.PostAnalysis
{
    /// <summary>
    /// Interaction logic for PostAnalysisWindow.xaml
    /// </summary>
    public partial class PostAnalysisWindow : Window
    {

        string ScriptPath;
        int ParamNum = 0;


        public PostAnalysisWindow(string scriptPath, string scriptName)
        {
            InitializeComponent();
            Title = scriptName;
            ScriptPath = scriptPath;
            ReadInfo();
            CheckParams();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DialogResult = true;
        }

        private void ReadInfo()
        {

            // TODO: Set info and number of parameters (changing param title) to unlock run button
        }

        private void CheckParams()
        {
            if (ParamListBox.Items.Count == ParamNum)
            {
                RunButton.IsEnabled = true;
            }
            else RunButton.IsEnabled = false;
        }

        private void AddFileParam_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddStrParam_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MoveUpParam_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MoveDownParam_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RemoveParam_Click(object sender, RoutedEventArgs e)
        {

        }


        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO: 
        }
    }
}
