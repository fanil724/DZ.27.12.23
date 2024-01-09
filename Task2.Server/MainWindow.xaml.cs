using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace Task2.Server
{
    public partial class MainWindow : Window
    {

        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
        List<string> strings = new List<string>() { "Цели никогда не должны быть простыми. Они должны быть неудобными, чтобы заставить вас работать",
        "Возраст — это всего лишь ограничение, которое вы кладёте себе в голову","Не бойтесь неудач, потому что это ваш путь к успеху",
        "Причина создаёт ограничения. Но вы можете сделать многое, если верите в это всем своим сердцем",
        "Тело может многое выдержать. Вам нужно только убедить своё сознание в этом", "Секрет жизни в том, чтобы семь раз упасть, но восемь раз подняться",
        "Неудача — это просто возможность начать снова, но уже более мудро","Величайшая слава в жизни заключается не в том, чтобы никогда не падать, а в том, чтобы подниматься каждый раз, когда мы падаем",
        "Успех — не окончателен, провал — не фатален: имеет значение лишь смелость продолжить путь",
        "Я могу смириться с неудачей. Все их испытывают в чём-то. Но я не могу смириться с тем, что не попробовал"};
        List<User> users = new List<User>() { new User("prof", "proffi"), new User("Test", "tester"), new User("server", "server") };
        Random rand = new Random();
        public MainWindow()
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            IPEndPoint ep = new IPEndPoint(ip, 1024);
            socket.Bind(ep);
            socket.Listen(10);
            InitializeComponent();
            Task.Run(ConnectAccept);
        }

        private async void ConnectAccept()
        {
            try
            {
                while (true)
                {
                    Socket s = await socket.AcceptAsync();
                    if (s != null)
                    {
                        bool IscountQuotes = true;
                        int count = 0;
                        byte[] buffer = new byte[1024];
                        string texts = "";
                        string otvet = "";
                        string log = "";
                        IPEndPoint iPEnd = (IPEndPoint)s.LocalEndPoint;

                        var result = await s.ReceiveAsync(buffer);
                        texts = Encoding.UTF8.GetString(buffer, 0, result);
                        Dispatcher.Invoke(() => Otschet.Items.Add($"В {DateTime.Now.ToString("HH:mm:ss")} от [{iPEnd.Address}:{iPEnd.Port}] получена строка {texts}"));

                        string[] text = texts.Split('+');
                        User? us = users.FirstOrDefault(x => x.login == text[0]);
                        if (us != null && us.password == text[1])
                        {
                            await s.SendAsync(System.Text.Encoding.UTF8.GetBytes("Соединение установлено"));
                            while (IscountQuotes)
                            {
                                count++;
                                if (count == 5) IscountQuotes = false;
                                result = await s.ReceiveAsync(buffer);
                                texts = Encoding.UTF8.GetString(buffer, 0, result);
                                otvet = strings[rand.Next(0, strings.Count - 1)];
                                log += $"\n {otvet}";
                                await s.SendAsync(System.Text.Encoding.UTF8.GetBytes($"{otvet} {(IscountQuotes ? "" : "\nСоединение закрыто: из-за превышения количества цитат")}"));
                                Dispatcher.Invoke(() => Otschet.Items.Add($"{otvet}"));
                            }
                            s.Shutdown(SocketShutdown.Both);
                            if (!s.Connected)
                            {
                                s.Close();
                                Dispatcher.Invoke(() => Otschet.Items.Add($"В {DateTime.Now.ToString("HH:mm:ss")} от [{iPEnd.Address}:{iPEnd.Port}] закрыто соединение"));
                            }
                        }
                        else
                        {
                            await s.SendAsync(System.Text.Encoding.UTF8.GetBytes("не верный логин или пароль"));
                            log += $"\n не верный логин или пароль";
                            s.Shutdown(SocketShutdown.Both);
                            if (!s.Connected)
                            {
                                s.Close();
                                Dispatcher.Invoke(() => Otschet.Items.Add($"В {DateTime.Now.ToString("HH:mm:ss")} от [{iPEnd.Address}:{iPEnd.Port}] закрыто соединение"));
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => Otschet.Items.Add(ex.Message));
            }
        }

        private void CanselConn_Click(object sender, RoutedEventArgs e)
        {
            socket.Shutdown(SocketShutdown.Both);
            if (!socket.Connected)
            {
                Otschet.Items.Add("Соединение разорвано");
                socket.Close();
            }
        }
    }
    record User(string login, string password);
}