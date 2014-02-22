using Microsoft.Kinect;
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

namespace CameraKinect
{
    /// <summary>
    /// Logica di interazione per DebugWindow.xaml
    /// </summary>
    public partial class ToolWindow : Window
    {
        private Connection pi;
        private KinectSensor sensor;
        private int angle = 3;
        private int step = 5;
        public ToolWindow(Connection c, KinectSensor s)
        {
            InitializeComponent();
            pi = c;
            sensor = s;
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            int h = sensor.ElevationAngle;
            if (h + angle > sensor.MaxElevationAngle) return;
            sensor.ElevationAngle += angle;
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            int h = sensor.ElevationAngle;
            if (h - angle < sensor.MinElevationAngle) return;
            sensor.ElevationAngle -= angle;
        }

        private void sliderGrade_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            angle = (int)sliderGrade.Value;
        }

        private void btnUpPi_Click(object sender, RoutedEventArgs e)
        {
            pi.sendUp(step);
        }

        private void btnDownPi_Click(object sender, RoutedEventArgs e)
        {
            pi.sendDown(step);
        }

        private void sliderStepPi_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            step = (int)sliderStepPi.Value;
        }
    }
}
