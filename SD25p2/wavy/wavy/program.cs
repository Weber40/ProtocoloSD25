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
                    string date = DateTime.Now.ToString("yyyy-MM-dd"); // Só a data

                    // Formato CSV fácil para DataAnalysis: TOPIC,VALUE,WAVYID,DATE
                    string tempLine = $"TEMPERATURE,{temp},{wavyId},{date}";
                    string pressureLine = $"PRESSURE,{pressure},{wavyId},{date}";

                    publisher.SendMoreFrame("TEMPERATURE").SendFrame(tempLine);
                    publisher.SendMoreFrame("PRESSURE").SendFrame(pressureLine);

                    Console.WriteLine($"[ZeroMQ] Published {tempLine}");
                    Console.WriteLine($"[ZeroMQ] Published {pressureLine}");

                    Thread.Sleep(1000);
                }
            }
        }
    }
}
