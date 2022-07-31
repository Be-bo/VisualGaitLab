using System.Collections.Generic;
using System.Windows;
using VisualGaitLab.SupportingClasses;

namespace VisualGaitLab.OtherWindows
{
    public partial class SelectVideoWindow : Window
    {
        private List<AnalysisVideo> videos;
        public List<AnalysisVideo> selectedVideos;

        public SelectVideoWindow(List<AnalysisVideo> analysisVideos)
        {
            InitializeComponent();
            videos = analysisVideos;
            if (videos != null && videos.Count > 0)
            {
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
            selectedVideos = new List<AnalysisVideo>();
            foreach (var item in VideoListBox.SelectedItems)
            {
                selectedVideos.Add((AnalysisVideo)item);
            }
            DialogResult = true;
        }
    }
}
