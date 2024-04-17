using System.Net.Sockets;
using System.Net;
using System.Text;
using Newtonsoft.Json.Converters;
using Newtonsoft;
using System.IO;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Reflection.Metadata;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;


//List players near map
interface IPlayers
{
    public void PlayersOut(User player, int count)
    {
        Console.SetCursorPosition(Map.width + 1, count);
        if (count == 1) Console.Write("(you) ");
        string str = "";
        if (player.Skin != '#') Console.WriteLine($"{player.name} - Skin: {player.Skin}, Color - {player.Color}, X - {player.X} Y - {player.Y}");
        else
        {
            for (int i = 0; i < player.name.Length + 23; i++) str += " ";
            Console.WriteLine(str);
        }
    }
}


class Client
{
    private static bool OpenConsole = false;
    private static bool CloseGameOrNot = false;
    public static List<User> Users = new List<User>();


    public static void ClearPreviousPosition(User user)
    {
        Console.SetCursorPosition(user.X, user.Y);
        Console.ForegroundColor = ConsoleColor.Black;
        Console.Write(' ');
    }

    public static void PlayersOut(User player, int count)
    {
        Console.SetCursorPosition(Map.width + 1, count);
        if (count == 1) Console.Write("(you) ");
        string str = "";
        if (player.Skin != '#') Console.WriteLine($"{player.name} - Skin: {player.Skin}, Color - {player.Color}, X - {player.X}, Y - {player.Y}  ");
        else
        {
            for (int i = 0; i < player.name.Length + 40; i++) str += " ";
            Console.WriteLine(str);
        }

    }

    public static void WriteCurrentPosition(User user)
    {
        Console.SetCursorPosition(user.X, user.Y);
        Console.ForegroundColor = user.Color;
        Console.Write(user.Skin);
    }

    public static async Task GetMessage(TcpClient client)
    {
        var stream = client.GetStream();
        List<byte> bytes = new List<byte>();
        try
        {
            while (CloseGameOrNot != true)
            {
                int bytes_read = 0;

                while ((bytes_read = stream.ReadByte()) != '\0')
                {
                    bytes.Add((byte)bytes_read);
                }

                string userJson = Encoding.UTF8.GetString(bytes.ToArray());



                User user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(userJson);
                bool isContain = false;
                for (int i = 0; i < Users.Count; i++)
                {
                    if (Users[i].Id == user.Id)
                    {
                        PlayersOut(user, i + 1);

                        if (user.X == -1 && user.Y == -1 && user.Skin == ' ')
                        {
                            Users.RemoveAt(i);
                            break;                      
                        }
                        else
                        {
                            if (OpenConsole == false) ClearPreviousPosition(Users[i]);
                            Users[i] = user;
                            if (OpenConsole == false) WriteCurrentPosition(Users[i]);
                            isContain = true;
                            break;
                        }
                    }
                }
                if (!isContain)
                {
                    Users.Add(user);
                    if (OpenConsole == false) WriteCurrentPosition(user);
                }
                bytes.Clear();
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }


    

    static async Task SendToServer(TcpClient client, User me)
    {
        var stream = client.GetStream();
        string jsonUser = Newtonsoft.Json.JsonConvert.SerializeObject(me);


        if (jsonUser != null)
        {
            jsonUser += '\0';
            byte[] bytes = Encoding.UTF8.GetBytes(jsonUser);
            await stream.WriteAsync(bytes);
        }
    }

    static void UserConsole(ref User me)
    {
        OpenConsole = true;
        UserConsole uc = new UserConsole(ref me);
        OpenConsole = false;
        Console.Clear();
        Map.OutputField();
        Console.WriteLine(" ");
        Console.WriteLine("Нажмите Enter чтобы открыть консоль");
        Console.WriteLine("Нажмите Esc чтобы выйти из игры");
    }

    static void GameExit(ref User me)
    {
        CloseGameOrNot = true;
        me.X = 0;
        me.Y = 0;
        me.Skin = '#';
        me.Color = ConsoleColor.Green;
        me.OnServerStatus = User.OflineOnlineStatus.Offline;
    }


    public static async Task MovementAsync(TcpClient client, User me)
    {
        while (CloseGameOrNot != true)
        {
            ConsoleKeyInfo key = Console.ReadKey(true);
            switch (key.Key)
            {
                case ConsoleKey.W:
                    me.Y -= 1;
                    if (!Availiable(me))
                    {
                        me.Y += 1;
                    }
                    break;
                case ConsoleKey.A:
                    me.X -= 1;
                    if (!Availiable(me))
                    {
                        me.X += 1;
                    }
                    break;
                case ConsoleKey.S:
                    me.Y += 1;
                    if (!Availiable(me))
                    {
                        me.Y -= 1;
                    }
                    break;
                case ConsoleKey.D:
                    me.X += 1;
                    if (!Availiable(me))
                    {
                        me.X -= 1;
                    }
                    break;
                case ConsoleKey.Enter:
                    UserConsole(ref me);
                    break;
                case ConsoleKey.Escape:
                    GameExit(ref me);
                    break;
                 default: 
                    break;
            }

            await SendToServer(client, me);
        }


        bool Availiable(User mee)
        {
            if (Map.field.First(cell => cell.X == me.X && cell.Y == me.Y).status != Block.Status.VOID)
            {
                return false;
            }
            for (int i = 0; i < Users.Count; i++)
            {
                if (mee.X == Users[i].X && mee.Y == Users[i].Y && mee.Id != Users[i].Id)
                {
                    return false;
                }
            }
            return true;
        }
    }
    public static async Task Main(string[] args)
    {
        TcpClient tcpClient = new TcpClient();

        await tcpClient.ConnectAsync(IPAddress.Parse("26.64.111.48"), 9010);

        await Console.Out.WriteLineAsync("Connected..");

        User user = new User();

        Registr(tcpClient);
        await Console.Out.WriteAsync("Input symbol ");

        char symb = Console.ReadLine()[0];

        await Console.Out.WriteLineAsync("Input color (1 - 15)");
        int color = Convert.ToInt32(Console.ReadLine());
        
        ConsoleColor cColor = (ConsoleColor)(color < 1 ? 1 : (color > 15 ? 15 : color));


        user.Color = cColor;
        user.Skin = symb;
        user.X = 10;
        user.Y = 10;
        user.OnServerStatus = User.OflineOnlineStatus.OnServer;

        Console.Clear();
        Users.Add(user);
        Map.GenerateMap(20, 20);
        Map.OutputField();
        Console.WriteLine(' ');
        Console.WriteLine("Нажмите Enter чтобы открыть консоль");
        Console.WriteLine("Нажмите Esc чтобы выйти из игры");


        _ = Task.Run(async () => await GetMessage(tcpClient));

        await SendToServer(tcpClient, user);

        await MovementAsync(tcpClient, Users.First(x => x.Id == user.Id));






        async Task Registr(TcpClient client)
        {
            var st = client.GetStream();
            bool logIn = false;
            while (logIn != true)
            {
                Console.Clear();
                Console.WriteLine("Введи 1 если создаешь новый акк и введи 2 если уже он у тебя есть");
                int te = 0;
                try { te = Convert.ToInt32(Console.ReadLine()); } catch { te = 0; }

                if (te == 1)
                {
                    Console.WriteLine("Введи новый email");
                    user.email = Console.ReadLine();
                    Console.WriteLine("Введи новый пароль");
                    user.password = Console.ReadLine();
                    Console.WriteLine("Введи имя аккаунта");
                    user.name = Console.ReadLine();
                    user.NewPlayerOrNot = true;
                }
                else if (te == 2)
                {
                    Console.WriteLine("Input Email");
                    user.email = Console.ReadLine();
                    Console.WriteLine("Input Password");
                    user.password = Console.ReadLine();
                    user.NewPlayerOrNot = false;
                }
                else
                {
                    continue;
                }

                string ToUs = Newtonsoft.Json.JsonConvert.SerializeObject(user);
                ToUs += '\0';
                byte[] bytes1 = Encoding.UTF8.GetBytes(ToUs);
                await st.WriteAsync(bytes1);


                int bytes_read = 0;
                List<byte> bt = new List<byte>();
                while ((bytes_read = st.ReadByte()) != '\0')
                {
                    bt.Add((byte)bytes_read);
                }
                string temp = Encoding.UTF8.GetString(bt.ToArray());
                user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(temp);


                if (user.statusLogin == User.LoginStatus.LogIn)
                {
                    Console.WriteLine($"Wellcum {user.name}");
                    logIn = true;
                }
                else
                {
                    if (user.statusLogin == User.LoginStatus.ThisUserOnServer) Console.WriteLine("This user now on server. Maybe your hacked <3");
                    else if (user.statusLogin == User.LoginStatus.WrongPasswordOrEmail) Console.WriteLine("Wrong email or password");
                    else if (user.statusLogin == User.LoginStatus.EmailIsOccupied) Console.WriteLine("This email used on another account");
                    else if (user.statusLogin == User.LoginStatus.NameIsOccupied) Console.WriteLine("This name is used on another account");

                    Console.WriteLine("Press Enter to continue");
                    Console.ReadLine();
                }
            }
        }

    }
}