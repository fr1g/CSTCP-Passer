// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;
using System.Text;

starting:
Console.WriteLine("CSTCPP CLIENT\nAutomatically using port 11451 as server's port.");
#if DEBUG
Console.WriteLine("debug mode");
#endif
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
        break;
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
        ip = null;
        Console.Error.WriteLine($"INVALID IP ADDRESS: {target} @ NOT FOUND or WRONG FORMAT \n");
        continue;
    }
}

Console.WriteLine("\nReady. Trying connection...");
var triage = (await SendTimeout("", client, true, 4001));
if(triage == ":::ERR")
    goto starting;
else Console.WriteLine($">>> {triage}");

var isEnding = false;
while (true)
{
    if (isEnding &&
        (await SendTimeout("@check_connection", client, isMaintain: true)).Contains(
            ":::ERR"))
    {
        Console.WriteLine("Connection shut. Farewell!");
        return;
    }
        
    
    Console.Write("<<< ");
    
    string read = Console.ReadLine() ?? "default_empty_message";
    
    #if DEBUG
    Console.WriteLine($"{isEnding}   |{read}");
    #endif
    
    switch (read)
    {
        case "":
            read = "default_empty_message";
            break;
        case "/end":
            isEnding = true;
            break;
        case "/forceend":
            Console.WriteLine("Force End was Called on the Client.");
            return;
    }
    var answer = await SendTimeout(read, client);
    if(answer == ":::ERR") 
        goto starting;
    else Console.WriteLine(answer);
}

async Task<string> SendTimeout(string read, Socket soc, bool isKnockDoor = false, int timeout = 4000, bool isMaintain = false)
{
    Task<string> triage;
    #if DEBUG
    timeout = 4001;
    Console.WriteLine($"Debug mode made wait time changed: {timeout}");
    #endif
    try
    {
        var delay = Task.Delay(timeout);
        triage = Send(isKnockDoor ? "@clientKnocksDoor" : read, soc);
        var completed = await Task.WhenAny(triage, delay);
        if (completed == delay) 
            throw new Exception(isKnockDoor ? "Timed out while knocking door." : "Time out.");
        else
        {
            var result = await triage;
            if (result.Contains("msg:::BUSY"))
                return ("server is busy.\n");
            else return (isKnockDoor ? "SERVER READY \n" : result);
        }
    }
    catch (Exception ex)
    {
        if(!isMaintain)
            Console.Error.WriteLine($"The connection is not usable: {ex.Message}\n");
        return ":::ERR";
    }
}

async Task<string> Send(string read, Socket c)
{
    await c.SendAsync(Encoding.UTF8.GetBytes(read));
    var buff = new byte [1024 * 1024];
    var parse = await c.ReceiveAsync(buff);
    var res = Encoding.UTF8.GetString(buff, 0, parse);
    return $"from server >>> {res}\n";
}