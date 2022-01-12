using System.Windows;
using VisualGaitLab.SupportingClasses;
using VisualGaitLab.GaitAnalysis;
using System.IO;
using System.Windows.Controls;
using VisualGaitLab.OtherWindows;

namespace VisualGaitLab
{
    public partial class MainWindow : Window
    {
        private void PreparePostAnalysisTab()
        {
            PostAnalysisTab.IsEnabled = true;
        }

        private void ScriptListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void NewScriptClicked(object sender, RoutedEventArgs e)
        {

        }

        private void RunScriptButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
