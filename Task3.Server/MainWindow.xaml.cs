using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace Task3.Server
{

    public partial class MainWindow : Window
    {
        List<string> psstrings = new List<string>() { "good weather", "Hello", "Bye", "Good evening", "we'll find out in a week", "yesterday was a beautiful day", "it's time for us to say goodbye", "see you tomorrow", "let's call you" };
        Socket socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Socket socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

       
        public MainWindow()
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            IPEndPoint ep = new IPEndPoint(ip, 1024);
            socketServer.Bind(ep);
            socketServer.Listen(10);

            InitializeComponent();
            this.MinHeight = 300;
            this.MinWidth = 500;
            Task.Run(ReadAccept);
        }

        private void SendAnswer_Click(object sender, RoutedEventArgs e)
        {
            if (stringAnswer.Text == string.Empty && stringAnswer.Text == "Введите сообщение")
            {
                Chat.Items.Add("Введите сообщение!!!");
                return;
            }

            if (socketClient.Connected)
            {
                socketClient.Send(Encoding.UTF8.GetBytes(stringAnswer.Text));
                Chat.Items.Add($"Вы отправили в {DateTime.Now.ToString("HH:mm:ss")}: {stringAnswer.Text}");
                stringAnswer.Text = "Введите сообщение";

            }
            else
            {
                Chat.Items.Add("Нет соединения");
            }
        }


        private void stringAnswer_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (stringAnswer.Text == "Введите сообщение") stringAnswer.Text = string.Empty;
        }

        private void stringAnswer_LostFocus(object sender, RoutedEventArgs e)
        {
            if (stringAnswer.Text == string.Empty) stringAnswer.Text = "Введите сообщение";
        }

        private async void ReadAccept()
        {
            try
            {
                while (true)
                {
                    Socket sreadchat = await socketServer.AcceptAsync();
                    if (sreadchat != null)
                    {
                        byte[] buffer = new byte[1024];
                        string texts = "";
                        var result = await sreadchat.ReceiveAsync(buffer);
                        texts = Encoding.UTF8.GetString(buffer, 0, result);
                        string[] ipaddres = texts.Split("+");
                        IPAddress ipclient;
                        int port;
                        if (IPAddress.TryParse(ipaddres[0], out ipclient) && Int32.TryParse(ipaddres[1], out port))
                        {
                            IPEndPoint iPEndPoint = new IPEndPoint(ipclient, port);
                            socketClient.Connect(iPEndPoint);
                            if (socketClient.Connected)
                            {
                                socketClient.Send(Encoding.UTF8.GetBytes("Содинение установлено, выбирите режим общения"));
                                Dispatcher.Invoke(() => Chat.Items.Add("Содинение установлено"));

                                result = await sreadchat.ReceiveAsync(buffer);
                                texts = Encoding.UTF8.GetString(buffer, 0, result);

                                if (texts == "user")
                                {
                                    ReadUser(sreadchat);
                                }
                                else
                                {
                                    ReadPC(sreadchat);
                                }

                                socketClient.Shutdown(SocketShutdown.Both);
                                sreadchat.Shutdown(SocketShutdown.Both);
                                if (!socketClient.Connected) socketClient.Close();
                                if (!sreadchat.Connected)
                                {
                                    sreadchat.Close();
                                    Dispatcher.Invoke(() => Chat.Items.Add("Соединение закрыто"));
                                   
                                }
                            }
                        }
                        else
                        {
                            sreadchat.Send(Encoding.UTF8.GetBytes("Содинение не установлено"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => Chat.Items.Add(ex.Message));
            }
        }

        private void ReadUser(Socket socket)
        {
            try
            {
                while (true)
                {
                    if (Chat.Items[Chat.Items.Count - 1].ToString().IndexOf("Bye") > -1) return;
                    byte[] buffer = new byte[1024];
                    var result = socket.Receive(buffer);
                    string texts = Encoding.UTF8.GetString(buffer, 0, result);
                    Dispatcher.InvokeAsync(() =>
                    {
                        Chat.Items.Add($"Собеседник отправил в {DateTime.Now.ToString("HH:mm:ss")} : {texts}");
                    });
                    if (texts == "Bye") return;

                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => Chat.Items.Add(ex.Message));
            }
        }

        private void ReadPC(Socket socket)
        {
            try
            {
                while (true)
                {
                    Task.Delay(1000).Wait();
                    byte[] buffer = new byte[1024];
                    var result = socket.Receive(buffer);
                    string texts = Encoding.UTF8.GetString(buffer, 0, result);
                    Dispatcher.Invoke(() => Chat.Items.Add($"Собеседник отправил в {DateTime.Now.ToString("HH:mm:ss")} : {texts}"));
                    if (texts.Contains("Bye")) { return; }

                    Task.Delay(1000).Wait();
                    Random random = new Random();
                    string s = psstrings[random.Next(0, psstrings.Count() - 1)];
                    socketClient.Send(Encoding.UTF8.GetBytes(s));
                    Dispatcher.Invoke(() => Chat.Items.Add($"Вы отправили в {DateTime.Now.ToString("HH:mm:ss")}: {s}"));
                    if (s.Contains("Bye")) return;
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => Chat.Items.Add(ex.Message));
            }
        }
    }
}