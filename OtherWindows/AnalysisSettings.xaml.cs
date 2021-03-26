using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Interaction logic for AnalysisSettings.xaml
    /// </summary>
    public partial class AnalysisSettings : Window {
        public AnalysisSettings(string path, string name, string dotsize) {
            InitializeComponent();
            Title = "Analysis Settings for " + name;
            BitmapImage bitmap = new BitmapImage(new Uri(path, UriKind.Absolute));
            LabelThumbnail.Source = bitmap;
            LabelThumbnail.Width = bitmap.Width;
            LabelThumbnail.Height = bitmap.Height;
            CurrentLabelSize.Text = dotsize;
            int intVal = int.Parse(dotsize);
            LabelPreviewImage.Width = intVal * 2;
            LabelPreviewImage.Height = intVal * 2;
            LabelSlider.Value = intVal;
            this.UpdateLayout();
        }

        private void LabelSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            double currentValue = LabelSlider.Value;
            string strVal = Convert.ToString(Math.Round(currentValue));
            int intVal = int.Parse(strVal);
            LabelPreviewImage.Width = intVal * 2;
            LabelPreviewImage.Height = intVal * 2;
            CurrentLabelSize.Text = strVal;
        }

        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        public bool getCheckBoxValue()
        {
            return SameSizeForAnalysisCheckBox.IsChecked ?? false;
        }
    }
}
