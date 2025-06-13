// filepath: c:\Users\tiago\wavy\wavy\program.cs
using System;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;

namespace Wavy
{
    class WAVY
    {
        public static void Main(string[] args)
        {
            string wavyId = args.Length > 0 ? args[0] : "UNKNOWN";
            var rand = new Random();

            using (var publisher = new PublisherSocket())
            {
                publisher.Connect("tcp://localhost:5556");
                while (true)
                {
                    string temp = (rand.NextDouble() * 50).ToString("F2");
                    string pressure = (rand.NextDouble() * 50).ToString("F2");
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    publisher.SendMoreFrame("TEMPERATURE").SendFrame($"{temp}:{wavyId}:{timestamp}");
                    publisher.SendMoreFrame("PRESSURE").SendFrame($"{pressure}:{wavyId}:{timestamp}");

                    Console.WriteLine($"[ZeroMQ] Published TEMPERATURE: {temp}:{wavyId}:{timestamp}");
                    Console.WriteLine($"[ZeroMQ] Published PRESSURE: {pressure}:{wavyId}:{timestamp}");

                    Thread.Sleep(1000);
                }
            }
        }
    }
}
