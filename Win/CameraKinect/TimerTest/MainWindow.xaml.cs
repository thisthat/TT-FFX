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
using System.Windows.Navigation;
using System.Windows.Shapes;
using CameraKinect;

namespace TimerTest
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CameraKinect.Connection c;
        public MainWindow()
        {
            InitializeComponent();
            lblInit.Content = "Invio:";
            lblEnd.Content = "Ricezione:";
            lblTot.Content = "Ms Risposta:";
            c = new Connection(null, null);
            c.Connect();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            
            byte[] buffer = new byte[80];
            DateTime Start = DateTime.Now;
            c.Ping(buffer);
            DateTime End = DateTime.Now;
            string r = System.Text.Encoding.Default.GetString(buffer);
            lblInit.Content = "Invio:" + Start.Ticks;
            lblEnd.Content = "Ricezione:" + End.Ticks;
            lblTot.Content = "Ms Risposta:" + ((End.Ticks - Start.Ticks) / TimeSpan.TicksPerMillisecond);
        }
    }
}
