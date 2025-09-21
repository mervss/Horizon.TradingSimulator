using System.Net.Sockets;
using System.Text;

class Program
{
    static async Task Main()
    {
        using var client = new TcpClient();
        await client.ConnectAsync("127.0.0.1", 5055); 
        using var reader = new StreamReader(client.GetStream(), Encoding.UTF8);

        Console.WriteLine("TCP connected. Waiting for lines... Ctrl+C to quit");

        while (true)
        {
            var line = await reader.ReadLineAsync();
            if (line is null) break; // sunucu kapandı
            Console.WriteLine(line);
        }
    }
}
