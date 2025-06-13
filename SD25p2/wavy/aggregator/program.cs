// filepath: c:\Users\tiago\wavy\aggregator\program.cs
using System;
using System.Collections.Generic;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;

namespace Aggregator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var preprocessingService = new PreprocessingService();
            var processedData = new List<string>();
            var sendInterval = TimeSpan.FromSeconds(15);

            using (var subscriber = new SubscriberSocket())
            using (var pusher = new PushSocket())
            {
                subscriber.Bind("tcp://*:5556"); // <-- ALTERA PARA BIND!
                subscriber.Subscribe("");
                Console.WriteLine("Aggregator Subscriber running on tcp://*:5556");

                pusher.Connect("tcp://localhost:6000"); // O servidor deve fazer PullSocket.Bind nesta porta
                Console.WriteLine("Aggregator Pusher connected to tcp://localhost:6000");

                var lastSend = DateTime.Now;

                while (true)
                {
                    // Recebe e pré-processa
                    var topic = subscriber.ReceiveFrameString();
                    var msg = subscriber.ReceiveFrameString();
                    Console.WriteLine($"[Aggregator][ZeroMQ] Data received: {topic}:{msg}");

                    var processed = preprocessingService.Preprocess(topic, msg);
                    Console.WriteLine($"[Aggregator][ZeroMQ] Data preprocessed: {processed}");

                    lock (processedData)
                    {
                        processedData.Add(processed);
                    }

                    // Envia a cada 15 segundos
                    if ((DateTime.Now - lastSend) > sendInterval)
                    {
                        List<string> toSend;
                        lock (processedData)
                        {
                            toSend = new List<string>(processedData);
                            processedData.Clear();
                        }
                        foreach (var data in toSend)
                        {
                            pusher.SendFrame(data);
                            Console.WriteLine($"[Aggregator][ZeroMQ] Sent to server: {data}");
                        }
                        lastSend = DateTime.Now;
                    }
                }
            }
        }
    }

    public class PreprocessingService
    {
        public string Preprocess(string topic, string msg)
        {
            // lógica de pré-processamento
            return $"{topic}:{msg}".ToUpper();
        }
    }
}
