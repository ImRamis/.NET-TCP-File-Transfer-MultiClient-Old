#region

using System;
using System.Windows.Forms;
using rho;

#endregion

namespace Proj
{
    public partial class Server : Form
    {
        private SocketServer _serv;

        public Server()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _serv = new SocketServer(Convert.ToInt32(numericUpDown1.Value)) {Listboxui = listBox1};
            _serv.StartServer();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _serv.StopServer();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (var fileBrowser = new OpenFileDialog {Multiselect = true})
            {
                if (fileBrowser.ShowDialog() == DialogResult.OK)
                    foreach (var item in fileBrowser.FileNames)
                        if (!listBox1.Items.Contains(item))
                            listBox1.Items.Add(item);
            }
            _serv?.SendUpdateListToAll();
        }

        private void server_FormClosed(object sender, FormClosedEventArgs e)
        {
            _serv?.StopServer();
            Environment.Exit(1);
        }
    }
}