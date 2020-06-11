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
    /// Interaction logic for EvalWindow.xaml
    /// </summary>
    public partial class EvalWindow : Window {
        public EvalWindow(string elapsedTime, string trainErr, string testErr, string pCutoff) {
            InitializeComponent();
            TimeTextBlock.Text = elapsedTime;
            TrainErrorTextBlock.Text = trainErr;
            TestErrorTextBlock.Text = testErr;
            PCutoffTextBlock.Text = pCutoff;
        }
    }
}
