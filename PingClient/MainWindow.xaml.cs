using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.Net;
using Windows.UI.Notifications;
using System.Threading;
using Windows.Data.Xml.Dom;
using System.ComponentModel;

namespace PingClient
{

    public partial class MainWindow : Window
    {
        private BackgroundWorker backgroundWorker;
        private static int port = 5001;
        private static IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
        public MainWindow()
        {
            InitializeComponent();

            backgroundWorker = FindResource("backgroundWorker") as BackgroundWorker;

            NotifyIcon ni = new NotifyIcon();
            ni.Icon = new Icon(@"C:\Users\hck\source\repos\PingClient\PingClient\notify.ico");
            ni.Visible = true;
            ni.DoubleClick +=
                delegate (object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                    backgroundWorker.CancelAsync();
                };
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
                Hide();

            base.OnStateChanged(e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
            backgroundWorker.RunWorkerAsync();

        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            IPAddress ip = IPAddress.Parse("192.168.110.139");
            int port = 5001;
            using (Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                server.Bind(endPoint);
                while (true)
                {
                    if (backgroundWorker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    byte[] msg = new byte[1024];
                    int bytesReceived = server.Receive(msg);
                    string notification = Encoding.UTF8.GetString(msg, 0, bytesReceived);
                    DisplayNotification(notification);
                }
            }
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            WindowState = WindowState.Normal;
        } 

        void DisplayNotification(string message)
        {
            var xml = $"<?xml version=\"1.0\"?><toast><visual><binding template=\"ToastText01\"><text id=\"1\">{message}</text></binding></visual></toast>";
            var toastXml = new XmlDocument();
            toastXml.LoadXml(xml);
            var notification = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier("Notification").Show(notification);
        }
    }
}
