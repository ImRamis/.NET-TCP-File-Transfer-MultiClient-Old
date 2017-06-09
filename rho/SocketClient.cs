#region

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

#endregion

namespace rho
{
    public class SocketClient
    {
        private readonly Thread _receiver;
        public string FileSavePath = "";
        private readonly string _ip;
        private readonly int _port;
        public Socket Client;
        private StreamReader _reader;
        private string _requestedFile = "";
        private NetworkStream _stream;
        private StreamWriter _writter;

        public SocketClient(string ip, int port)
        {
            _ip = ip;
            _port = port;
            Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _receiver = new Thread(Receieve);
        }

        public ListBox Listboxui { get; set; }

        public void Connect()
        {
            Client.Connect(IPAddress.Parse(_ip), _port);
            _receiver.Start();
        }

        public void UploadFile(string file)
        {
            _writter.WriteLine((int) ServiceType.FilePath + file.Split('\\').Last());
            _writter.Flush();
            var filedata = File.ReadAllBytes(file);
            _writter.WriteLine((int) ServiceType.UploadClient + Convert.ToBase64String(filedata));
            _writter.Flush();
        }

        public void DownloadFile(string file)
        {
            _requestedFile = file.Split('\\').Last();
            _writter.WriteLine(file);
            _writter.Flush();
        }

        public void Receieve()
        {
            _stream = new NetworkStream(Client);
            _reader = new StreamReader(_stream);
            _writter = new StreamWriter(_stream);

            while (true)
            {
                var received = _reader.ReadLine();
                var code = received[0];
                var data = received.Substring(1, received.Length - 1);
                switch (received[0])
                {
                    case '0':
                        Listboxui.Invoke(new Action(() =>
                        {
                            if (!Listboxui.Items.Contains(data)) Listboxui.Items.Add(data);
                        }));
                        break;
                    case '1':
                        File.WriteAllBytes(FileSavePath + "\\" + _requestedFile, Convert.FromBase64String(data));
                        break;
                    case '2':
                        break;
                }
                Thread.Sleep(500);
            }
        }

        public void Disconnect()
        {
            _reader.Close();
            _writter.Close();
            Client.Disconnect(true);
        }
    }
}