using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace DZ._27._12._23
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void GetDate_Click(object sender, RoutedEventArgs e)
        {
            string sget = "Get Date";
            WorkSocket(sget);
        }

        private void GetTime_Click(object sender, RoutedEventArgs e)
        {
            string sget = "Get Time";
            WorkSocket(sget);
        }

        private void Welcome_Click(object sender, RoutedEventArgs e)
        {
            string sget = "Привет, сервер!";
            WorkSocket(sget);
        }

        private async void WorkSocket(string s)
        {
            try
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                IPAddress ip = IPAddress.Parse("127.0.0.1");
                IPEndPoint ep = new IPEndPoint(ip, 1024);
                await socket.ConnectAsync(ep);
                Otschet.Items.Add($"Соединенине с {ip} установлено");
                IPEndPoint iPEnd = (IPEndPoint)socket.RemoteEndPoint;
                await socket.SendToAsync(System.Text.Encoding.UTF8.GetBytes(s), iPEnd);
                Otschet.Items.Add("Запрос отправлен");
                byte[] buffer = new byte[1024];
                string texts = "";
                var result = await socket.ReceiveAsync(buffer);
                texts = Encoding.UTF8.GetString(buffer, 0, result);
                Otschet.Items.Add($"В {DateTime.Now.ToString("HH:mm:ss")} от [{iPEnd.Address}] получена строка {texts}");
                socket.Shutdown(SocketShutdown.Both);
                if (!socket.Connected) { socket.Close(); }
            }
            catch (Exception ex)
            {
                Otschet.Items.Add(ex.Message);
            }
        }
    }
}