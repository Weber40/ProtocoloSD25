using System;
using System.IO;
using NetMQ;
using NetMQ.Sockets;

class Server
{
    static void Main()
    {
        string filePath = "received_data.csv";
        using (var puller = new PullSocket("@tcp://*:6000"))
        using (var writer = new StreamWriter(filePath, append: true))
        {
            Console.WriteLine("Server (ZeroMQ) is running and waiting for data...");
            while (true)
            {
                var data = puller.ReceiveFrameString();
                Console.WriteLine($"[Server][ZeroMQ] Received: {data}");
                writer.WriteLine(data);
                writer.Flush();
            }
        }
    }
}