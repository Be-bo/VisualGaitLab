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
    /// Interaction logic for BitrateWindow.xaml
    /// </summary>
    public partial class BitrateWindow : Window {
        public BitrateWindow() {
            InitializeComponent();
        }

        private void BitrateSaveButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }
    }
}
