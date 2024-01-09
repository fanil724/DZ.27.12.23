using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace Task3.Client
{
    public partial class MainWindow : Window
    {
        List<string> psstrings = new List<string>() { "good weather", "Hello", "Bye", "Good evening", "we'll find out in a week", "yesterday was a beautiful day", "it's time for us to say goodbye", "see you tomorrow", "let's call you" };
        Socket socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Socket socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Socket sreadchat = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        string adres = "127.0.0.154";
        int portServer = 888;

      

        public MainWindow()
        {
            IPAddress ip = IPAddress.Parse(adres);
            IPEndPoint ep = new IPEndPoint(ip, portServer);
            socketServer.Bind(ep);
            socketServer.Listen(10);
            InitializeComponent();
            this.MinHeight = 300;
            this.MinWidth = 500;

            Task.Run(() =>
            {
                Task<Socket?> s1 = ReadAccept();
                if (s1.Result != null) { sreadchat = s1.Result; }
                else
                {
                    Chat.Items.Add("Соединение не установлено!");
                }
            });
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

        private void Connnect_Click(object sender, RoutedEventArgs e)
        {
            if (IPaddress.Text == string.Empty || PORT.Text == string.Empty)
            {
                Chat.Items.Add("Введеите адресс и порт подключения");
                return;
            }

            try
            {
                IPAddress pAddress;
                int port;
                if (IPAddress.TryParse(IPaddress.Text, out pAddress) && Int32.TryParse(PORT.Text, out port))
                {
                    IPEndPoint pPEndPoint = new IPEndPoint(pAddress, port);
                    socketClient.Connect(pPEndPoint);
                    socketClient.Send(Encoding.UTF8.GetBytes($"{adres}+{portServer}"));
                }
                else
                {
                    Chat.Items.Add("Некорректный адресс и порт подключения");
                    return;
                }
            }
            catch (Exception ex)
            {
                Chat.Items.Add(ex.Message);
            }
        }

        private void User_User_Click_1(object sender, RoutedEventArgs e)
        {
            socketClient.Send(Encoding.UTF8.GetBytes("user"));
            Task.Run(() => { ReadUser(sreadchat); });
        }

        private void User_PC_Click_2(object sender, RoutedEventArgs e)
        {
            socketClient.Send(Encoding.UTF8.GetBytes("PC"));
            Task.Run(() => { ReadUser(sreadchat); });
        }

        private void PC_User_Click_3(object sender, RoutedEventArgs e)
        {
            socketClient.Send(Encoding.UTF8.GetBytes("user"));
            ReadPC(sreadchat);
        }

        private void PC_PC_Click_4(object sender, RoutedEventArgs e)
        {
            socketClient.Send(Encoding.UTF8.GetBytes("PC"));
            ReadPC(sreadchat);
        }

        private async Task<Socket?> ReadAccept()
        {
            try
            {
                while (true)
                {
                    Socket sreadchat = await socketServer.AcceptAsync();
                    if (sreadchat != null)
                    {
                        byte[] buffer = new byte[1024];
                        var result = await sreadchat.ReceiveAsync(buffer);
                        string texts = Encoding.UTF8.GetString(buffer, 0, result);
                        Dispatcher.Invoke(() => Chat.Items.Add(texts));
                        return sreadchat;
                    }
                }
            }
            catch
            {
                return null;
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
                    Dispatcher.InvokeAsync(() =>                    {                     

                        Chat.Items.Add($"Собеседник отправил в {DateTime.Now.ToString("HH:mm:ss")} : {texts}");
                    });


                    
                    if (texts == "Bye") return;
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => Chat.Items.Add(ex.Message));
            }
            finally { CloseSocket(socket); }

        }

        private async void ReadPC(Socket socket)
        {
            try
            {
                while (true)
                {
                    Task.Delay(1000).Wait();
                    Random random = new Random();
                    string s = psstrings[random.Next(0, psstrings.Count() - 1)];
                    await socketClient.SendAsync(Encoding.UTF8.GetBytes(s));
                    Dispatcher.Invoke(() => Chat.Items.Add($"Вы отправили в {DateTime.Now.ToString("HH:mm:ss")}: {s}"));
                    if (s.Contains("Bye")) { return; }

                    Task.Delay(1000).Wait();
                    byte[] buffer = new byte[1024];
                    var result = await socket.ReceiveAsync(buffer);
                    string texts = Encoding.UTF8.GetString(buffer, 0, result);
                    Dispatcher.Invoke(() => Chat.Items.Add($"Собеседник отправил в {DateTime.Now.ToString("HH:mm:ss")} : {texts}"));
                    if (texts.Contains("Bye")) return;
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => Chat.Items.Add(ex.Message));
            }
            finally { CloseSocket(socket); }

        }

        private void CloseSocket(Socket socket)
        {
            socketClient.Shutdown(SocketShutdown.Both);
            socket.Shutdown(SocketShutdown.Both);
            if (!socketClient.Connected) socketClient.Close();
            if (!socket.Connected)
            {
                socket.Close();
                Dispatcher.Invoke(() => Chat.Items.Add("Соединение закрыто"));               
            }

        }

    }
}