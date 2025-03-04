using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;

var sockets = new ConcurrentDictionary<string, Socket>();
var semaphore = new SemaphoreSlim(10); 
List<string> queue = []; // queue reference (controlling the order)

await StartListeningAsync();

async Task StartListeningAsync()
{
    var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    listener.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Any, 11451));
    listener.Listen(100);
    Console.WriteLine("Server started. Waiting for connections...");

    while (true)
    {
        var clientSocket = await listener.AcceptAsync();
        Console.WriteLine($"New connection from {clientSocket.RemoteEndPoint}");
        
        sockets.TryAdd(clientSocket.RemoteEndPoint!.ToString()!, clientSocket);
        queue.Add(clientSocket!.RemoteEndPoint!.ToString()!);
        
        _ = Task.Run(() => HandleClientAsync(clientSocket));
    }
}

async Task HandleClientAsync(Socket clientSocket)
{
    var ip = clientSocket.RemoteEndPoint!.ToString()!;
    await semaphore.WaitAsync(); 

    try
    {
        var buffer = new byte[1024 * 1024]; 

        while (true)
        {
            int bytesRead;
            try
            {
                bytesRead = await clientSocket.ReceiveAsync(buffer);
                if (sockets.Count > 1 && queue.IndexOf(ip) >= 1)
                {
                    await clientSocket.SendAsync(Encoding.UTF8.GetBytes("Server msg:::BUSY"));
                    continue;
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Socket error: {ex.Message}");
                await clientSocket.SendAsync(Encoding.UTF8.GetBytes("Server msg:::SE"));
                queue.Remove(ip);
                break;
            }

            if (bytesRead == 0)
            {
                Console.WriteLine($"Client disconnected: {clientSocket.RemoteEndPoint}");
                queue.Remove(ip);
                break;
            }

            var content = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Message from client: {content}");

            if (content == "/end")
            {
                await clientSocket.SendAsync(Encoding.UTF8.GetBytes("Server msg:::CLOSING_CONNECTION"));
                Console.WriteLine($"Closed connection from {clientSocket.RemoteEndPoint}");
                queue.Remove(ip);
                sockets.TryRemove(new KeyValuePair<string, Socket>(ip, clientSocket));
                return;
            }
            
            await clientSocket.SendAsync(Encoding.UTF8.GetBytes($"Server received: {content} <<< "));
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error processing on {ip}:\n    {ex.Message}\n---");
    }
    finally
    {
        try
        {
            clientSocket.Shutdown(SocketShutdown.Both);
        }
        catch { /*...*/ }
        finally
        {
            clientSocket.Close();
            semaphore.Release(); 
        }
    }
}



// // See https://aka.ms/new-console-template for more information
//
// using System.Net;
// using System.Net.Sockets;
// using System.Text;
//
// Console.WriteLine("CSTCPP Server side...\nThis program will bind on port: 11451, timeout: 4");
//
// Socket listener = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
// IPAddress addr = IPAddress.Any;
// IPEndPoint endPoint = new(addr, 11451);
//
// try
// {
//     listener.Bind(endPoint);
// }
// catch (Exception ex)
// {
//     Console.Error.WriteLine("Failed to bind on the port 11451");
//     Console.WriteLine(ex);
//     throw;
// }
//
// listener.Listen(10);
//
// List<Socket> comm = [];
//
// Console.WriteLine($"Now started listening on the local.\n");
//
// // var th = new Thread(ProcessRequest);
// Task.Run(ProcessRequest);
// // th.Start();
// while (true)
// {
//     var acc = await listener.AcceptAsync();
//     comm.Add(acc);
//     Console.WriteLine($"inbound client: {acc.RemoteEndPoint}");
//     
//     GC.Collect();
// }
//
//
// void ProcessRequest()
// {
//     while (true)
//     {
//         // Console.WriteLine("Started Task Listening");
//         try
//         {
//             foreach (var communicate in comm.ToArray())
//             {
//                 Console.WriteLine($"");
//                 Task.Run(() =>
//                 {
//                     if (comm.IndexOf(communicate) >= 1)
//                     {
//                         communicate.Send(Encoding.UTF8.GetBytes($"Server received msg:::BUSY"));
//                         return;
//                     }
//
//                     var buff = new byte [1024 * 1024];
//                     int parse = communicate.Receive(buff);
//                     var content = Encoding.UTF8.GetString(buff, 0, parse);
//
//                     if (content == "/end")
//                     {
//                         communicate.Send(Encoding.UTF8.GetBytes($"Server msg:::CLOSING_CONNECTION"));
//
//                         Console.WriteLine($"Closed connection from {communicate.RemoteEndPoint}");
//                         comm.Remove(communicate);
//
//                         communicate!.Close();
//                         communicate?.Dispose();
//                     }
//
//                     Console.WriteLine($"Msg::client >>> {content}");
//                     communicate.Send(Encoding.UTF8.GetBytes($"Server received msg OK <<< {content}"));
//                 });
//                 // communication.Disconnect(true);
//                 GC.Collect();
//                 GC.WaitForPendingFinalizers();
//                 GC.Collect();
//             }
//         }
//         catch (Exception ex)
//         {
//             GC.Collect();
//             Console.WriteLine(ex);
//             if (ex.Message.Contains("Cannot access a disp"))
//             {
//                 Console.WriteLine($"{comm.Count}; ");
//                 return;
//             }
//             if(ex.Message.Contains("Collection was modified"))
//             {
//                 Console.WriteLine($"{comm.Count}; ");
//                 continue;
//             }
//         }
//         GC.Collect();
//         GC.WaitForPendingFinalizers();
//         GC.Collect();
//         // GC.RemoveMemoryPressure();
//     }
//     
// }
