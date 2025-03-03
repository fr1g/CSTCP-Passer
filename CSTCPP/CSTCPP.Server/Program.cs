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

Console.WriteLine($"Now started listening on the xxx");

Socket communication = listener.Accept();

while (true)
{
    // communication = listener.Accept();
    Console.WriteLine($"inbound client: {communication.RemoteEndPoint}");
    if (connected.IndexOf(communication.RemoteEndPoint!) > 0)
    {
        Console.WriteLine("inbound but busy.");
        communication.Send(Encoding.UTF8.GetBytes($"Server received msg:::BUSY"));
        communication.Close();
        continue;
    }
    else
    {
        if(connected.IndexOf(communication.RemoteEndPoint!) == -1) 
            connected.Add(communication.RemoteEndPoint!);
        
    }
    var buff = new byte [1024 * 1024];
    int parse = communication.Receive(buff);
    var content = Encoding.UTF8.GetString(buff, 0, parse);
    if (content == "/end")
    {
        connected.Remove(communication.RemoteEndPoint!);
        communication.Send(Encoding.UTF8.GetBytes($"Server msg:::CLOSING_CONNECTION"));
        communication.Close();
        continue;
    }
    Console.WriteLine($"Msg::client >>> {content}");
    communication.Send(Encoding.UTF8.GetBytes($"Server received msg OK <<< {content}"));
    // communication.Disconnect(true);
    GC.Collect();
}

