using System.Net.Sockets;
using System.Net;
using System.Text;
using System.IO;
using System;
using Newtonsoft;
using Newtonsoft.Json.Converters;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Reflection.Metadata;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Security.Claims;
using System.Diagnostics;
using System.Net.WebSockets;

class UserClient
{
    public User user;
    public TcpClient tcpClient;
    
}


class Block
{
    public virtual char Skin { get; set; }
    public ConsoleColor Color { get; set; }
    public int X { get; set; }
    public int Y { get; set; }

    public Status status = Status.BORDER;
    public enum Status
    {
        BORDER, VOID, PLAYER
    }
}
class User : Block
{
    public enum OflineOnlineStatus
    {

        Offline,
        OnServer,
    }
    public OflineOnlineStatus OnServerStatus;

    public enum LoginStatus
    {
        LogIn,
        WrongPasswordOrEmail,
        NameIsOccupied,
        EmailIsOccupied,
        ThisUserOnServer,
        
    }
    public LoginStatus statusLogin;


    public string name;
    public string email;
    public bool NewPlayerOrNot;

    private string Password;
    public string password;

    public override char Skin
    {
        get => base.Skin;
        set
        {
            if (base.Skin == ' ') base.Skin = '0';
            else base.Skin = value;
        }
    }
    public int Id { get; set; }
}



class Server
{
    public static List<UserClient> Clients = new List<UserClient>();
    public static List<User> AllUsers = new List<User>();
    private static int OnlinePlayersCount = 0;
    private static readonly object lockObject = new object(); 

    public static async Task ProcessClient(TcpClient client)
    {
        lock (lockObject) 
        {
            OnlinePlayersCount = Clients.Count;
        }

        bool UserOnServer = true;
        while (UserOnServer)
        {
            try
            {
                var stream = client.GetStream();
                List<byte> bytes = new List<byte>();
                int bytesRead = 0;

                while ((bytesRead = stream.ReadByte()) != '\0')
                {
                    bytes.Add((byte)bytesRead);
                }
                bytes.Add((byte)'\0');

                User user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(Encoding.UTF8.GetString(bytes.ToArray()));
                Console.WriteLine($"{Encoding.UTF8.GetString(bytes.ToArray())}");

                if (user.OnServerStatus == User.OflineOnlineStatus.Offline)
                {
                    lock (lockObject)  {
                        for (int i = 0; i < Clients.Count; i++)
                        {
                            if (Clients[i].user.Id == user.Id)
                            {
                                Clients.RemoveAt(i);
                                OnlinePlayersCount--;
                                PutUsersInFile();
                                UserOnServer = false;
                                break;                            
                            }
                        }
                    }
                }

                lock (lockObject) 
                {
                    for (int i = 0; i < Clients.Count; i++)
                    {
                        if (client.Connected)
                        {
                            var sendMessageStream = Clients[i].tcpClient.GetStream();
                            _ = sendMessageStream.WriteAsync(bytes.ToArray());
                        }
                    }
                }

                bytes.Clear();
            }
            catch (Exception)
            {
                Console.WriteLine("An error occurred.");
                return;
            }
        }
    }

    public static void PutUsersInFile()
    {
        string InFile = Newtonsoft.Json.JsonConvert.SerializeObject(AllUsers);
        File.WriteAllText("Users.json", InFile);
    }

    public static void GetUsersFromFile()
    {
        string fileRead = File.ReadAllText("Users.json");
        AllUsers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<User>>(fileRead);
    }




    public static async Task Main(string[] args)
    {
        TcpListener tcpListener = new TcpListener(IPAddress.Parse("26.136.90.213"), 9010);
        tcpListener.Start();
        Console.WriteLine("Server started..");
        GetUsersFromFile();

        User us = null;
        
        while (true)
        {
            try
            {
                
                TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();
                await Console.Out.WriteLineAsync("Client connected..");

                Registr(tcpClient);
                us.OnServerStatus = User.OflineOnlineStatus.OnServer;
                PutUsersInFile();

                Clients.Add(new UserClient() { tcpClient = tcpClient,user = us,});
                _ = Task.Run(async () => await ProcessClient(Clients[Clients.Count - 1].tcpClient));

            }
            catch (Exception)
            {

                throw;
            }

        }




        async Task Registr(TcpClient client)
        {
            var st = client.GetStream();

            User user = new User();
            bool logIn = false;
            while (logIn != true)
            {
                //Чтение данных от пользователя с данными регистрации
                int bytes_read = 0;
                List<byte> bytes = new List<byte>();
                while ((bytes_read = st.ReadByte()) != '\0')
                {
                    bytes.Add((byte)bytes_read);
                }
                string temp = Encoding.UTF8.GetString(bytes.ToArray());
                user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(temp);


                int count = 0;
                for (int i = 0; i < AllUsers.Count; i++)
                {
                    //База данных не обновляется из-за чего он и пускает на сервер двух одинаковых пользователей
                    if (AllUsers[i].email == user.email && AllUsers[i].password == user.password)
                    {
                        if (user.NewPlayerOrNot == false)
                        {
                            if (Clients.Find(x => x.user.Id == AllUsers[i].Id) == null)
                            {
                                user.statusLogin = User.LoginStatus.LogIn;
                                logIn = true;
                                user = AllUsers[i];
                                us = user;
                                Console.WriteLine($"User LogIn  {user.name}   {user.Id}");
                            }
                            else
                            {
                                user.statusLogin = User.LoginStatus.ThisUserOnServer; 
                            }
                        }
                        else
                        {
                            count++;
                        }
                    }
                    else
                    {
                        count++;
                    }
                }
                Console.WriteLine(user.email, user.password);


                if (count == AllUsers.Count)
                {
                    bool key = false;
                    if (user.NewPlayerOrNot == true)
                    {

                        for (int i = 0; i < AllUsers.Count; i++)
                        {
                            if (user.email == AllUsers[i].email)
                            {
                                user.statusLogin = User.LoginStatus.EmailIsOccupied;
                                key = true;
                                break;
                            }
                            else if (user.name == AllUsers[i].name)
                            {
                                user.statusLogin = User.LoginStatus.NameIsOccupied;
                                key = true;
                                break;
                            }
                        }

                        if (key == false)
                        {
                            user.Id = AllUsers.Count + 1;
                            AllUsers.Add(user);
                            user.NewPlayerOrNot = false;
                            us = user;
                            logIn = true;
                            user.statusLogin = User.LoginStatus.LogIn;
                            Console.WriteLine($"User LogIn {user.name} {user.Id}");

                        }

                    }
                    else
                    {
                        user.statusLogin = User.LoginStatus.WrongPasswordOrEmail;
                    }
                }


                string ToUs = Newtonsoft.Json.JsonConvert.SerializeObject(user) + '\0';
                byte[] bytes1 = Encoding.UTF8.GetBytes(ToUs);
                await st.WriteAsync(bytes1);
            }
            PutUsersInFile();
            //st.Close();
        }
    }
}
