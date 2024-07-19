using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.IO;

namespace TCPSERVER
{
    public partial class Form1 : Form
    {
        Thread t1;
        int flag = 0;
        string receivedPath = "yok";
        public delegate void MyDelegate();
        public Form1()
        {
            t1 = new Thread(new ThreadStart(StartListening));
            t1.Start();
            InitializeComponent();
            //fcreate();
           
        }
        public static ManualResetEvent allDone = new ManualResetEvent(false); 
        public class StateObject
        {
            // Client socket.
            public Socket workSocket = null;

            public const int BufferSize = 1024;
            // Receive buffer.
            public byte[] buffer = new byte[BufferSize];
        }
        public void StartListening()
        {
            byte[] bytes = new Byte[1024];
            IPEndPoint ipEnd = new IPEndPoint(IPAddress.Any, 9050);
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listener.Bind(ipEnd);
                listener.Listen(100);
                while (true)
                {
                    allDone.Reset();
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                   
                    allDone.WaitOne();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        public void AcceptCallback(IAsyncResult ar)
        {

            allDone.Set();


            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);


            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
            new AsyncCallback(ReadCallback), state);

            flag = 0;
        }
        public void ReadCallback(IAsyncResult ar)
        {

            int fileNameLen = 1;
            String content = String.Empty;
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            int bytesRead = handler.EndReceive(ar);
            if (bytesRead > 0)
            {

                if (flag == 0)
                {
                    fileNameLen = BitConverter.ToInt32(state.buffer, 0);
                    string fileName = Encoding.UTF8.GetString(state.buffer, 4, fileNameLen);
                    receivedPath = "D:\\Filetrans\\" + fileName+".wmv";
                    flag++;
                }
                if (flag >= 1)
                {
                    BinaryWriter writer = new BinaryWriter(File.Open(receivedPath, FileMode.Append));
                    if (flag == 1)
                    {
                        writer.Write(state.buffer, 4 + fileNameLen, bytesRead - (4 + fileNameLen));
                        flag++;
                    }
                    else
                        writer.Write(state.buffer, 0, bytesRead);
                    writer.Close();
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }

            }
            else
            {
                Invoke(new MyDelegate(LabelWriter));
            }

        }
        public void LabelWriter()
        {
            fcreate();
            label2.Text = "DOWNLOAD FILE";
        }
        void fcreate()
        {
            receivedPath = "D:\\Filetrans";
            if (!System.IO.Directory.Exists("D:\\Filetrans"))
            {
                System.IO.Directory.CreateDirectory("D:\\Filetrans"); 

            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            //label1.Text = "Server Started";
            //timer1.Enabled = true;
        }
        int p = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            p += 20;
            progressBar1.Value = p;
            if (progressBar1.Value == 20)
            {
                label1.Text = "Connection Establishing";
            }
            if (progressBar1.Value == 40)
            {
                label1.Text = "Listening on Port";
            }
            if (progressBar1.Value == 60)
            {
                label1.Text = "Search Request";
            }
            if (progressBar1.Value == 80)
            {
                label1.Text = "Response to Client";
            }
            if (progressBar1.Value == 100)
            {
                timer1.Enabled = false;
                MessageBox.Show("connected");
            }
          

           
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            fcreate();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Feedback gg = new Feedback();
            //gg.Show();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            label1.Text = "Server Started";
            timer1.Enabled = true;
        }

       
    }
}
