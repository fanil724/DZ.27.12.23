using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace Task2.Client
{
    public partial class MainWindow : Window
    {
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ip = IPAddress.Parse("127.0.0.1");

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            string login = LogginUser.Text;
            string password = PassUser.Text;
            if (login == string.Empty || password == string.Empty)
            {
                outputQuotes.Items.Add("Заполните поля логина и пароля!!");
            }
            else
            {
                outputQuotes.Items.Add(ConnectServer(login, password));
            }
        }

        private void request_Click(object sender, RoutedEventArgs e)
        {
            string s = GetRequest();
            outputQuotes.Items.Add(s);
        }

        private string ConnectServer(string login, string password)
        {
            IPEndPoint ep = new IPEndPoint(ip, 1024);
            string otvet = "";
            try
            {
                socket.Connect(ep);
                if (socket.Connected)
                {
                    socket.Send(System.Text.Encoding.UTF8.GetBytes($"{login}+{password}"));
                    byte[] buffer = new byte[512];
                    int l = socket.Receive(buffer);
                    otvet = System.Text.Encoding.UTF8.GetString(buffer, 0, l);
                }
                return otvet;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                if (!socket.Connected) { socket.Close(); }
            }
        }

        private string GetRequest()
        {
            if (!socket.Connected) { return "Нет соединения"; }
            string sget = "Get quotes";
            try
            {
                byte[] beData = Encoding.UTF8.GetBytes(sget);
                socket.Send(beData);
                byte[] buffer = new byte[512];
                int l = socket.Receive(buffer);
                string data = Encoding.UTF8.GetString(buffer, 0, l);
                if (data.Contains("Соединение закрыто"))
                {
                    socket.Shutdown(SocketShutdown.Both);
                    if (!socket.Connected) socket.Close();
                }
                return data;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }
    }
}