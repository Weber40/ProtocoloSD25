using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;

class Program
{
    static void Main()
    {
        string serverAddress = "tcp://localhost:7100"; // Recebe dados do servidor
        string resultAddress = "tcp://localhost:7001"; // Envia resultados para o servidor

        List<DataRow> data = new List<DataRow>();
        object dataLock = new object();

        // Thread para receber dados do servidor e atualizar a lista
        Thread receiverThread = new Thread(() =>
        {
            using (var puller = new PullSocket())
            {
                puller.Connect(serverAddress);
                Console.WriteLine($"[DataAnalysis] Waiting for data from server at {serverAddress}");

                while (true)
                {
                    string allData = puller.ReceiveFrameString();
                    Console.WriteLine($"[DataAnalysis] Received data ({allData.Length} chars)");
                    var newData = LoadDataFromString(allData);
                    lock (dataLock)
                    {
                        data = newData;
                    }
                }
            }
        });
        receiverThread.IsBackground = true;
        receiverThread.Start();

        // Thread para enviar resultados a cada 10 segundos
        using (var pusher = new PushSocket())
        {
            pusher.Connect(resultAddress);

            while (true)
            {
                List<DataRow> snapshot;
                lock (dataLock)
                {
                    snapshot = new List<DataRow>(data);
                }

                foreach (var sensorType in new[] { "TEMPERATURE", "PRESSURE" })
                {
                    var values = snapshot.Where(d => d.Topic == sensorType).Select(d => d.Value).ToList();
                    if (values.Count > 0)
                    {
                        double avg = values.Average();
                        double min = values.Min();
                        double max = values.Max();
                        string result = $"{sensorType}:AVG={avg:F2};MIN={min:F2};MAX={max:F2}";
                        Console.WriteLine($"[DataAnalysis] {result}");
                        pusher.SendFrame(result); // Envia para o servidor
                        Console.WriteLine($"[DataAnalysis] Sent to server: {result}");
                    }
                }
                Thread.Sleep(10000); // 10 segundos
            }
        }
    }

    class DataRow
    {
        public string Topic { get; set; } = "";
        public double Value { get; set; }
        public string WavyId { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }

    static List<DataRow> LoadDataFromString(string allData)
    {
        var list = new List<DataRow>();
        var lines = allData.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            Console.WriteLine($"[DataAnalysis] Line: {line}");
            var parts = line.Split(',');
            if (parts.Length == 4 &&
                double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double value) &&
                DateTime.TryParse(parts[3], out DateTime date))
            {
                list.Add(new DataRow
                {
                    Topic = parts[0],
                    Value = value,
                    WavyId = parts[2],
                    Timestamp = date.Date
                });
            }
            else
            {
                Console.WriteLine($"[DataAnalysis] Skipped invalid line: {line}");
            }
        }
        return list;
    }
}