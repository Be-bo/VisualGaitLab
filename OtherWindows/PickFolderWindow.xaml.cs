using Microsoft.WindowsAPICodePack.Dialogs;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace VisualGaitLab.OtherWindows
{
    /// <summary>
    /// Interaction logic for PickFolderWindow.xaml
    /// </summary>
    public partial class PickFolderWindow : Window
    {
        public PickFolderWindow(string message, string startingDir, string title)
        {
            InitializeComponent();
            Message_textblock.Text = message;
            Directory_textbox.Text = startingDir;
            Title = title;
        }

        private void Directory_textbox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ChangeDirectory();
        }

        private void ChangeDirectory_button_Click(object sender, RoutedEventArgs e)
        {
            ChangeDirectory();
        }

        private void ChangeDirectory()
        {
            var window = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = Directory_textbox.Text;
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog(window) == CommonFileDialogResult.Ok)
            {
                Directory_textbox.Text = dialog.FileName;
            }
        }

        private void Ok_button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
