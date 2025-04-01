using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class Agregador
{
    private readonly int port;
    private TcpListener listener;
    private Dictionary<string, WavyInfo> wavyDevices = new Dictionary<string, WavyInfo>();
    private readonly string configPath = "config/";

    public Agregador(int port)
    {
        this.port = port;
        listener = new TcpListener(IPAddress.Any, port);
        LoadWavyConfigurations();
    }

    public void Start()
    {
        listener.Start();
        Console.WriteLine($"Agregador iniciado na porta {port}");

        while (true)
        {
            var client = listener.AcceptTcpClient();
            ThreadPool.QueueUserWorkItem(HandleClient, client);
        }
    }

    private void LoadWavyConfigurations()
    {
        if (!Directory.Exists(configPath))
            Directory.CreateDirectory(configPath);
        
        var configFile = Path.Combine(configPath, "wavy_config.csv");
        if (File.Exists(configFile))
        {
            Console.WriteLine($"Carregando configurações de {configFile}...");
            
            foreach (var line in File.ReadAllLines(configFile).Where(l => !string.IsNullOrWhiteSpace(l)))
            {
                var parts = line.Split(',');
                if (parts.Length >= 2)
                {
                    wavyDevices[parts[0]] = new WavyInfo
                    {
                        Status = parts[1],
                        LastSync = parts.Length > 2 && DateTime.TryParse(parts[2], out var date) ? date : DateTime.Now
                    };
                }
            }
        }
    }

    private void HandleClient(object? obj)
    {
        if (obj is not TcpClient client) return;

        using (var stream = client.GetStream())
        using (var reader = new StreamReader(stream))
        using (var writer = new StreamWriter(stream) { AutoFlush = true })
        {
            var message = reader.ReadLine() ?? string.Empty;
            var (command, parameters) = Protocolo.ParseMessage(message);

            switch (command)
            {
                case "CONNECT":
                    HandleConnect(parameters, writer);
                    break;
                case "STATUS":
                    HandleStatus(parameters, writer);
                    break;
                case "DISCONNECT":
                    HandleDisconnect(parameters, writer);
                    break;
                default:
                    writer.WriteLine("NACK");
                    break;
            }
        }
    }

    private void HandleConnect(string[] parameters, StreamWriter writer)
    {
        if (parameters.Length < 1) { writer.WriteLine("NACK"); return; }
        var wavyId = parameters[0];
        
        if (wavyDevices.ContainsKey(wavyId))
        {
            wavyDevices[wavyId].Status = "operacao";
            wavyDevices[wavyId].LastSync = DateTime.Now;
            writer.WriteLine("ACK,operacao");
        }
        else
        {
            writer.WriteLine("NACK");
        }
    }

    private void HandleStatus(string[] parameters, StreamWriter writer)
    {
        if (parameters.Length < 1) { writer.WriteLine("NACK"); return; }
        var wavyId = parameters[0];
        writer.WriteLine(wavyDevices.TryGetValue(wavyId, out var info) ? $"ACK,{info.Status}" : "NACK");
    }

    private void HandleDisconnect(string[] parameters, StreamWriter writer)
    {
        if (parameters.Length < 1) { writer.WriteLine("NACK"); return; }
        var wavyId = parameters[0];
        
        if (wavyDevices.ContainsKey(wavyId))
        {
            wavyDevices[wavyId].Status = "associada";
            writer.WriteLine("ACK");
        }
        else
        {
            writer.WriteLine("NACK");
        }
    }
}

public class WavyInfo
{
    public string Status { get; set; } = "associada";
    public DateTime LastSync { get; set; }
}
