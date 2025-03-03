// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;
using System.Text;

Console.WriteLine("CSTCPP Server side...\nThis program will bind on port: 11451, timeout: 4");

Socket listener = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
IPAddress addr = IPAddress.Any;
IPEndPoint endPoint = new(addr, 11451);

try
{
    listener.Bind(endPoint);
}
catch (Exception ex)
{
    Console.Error.WriteLine("Failed to bind on the port 11451");
    Console.WriteLine(ex);
    throw;
}

listener.Listen(10);


List<EndPoint> connected = [];

List<Socket> comm = [];

Console.WriteLine($"Now started listening on the local.\n");

Socket communication;// = listener.Accept();

while (true)
{
    var acc = listener.Accept();
    comm.Add(acc);
    Console.WriteLine($"inbound client: {acc.RemoteEndPoint}");
    
    foreach (var communicate in comm)
    {
        if (comm.IndexOf(communicate) >= 1)
        {
            communicate.Send(Encoding.UTF8.GetBytes($"Server received msg:::BUSY"));
            continue;
        }
        var buff = new byte [1024 * 1024];
        int parse = communicate.Receive(buff);
        var content = Encoding.UTF8.GetString(buff, 0, parse);
        
        if (content == "/end")
        {
            communicate.Send(Encoding.UTF8.GetBytes($"Server msg:::CLOSING_CONNECTION"));
            communicate.Close();
            Console.WriteLine($"Closed connection from {communicate.RemoteEndPoint}");
            comm.Remove(communicate);
        }
        
        Console.WriteLine($"Msg::client >>> {content}");
        communicate.Send(Encoding.UTF8.GetBytes($"Server received msg OK <<< {content}"));
        // communication.Disconnect(true);
        GC.Collect();
    }
    GC.Collect();
}

