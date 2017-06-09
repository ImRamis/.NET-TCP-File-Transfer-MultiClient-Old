#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

#endregion

namespace rho
{
    public class SocketServer
    {
        private readonly Thread _accepter;
        public readonly string FileSavePath = "Shared";
        private readonly List<Socket> _clients;
        private readonly Socket _listener;

        public SocketServer(int port)
        {
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _clients = new List<Socket>();
            _listener.Bind(new IPEndPoint(IPAddress.Any, port));
            _accepter = new Thread(ClientAccepter);
        }

        public ListBox Listboxui { get; set; }

        public void StartServer()
        {
            _listener.Listen(10);
            _accepter.Start();
        }

        public void StopServer()
        {
            foreach (var item in _clients)
                item.Close();
            _listener.Close();
        }

        public void SendUpdateListToAll()
        {
            foreach (var item in _clients)
                SendUpdateFileList(item);
        }

        public void SendUpdateFileList(Socket item)
        {
            var client = new NetworkStream(item);
            var writter = new StreamWriter(client);
            foreach (var filepath in Listboxui.Items)
            {
                writter.WriteLine((int) ServiceType.FilePath + filepath.ToString());
                writter.Flush();
            }
            writter.Close();
            client.Close();
        }

        private void ClientAccepter()
        {
            while (true)
            {
                var client = _listener.Accept();
                _clients.Add(client);
                SendUpdateFileList(client);
                var clientThread = new Thread(()
                    =>
                        ClientWorkProcessor(client));
                clientThread.Start();
            }
        }

        private void ClientWorkProcessor(Socket client)
        {
            var stream = new NetworkStream(client);
            var reader = new StreamReader(stream);
            var writter = new StreamWriter(stream);
            var receivedFileName = "";
            while (true)
            {
                var received = reader.ReadLine();
                var code = received[0];
                var data = received.Substring(1, received.Length - 1);
                switch (code)
                {
                    case '0':
                        receivedFileName = data;
                        break;
                    case '1':
                        SendRequestedFile(writter, data);
                        break;
                    case '2':
                        SaveReceivedFile(receivedFileName, data);
                        break;
                }
                Thread.Sleep(1000);
            }
        }

        private static void SendRequestedFile(TextWriter writter, string data)
        {
            var file = File.ReadAllBytes(data);
            writter.WriteLine((int) ServiceType.UploadServer + Convert.ToBase64String(file));
            writter.Flush();
        }

        private void SaveReceivedFile(string receivedFileName, string data)
        {
            File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + "\\Shared\\" + receivedFileName,
                Convert.FromBase64String(data));
            var abs = Path.GetFullPath(FileSavePath + "/" + receivedFileName);
            Listboxui.Invoke(new Action(() =>
            {
                if (!Listboxui.Items.Contains(abs)) Listboxui.Items.Add(abs);
            }));
            SendUpdateListToAll();
        }
    }
}