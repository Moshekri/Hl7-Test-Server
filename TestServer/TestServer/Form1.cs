using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace TestServer
{
    public partial class Form1 : Form
    {
        CancellationTokenSource tokenSource;
        TcpListener server;
        System.Timers.Timer timer;
        IPEndPoint localEndPoint;
        CancellationToken cancelServer;
        bool serverStarted = false;

        public Form1()
        {
            timer = new System.Timers.Timer(200);
            InitializeComponent();
            btnStop.Enabled = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            var adressess = Dns.GetHostAddresses("localHost");
            foreach (var add in adressess)
            {
                if (add.AddressFamily == AddressFamily.InterNetwork)
                {
                    cmbLocalIp.Items.Add(add);
                    cmbLocalIp.SelectedIndex = 0;
                }
            }

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            localEndPoint = new IPEndPoint((IPAddress)cmbLocalIp.SelectedItem, int.Parse(txtPort.Text));
            server = new TcpListener(localEndPoint);
            tokenSource = new CancellationTokenSource();
            cancelServer = tokenSource.Token;
            try
            {
                server.Start();
                serverStarted = true;
                serverStarted = true;
                timer.Elapsed += Timer_Elapsed;
                timer.Start();
                LogEntry("Server Started ");
                ChangeButtonState(btnStop, true);
                ChangeButtonState(btnStart, false);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }


        }
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            byte[] buffer = new byte[50000];
            StringBuilder sb = new StringBuilder();
            if (!cancelServer.IsCancellationRequested)
            {
                //  LogEntry(server.Pending().ToString());
                if (server.Pending())
                {
                    var client = server.AcceptTcpClient();
                    var stream = client.GetStream();
                    if (stream.DataAvailable)
                    {
                        int amountRead = 0;
                        do
                        {
                            amountRead = stream.Read(buffer, 0, buffer.Length);
                            sb.Append(Encoding.ASCII.GetString(buffer));
                        } while (amountRead == buffer.Length);
                    }
                }
                else
                {
                    return;
                }
                var message = sb.ToString().TrimEnd(new char[] { '\0' });
                LogEntry(message);
            }
            else
            {
                StopServer();
            }
        }
        private void StopServer()
        {

            server.Stop();
            serverStarted = false;
            LogEntry("Server Stopped");
            timer.Stop();
            timer.Elapsed -= Timer_Elapsed;
            ChangeButtonState(btnStop, false);
            ChangeButtonState(btnStart, true);


        }
        private void btnStop_Click(object sender, EventArgs e)
        {
            tokenSource.Cancel();


        }
        private void LogEntry(string text)
        {

            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() => LogEntry(text)));
            }
            else
            {
                txtLog.AppendText($"{text} {Environment.NewLine}");
                txtLog.Refresh();
            }

        }
        private void ChangeButtonState(Button b, bool state)
        {
            if (b.InvokeRequired)
            {
                b.Invoke(new Action(() => ChangeButtonState(b, state)));
            }
            else
            {
                b.Enabled = state;
            }
        }
        public string LongToIP(long longIP)
        {
            string ip = string.Empty;
            for (int i = 0; i < 4; i++)
            {
                int num = (int)(longIP / Math.Pow(256, (3 - i)));
                longIP = longIP - (long)(num * Math.Pow(256, (3 - i)));
                if (i == 0)
                    ip = num.ToString();
                else
                    ip = ip + "." + num.ToString();
            }
            return ip;
        }
        private void txtIpAddress_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnQuitProgram_Click(object sender, EventArgs e)
        {
            LogEntry("Checking server status ...");
            Thread.Sleep(250);
            if (serverStarted)
            {
                LogEntry("Stopping Server ...");
                Thread.Sleep(500);
                StopServer();
                Thread.Sleep(500);

            }
            else
            {
                LogEntry("Server Not running , Exiting ...");
                Thread.Sleep(250);
            }
            LogEntry("Exiting Application ...");

            Thread.Sleep(1500);
            Application.Exit();
        }

        private void btnSaveToFile_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.InitialDirectory = @"\Saves";
            var result = saveFile.ShowDialog();
            if (result == DialogResult.OK)
            {
                File.AppendAllText(saveFile.FileName, txtLog.Text);
            }
        }
    }
}
