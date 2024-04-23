

using Newtonsoft;
using System.Net.Sockets;
using System.Text;

public static class ServerUser
{

    public static void SendClass<Class> (TcpClient client,Class cl)
    {
        var st = client.GetStream();
        string JsonString = Newtonsoft.Json.JsonConvert.SerializeObject(cl) + '\0';
        byte[] bytes1 = Encoding.UTF8.GetBytes(JsonString);
        st.WriteAsync(bytes1);

    }

    public static void SendString(TcpClient client, string str)
    {
        var st = client.GetStream();
        str += '\0';
        byte[] bytes1 = Encoding.UTF8.GetBytes(str);
        st.WriteAsync(bytes1);
    }

    public static List<byte> GetBytes(TcpClient client) {
        var stream = client.GetStream();
        List<byte> bytes = new List<byte>();
        int bytesRead = 0;

        while ((bytesRead = stream.ReadByte()) != '\0')
        {
            bytes.Add((byte)bytesRead);
        }
        bytes.Add((byte)'\0');

        return bytes;
    }

    public static string GetString(TcpClient client)
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

            string str = Encoding.UTF8.GetString(bytes.ToArray());
            return str;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}



