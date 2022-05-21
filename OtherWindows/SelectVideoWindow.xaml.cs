using System.Collections.Generic;
using System.Windows;
using VisualGaitLab.SupportingClasses;

namespace VisualGaitLab.OtherWindows
{
    public partial class SelectVideoWindow : Window
    {
        private List<AnalysisVideo> videos;

        public SelectVideoWindow(List<AnalysisVideo> analysisVideos)
        {
            InitializeComponent();
            videos = analysisVideos;
            if (videos != null && videos.Count > 0)
            {
                System.Console.WriteLine(videos[0].Name);
                VideoListBox.ItemsSource = null;
                VideoListBox.ItemsSource = videos;
            }
            else
            {
                MessageBox.Show("Videos have yet to be gait analyzed!", "No Videos", MessageBoxButton.OK, MessageBoxImage.Error);
                DialogResult = false;
                Close();
            }
        }


        // Done selection
        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
