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
using Microsoft.Kinect;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CameraKinect
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap colorBitmap;

        /// <summary>
        /// Intermediate storage for the color data received from the camera
        /// </summary>
        private byte[] colorPixels;
        private byte[] colorPixelsFlip;

        //Variabili per il disegno
        Point initPoint = new Point();
        Point endPoint = new Point();
        private Int32Rect rect = new Int32Rect();
        Rectangle rectangle = new Rectangle();

        //Debug Win
        DebugWindow debugWin;
        //Tool Win
        ToolWindow toolWin;

        //Info
        WriteInfo wrInfo;

        //Rasp
        Connection pi;

        //Thread button
        Thread t;

        public MainWindow()
        {
            InitializeComponent();
            wrInfo = new WriteInfo(statusBarText);
            pi = new Connection(wrInfo);
            t = new Thread(swapButton);
            t.Start();
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                // Turn on the color stream to receive color frames
                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                // Allocate space to put the pixels we'll receive
                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];
                this.colorPixelsFlip = new byte[this.sensor.ColorStream.FramePixelDataLength];

                // This is the bitmap we'll display on-screen
                this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
                // Set the image we display to point to the bitmap where we'll put the image data
                this.Image.Source = this.colorBitmap;

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.ColorFrameReady += this.SensorColorFrameReady;

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                    debugWin = new DebugWindow(colorPixels, this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, wrInfo, pi);
                    toolWin = new ToolWindow(pi,sensor);
                    attachWin();
                    debugWin.Show();
                    toolWin.Show();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                wrInfo.setTextCamera("Kinect Not Ready");
            }
            else
            {
                wrInfo.setTextCamera("Kinect Connected :: Stream Open");
            }
        }

        private void attachWin()
        {
            if (debugWin != null)
            {
                debugWin.Left = this.Left + this.Width;
                debugWin.Top = this.Top;
            }
            if (toolWin != null)
            {
                toolWin.Left = this.Left + this.Width;
                toolWin.Top = this.Top + (this.Height / 2);
            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's ColorFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    if ((bool)ckFlip.IsChecked)
                    {
                        // Copy the pixel data from the image to a temporary array
                        colorFrame.CopyPixelDataTo(this.colorPixelsFlip);
                        var w = this.sensor.ColorStream.FrameWidth;
                        var h = this.sensor.ColorStream.FrameHeight;
                        int index;
                        int lastLine;
                        int firstLine;
                        int flip;

                        for (int y = 0; y < h; y++)
                        {
                            for (int x = 0; x < w; x++)
                            {
                                index = y * w + x;
                                lastLine = y * w + (w - 1);
                                firstLine = y * w;
                                flip = lastLine - index + firstLine;
                                index *= 4;
                                flip *= 4;
                                colorPixels[index + 0] = colorPixelsFlip[flip + 0];
                                colorPixels[index + 1] = colorPixelsFlip[flip + 1];
                                colorPixels[index + 2] = colorPixelsFlip[flip + 2];
                                colorPixels[index + 3] = colorPixelsFlip[flip + 3];
                            }
                        }
                        // Write the pixel data into our bitmap
                        this.colorBitmap.WritePixels(
                            new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                            this.colorPixels,
                            this.colorBitmap.PixelWidth * sizeof(int),
                            0);

                    }
                    else
                    {
                        // Copy the pixel data from the image to a temporary array
                        colorFrame.CopyPixelDataTo(this.colorPixels);
                        // Write the pixel data into our bitmap
                        this.colorBitmap.WritePixels(
                            new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                            this.colorPixels,
                            this.colorBitmap.PixelWidth * sizeof(int),
                            0);
                    }


                    debugWin.reDraw(colorPixels);
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
            if (debugWin != null) { debugWin.Close(); }
            if (toolWin != null) { toolWin.Close(); }

            pi.Dispose();

            t.Abort();
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                initPoint = e.GetPosition(this);
                // initPoint.X -= xDelta;
                // initPoint.Y -= yDelta;
            }
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                tela.Children.Remove(rectangle);
                endPoint = e.GetPosition(this);
                //  endPoint.X -= xDelta;
                // endPoint.Y -= yDelta;
                var w = Math.Abs(endPoint.X - initPoint.X);
                var h = Math.Abs(endPoint.Y - initPoint.Y);
                rectangle = new Rectangle();
                rectangle.Width = w;
                rectangle.Height = h;
                rectangle.Stroke = new SolidColorBrush(System.Windows.Media.Colors.Red);
                rectangle.StrokeThickness = 2.0;
                var x = Math.Min(initPoint.X, endPoint.X);
                var y = Math.Min(initPoint.Y, endPoint.Y);
                tela.Children.Add(rectangle);
                Canvas.SetLeft(rectangle, x);
                Canvas.SetTop(rectangle, y);
            }
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            rect.X = (Int32)Math.Min(initPoint.X, endPoint.X);
            rect.Y = (Int32)Math.Min(initPoint.Y, endPoint.Y);
            rect.Width = (Int32)Math.Abs(endPoint.X - initPoint.X);
            rect.Height = (Int32)Math.Abs(endPoint.Y - initPoint.Y);
            if (debugWin != null && rect.Width > 30 && rect.Height > 30)
                debugWin.setRect(rect);
        }

        private void swapButton()
        {
            while (true)
            {
                Dispatcher.BeginInvoke(new Action(() => { swapButtonDispatcher(); }));
                Thread.Sleep(500);
            }

        }

        private void swapButtonDispatcher()
        {
            if (sensor == null)
            {
                btnOpen.Visibility = System.Windows.Visibility.Visible;
                btnOpenPi.Visibility = System.Windows.Visibility.Collapsed;
            }
            else if (sensor != null && !pi.isConnect())
            {
                btnOpen.Visibility = System.Windows.Visibility.Collapsed;
                btnOpenPi.Visibility = System.Windows.Visibility.Visible;
                ckFlip.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                btnOpen.Visibility = System.Windows.Visibility.Collapsed;
                btnOpenPi.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void btnOpenPi_Click(object sender, RoutedEventArgs e)
        {
            pi.Connect();
        }


        private void Window_LocationChanged(object sender, EventArgs e)
        {        
                attachWin();
        }

    }
}
