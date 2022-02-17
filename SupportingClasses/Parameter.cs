using System;
using System.Windows;

namespace VisualGaitLab.PostAnalysis
{
    [Serializable]
    public class Parameter
    {
        public Parameter(string txt)
        {
            Txt = txt;
            BlockVisible = Visibility.Visible;
            BoxVisible = Visibility.Collapsed;
        }

        public string Txt { get; set; }
        public Visibility BlockVisible{ get; set; }
        public Visibility BoxVisible { get; set; }

        public void StartEdit()
        {
            BlockVisible = Visibility.Collapsed;
            BoxVisible = Visibility.Visible;
        }

        public void EndEdit()
        {
            BlockVisible = Visibility.Visible;
            BoxVisible = Visibility.Collapsed;
        }
    }
}
