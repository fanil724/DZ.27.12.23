using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace Task2
{
    public partial class MainWindow : Window
    {
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
        static CancellationTokenSource cancellation = new CancellationTokenSource();
        CancellationToken token = cancellation.Token;

        public MainWindow()
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            IPEndPoint ep = new IPEndPoint(ip, 1024);
            socket.Bind(ep);
            socket.Listen(10);
            InitializeComponent();
            Task.Run(ConnectAccept);
        }

        private void CanselConn_Click(object sender, RoutedEventArgs e)
        {
            socket.Shutdown(SocketShutdown.Both);
            cancellation.Cancel();
            if (!socket.Connected)
            {
                Otschet.Items.Add("Соединение разорвано");
                socket.Close();
            }
        }

        private async void ConnectAccept()
        {
            try
            {
                while (true)
                {
                    if (token.IsCancellationRequested) return;
                    Socket s = await socket.AcceptAsync();
                    if (s != null)
                    {
                        byte[] buffer = new byte[1024];
                        string texts = "";
                        string otvet = "";
                        IPEndPoint iPEnd = (IPEndPoint)s.RemoteEndPoint;
                        var result = await s.ReceiveAsync(buffer);
                        texts = Encoding.UTF8.GetString(buffer, 0, result);
                        Dispatcher.Invoke(() => Otschet.Items.Add($"В {DateTime.Now.ToString("HH:mm:ss")} от [{iPEnd.Address}] получена строка {texts}"));
                        switch (texts)
                        {
                            case "Привет, сервер!": { otvet = "Привет, клиент!"; } break;
                            case "Get Time": { otvet = $"Текущее время {DateTime.Now.ToString("HH:mm:ss")}"; } break;
                            case "Get Date": { otvet = $"Текущее дата {DateTime.Now.ToString("dd MMMM yyyy")}"; } break;
                            default: otvet = "НЕ корректный запрос!!"; break;
                        }
                        await s.SendAsync(System.Text.Encoding.UTF8.GetBytes(otvet));
                        s.Shutdown(SocketShutdown.Both);
                        s.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => Otschet.Items.Add(ex.Message));
            }
        }
    }
}