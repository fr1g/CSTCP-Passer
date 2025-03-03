// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;
using System.Text;

starting:
Console.WriteLine("CSTCPP CLIENT\nAutomatically using port 11451 as server's port.");
IPAddress? ip = null;
Socket? client = null;
IPEndPoint? endpoint = null;
while (ip == null || endpoint == null || client == null)
{
    Console.WriteLine("Enter IP of target server: ");
    Console.Write("<<< ");
    var target = Console.ReadLine();
    try
    {
        ip = IPAddress.Parse(target);
        
        endpoint = new(ip, 11451);
        client = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        
        client!.Connect(endpoint);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
        ip = null;
        Console.Error.WriteLine($"INVALID IP ADDRESS: {target} @ NOT FOUND or WRONG FORMAT \n");
        continue;
    }
}

Console.WriteLine("\nReady.\n");

while (true)
{
    Console.Write("<<< ");
    string read = Console.ReadLine() ?? "default_empty_message";
    if (read == "") 
        read = "default_empty_message";
    try
    {
        client.Send(Encoding.UTF8.GetBytes(read));
        var buff = new byte [1024 * 1024];
        int parse = client.Receive(buff);
        var res = Encoding.UTF8.GetString(buff, 0, parse);
        Console.WriteLine($"from server >>> {res}\n");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"\n===ERR CRIT===\nconnection lost.\n{ex}\n=== EC ===\n");
        goto starting;
    }


    
}