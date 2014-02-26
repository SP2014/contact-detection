using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TrackingHand
{
    /// <summary>
    /// Interaction logic for KinectPlacementDialog.xaml
    /// </summary>
    partial class KinectPlacementDialog : Window
    {

        public KinectPlacementDialog()
        {
            InitializeComponent();
        }

        //Default position would be left
        public int kinectPlacement = Globals.LEFT_KINECT;

        private void Left_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            kinectPlacement = Globals.LEFT_KINECT;
            Close();
        }

        private void Right_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            kinectPlacement = Globals.RIGHT_KINECT;
            Close();
        }
    }
}
