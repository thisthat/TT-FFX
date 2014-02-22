using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CameraKinect
{
    public class WriteInfo
    {
        TextBlock status;
        string infoCamera = "";
        string infoConn = "";

        public WriteInfo(TextBlock t)
        {
            status = t;
        }

        public void setTextCamera(string t)
        {
            infoCamera = t;
            status.Text = infoConn + " :: " + infoCamera;
        }
        public void setTextConn(string t)
        {
            infoConn = t;
            status.Text = infoConn + " :: " + infoCamera;
        }
    }
}
