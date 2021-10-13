using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

namespace PingServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BackgroundWorker backgroundWorker;
        private List<ReplyStatus> replies = new List<ReplyStatus>();
        private int port = 5001;

        public MainWindow()
        {
            InitializeComponent();
            backgroundWorker = FindResource("backgroundWorker") as BackgroundWorker;

            stop.IsEnabled = false;
        }

        private void start_Click(object sender, RoutedEventArgs e)
        {

            if (!string.IsNullOrWhiteSpace(hostname.Text))
            {
                backgroundWorker.RunWorkerAsync(hostname.Text);
                start.IsEnabled = false;
                stop.IsEnabled = true;
            }
        }

        private void stop_Click(object sender, RoutedEventArgs e)
        {
            stop.IsEnabled = false;
            backgroundWorker.CancelAsync();
            Thread.Sleep(5000);
            start.IsEnabled = true;
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Ping ping = new Ping();
            PingReply pingReply;
            string hostname = e.Argument as string;
            bool isUnavailable = false;

            while (true)
            {
                if (backgroundWorker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                try
                {
                    pingReply = ping.Send(hostname);

                    AddReply(hostname, pingReply.Status.ToString());
                    Thread.Sleep(5000);
                }
                catch (PingException)
                {
                    isUnavailable = true;
                    //AddReply(hostname, "Failed");
                    for (int i = 0; i < 12; i++)
                    {
                        try
                        {
                            pingReply = ping.Send(hostname);
                            AddReply(hostname, pingReply.Status.ToString());
                            isUnavailable = false;
                        }
                        catch (PingException)
                        {
                            AddReply(hostname, "Failed");
                        }
                        Thread.Sleep(5000);
                    }
                }
                if (isUnavailable)
                {
                    SendNotification(hostname);
                    backgroundWorker.CancelAsync();
                }
            }

        }

        private void AddReply(string hostname, string status)
        {
            replies.Add(new ReplyStatus { Resource = hostname, Status = status });
            ReplyList.Dispatcher.Invoke(() => ReplyList.Items.Add(replies.Last()));
        }

        private void SendNotification(string hostname)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, port);
            using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

                string msg = $"Resource {hostname} is currently unavailable";
                byte[] byteMsg = Encoding.UTF8.GetBytes(msg);

                client.SendTo(byteMsg, endPoint);
            }
        }
    }
}
