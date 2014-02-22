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
    public partial class DebugWindow : Window
    {
        byte[] colorImage;
        byte[] debugImage;
        int lineSize;
        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap colorBitmap;
        Int32Rect rect;
        int mX, mY;

        private int score = 0;
        private WriteInfo wrInfo;
        private Connection pi;

        public DebugWindow(byte[] img,int w, int h,WriteInfo txtInfo,Connection c)
        {
            InitializeComponent();
            pi = c;
            colorImage = img;
            lineSize = w;
            wrInfo = txtInfo;
            // This is the bitmap we'll display on-screen
            this.colorBitmap = new WriteableBitmap(w, h, 96.0, 96.0, PixelFormats.Bgr32, null);
            imgDebug.Source = colorBitmap;
        }

        public void setRect(Int32Rect r)
        {
            this.rect = r;
            this.colorBitmap = new WriteableBitmap(rect.Width, rect.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
            imgDebug.Source = colorBitmap;
            mX = rect.X + rect.Width;
            mY = rect.Y + rect.Height;
            debugImage = new byte[rect.Width * rect.Height * 4];
            
        }

        public void reDraw(byte[] colorImage){
            score = 0;
            if (!rect.IsEmpty)
            {
                int k = 0;
                score = 0;
                for (int i = rect.Y; i < mY; i++)
                {
                    for (int j = rect.X; j < mX; j++)
                    {
                        var index = i * lineSize + j;
                        index *= 4;
                        debugImage[k] = colorImage[index];
                        debugImage[k+1] = colorImage[index+1];
                        debugImage[k+2] = colorImage[index+2];
                        debugImage[k+3] = colorImage[index+3];
                        //Valutazione score
                        if (debugImage[k] > 0xBB && debugImage[k + 1] > 0xBB && debugImage[k + 2] > 0xBB)
                        {
                            score++;
                        }
                        else
                        {
                            score--;
                        }
                        
                        k += 4;
                    }
                }
                this.colorBitmap.WritePixels(
                    new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                    this.debugImage,
                    this.colorBitmap.PixelWidth * sizeof(int),
                    0);
                float scoreFinal = (float)score / (float)(rect.Width * rect.Height);
                if (scoreFinal > 0)
                {
                    pi.sendData();
                    wrInfo.setTextCamera("Kinect Connected :: Stream Open :: Fulmine Rilevato : " + scoreFinal.ToString());
                }
                else
                {
                    wrInfo.setTextCamera("Kinect Connected :: Stream Open : " + scoreFinal.ToString());
                }
            }
            else
            {
                // Write the pixel data into our bitmap
                this.colorBitmap.WritePixels(
                    new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                    this.colorImage,
                    this.colorBitmap.PixelWidth * sizeof(int),
                    0);
            }
            
        }
    }
}
