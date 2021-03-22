using System;
using System.Windows;

namespace VisualGaitLab.OtherWindows
{
    /// <summary>
    /// Interaction logic for AdjustBiasWindow.xaml
    /// </summary>
    public partial class AdjustBiasWindow : Window
    {
        private readonly double previousBias;

        public AdjustBiasWindow(double currentBias)
        {
            InitializeComponent();
            previousBias = currentBias;
            biasSlider.Value = currentBias;
        }

        private void BiasSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            biasValue.Text = "Bias: " + biasSlider.Value.ToString("0.00");
            if (biasSlider.Value != previousBias){
                continueButton.IsEnabled = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        public double GetAdjustedBias()
        {
            return Math.Round(biasSlider.Value, 2, MidpointRounding.AwayFromZero);
        }
    }
}
