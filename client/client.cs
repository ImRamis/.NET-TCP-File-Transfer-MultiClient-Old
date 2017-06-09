#region

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using rho;

#endregion

namespace client
{
    public partial class Client : Form
    {
        private SocketClient _client;

        public Client()
        {
            InitializeComponent();
            FileList = new List<string>();
        }

        public List<string> FileList { get; set; }

        private void button1_Click(object sender, EventArgs e)
        {
            _client = new SocketClient(textBox1.Text, int.Parse(textBox2.Text)) {Listboxui = listBox1};
            _client.Connect();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
                _client.DownloadFile(listBox1.SelectedItem + "");
        }

        private void client_FormClosed(object sender, FormClosedEventArgs e)
        {
            _client?.Disconnect();
            Environment.Exit(1);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (var filebrowser = new OpenFileDialog {Multiselect = true})
            {
                if (filebrowser.ShowDialog() != DialogResult.OK) return;
                foreach (var item in filebrowser.FileNames)
                    _client.UploadFile(item);
            }
        }
    }
}