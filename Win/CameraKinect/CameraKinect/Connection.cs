using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Xml;
using System.Xml.Linq;

namespace CameraKinect
{
    static class Protocol
    {
        public static readonly byte[] SU = Encoding.ASCII.GetBytes("SU");
        public static readonly byte[] GIU = Encoding.ASCII.GetBytes("GIU");
        public static readonly byte[] JUMP = Encoding.ASCII.GetBytes("JUMP");
    }

    public class Connection
    {
        string host = "192.168.0.15";
        int port = 1313;
        WriteInfo w;
        public bool _isConnect = false;
        Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        bool canSend = true;
        Thread t;
        MainWindow wnd = null;

        public Connection(WriteInfo wr,MainWindow _wnd)
        {
            w = wr;
            if(w != null) w.setTextConn("PI Non Connesso");
            wnd = _wnd;
            t = new Thread(doWork);
            var doc = XDocument.Load("conf.xml");
            var ips = doc.Descendants("ip");
            var ports = doc.Descendants("port");

            foreach (var ip in ips)
            {
                host = ip.Value;
            }
            foreach (var p in ports)
            {
                port = Convert.ToInt32(p.Value);
            }
        }

        public void Ping(byte[] buffer)
        {
            if (s.Connected)
            {
                s.Send(Protocol.JUMP, SocketFlags.None);
                s.Receive(buffer);
            }
        }

        public bool isConnect()
        {
            return s.Connected;
        }
        public void Dispose()
        {
            s.Close();
            t.Abort();
        }

        public bool Connect()
        {
            try
            {
                IPAddress[] IPs = Dns.GetHostAddresses(host);
                s.Connect(IPs[0], port);
                if (w != null) w.setTextConn("PI Connesso");
                this._isConnect = true;
            }
            catch (Exception)
            {
                if (w != null) w.setTextConn("PI Non Connesso");
                this._isConnect = false;
                return false;
            }
            return true;
        }

        public void sendData()
        {
            if (canSend)
            {
                if (!s.Connected) { return; }
                canSend = false;
                s.Send(Protocol.JUMP, SocketFlags.None);
                t = new Thread(doWork);
                t.Start();
            }
        }

        void doWork()
        {
            Thread.Sleep(1000);
            canSend = true;
        }

        public void sendUp(int step)
        {
            if (!s.Connected) { return; }
            if (canSend)
            {
                canSend = false;
                s.Send(Encoding.ASCII.GetBytes("Step:" + step), SocketFlags.None);
                s.Send(Protocol.SU, SocketFlags.None);
                t = new Thread(doWork);
                t.Start();
                wnd.incrementCounter();
            }
            
        }
        public void sendDown(int step)
        {
            if (!s.Connected) { return; }
            if (canSend)
            {
                canSend = false;
                s.Send(Encoding.ASCII.GetBytes("Step:" + step), SocketFlags.None);
                s.Send(Protocol.GIU, SocketFlags.None);
                t = new Thread(doWork);
                t.Start();
            }
        }
    }
}
