using System;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using System.Linq;
using NetMQ;
using NetMQ.Sockets;

class Server
{
    static void Main()
    {
        string filePath = @"C:\Users\tiago\sd2425parte2\ProtocoloSD25-1\SD25p2\wavy\received_data.csv";
        string analysisAddress = "tcp://localhost:7100"; // para enviar dados ao DataAnalysis
        string analysisResultsAddress = "tcp://*:7001";  // para receber resultados do DataAnalysis

        // Dicionário thread-safe para controlar o último tempo de cada WAVY
        var wavyLastSeen = new ConcurrentDictionary<string, DateTime>();

        // Thread para mostrar número de WAVYs online a cada 5 segundos
        new Thread(() =>
        {
            while (true)
            {
                var now = DateTime.UtcNow;
                // Só conta WAVYs que enviaram dados nos últimos 4 segundos
                var onlineCount = wavyLastSeen.Where(kv => (now - kv.Value).TotalSeconds <= 15).Count();
                Console.WriteLine($"[Server] Current number of WAVY's connected: {onlineCount}");
                Thread.Sleep(10000);
            }
        })
        { IsBackground = true }.Start();

        // Thread para receber resultados do DataAnalysis
        new Thread(() =>
        {
            using (var analysisPull = new PullSocket(analysisResultsAddress))
            {
                Console.WriteLine($"[Server] Listening for analysis results at {analysisResultsAddress}");
                while (true)
                {
                    try
                    {
                        var result = analysisPull.ReceiveFrameString();
                        Console.WriteLine($"[Server][AnalysisResult] {result}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Server] Error receiving analysis result: {ex.Message}");
                    }
                }
            }
        })
        { IsBackground = true }.Start();

        // Thread para enviar periodicamente o ficheiro para o DataAnalysis
        new Thread(() =>
        {
            using (var analysisPush = new PushSocket())
            {
                analysisPush.Bind(analysisAddress);
                Console.WriteLine($"[Server] Ready to send data to DataAnalysis at {analysisAddress}");

                while (true)
                {
                    try
                    {
                        if (File.Exists(filePath))
                        {
                            string allData;
                            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            using (var sr = new StreamReader(fs))
                            {
                                allData = sr.ReadToEnd();
                            }
                            analysisPush.SendFrame(allData);
                            Console.WriteLine($"[Server] Sent all received_data.csv to DataAnalysis ({allData.Length} chars)");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Server] Error sending data to DataAnalysis: {ex.Message}");
                    }
                    Thread.Sleep(10000); // 10 segundos
                }
            }
        })
        { IsBackground = true }.Start();

        // --- FUNCIONALIDADE ATUAL DO SERVIDOR ---
        using (var puller = new PullSocket("@tcp://*:6000"))
        using (var writer = new StreamWriter(filePath, append: true))
        {
            Console.WriteLine("Server (ZeroMQ) is running and waiting for data...");
            while (true)
            {
                var data = puller.ReceiveFrameString();

                // Se a linha tiver ":", guarda só o que vem depois do primeiro ":"
                var idx = data.IndexOf(':');
                string dataToWrite = data;
                if (idx >= 0 && idx < data.Length - 1)
                    dataToWrite = data.Substring(idx + 1);

                // Extrai o wavy_ID (assumindo formato CSV: TOPIC,VALUE,WAVYID,DATE)
                string wavyId = "unknown";
                var parts = dataToWrite.Split(',');
                if (parts.Length >= 3)
                    wavyId = parts[2];

                // Atualiza o tempo deste WAVY
                if (wavyId != "unknown")
                    wavyLastSeen[wavyId] = DateTime.UtcNow;

                writer.WriteLine(dataToWrite);
                writer.Flush();
            }
        }
    }
}