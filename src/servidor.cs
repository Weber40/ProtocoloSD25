using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

public class Servidor
{
    private TcpListener listener;
    private readonly string dataPath = "data/";

    public Servidor(int port)
    {
        listener = new TcpListener(IPAddress.Any, port);
        if (!Directory.Exists(dataPath))
            Directory.CreateDirectory(dataPath);
    }

    public void Start()
    {
        listener.Start();
        Console.WriteLine($"Servidor iniciado na porta {((IPEndPoint)listener.LocalEndpoint).Port}");

        while (true)
        {
            var client = listener.AcceptTcpClient();
            Task.Run(() => HandleClient(client));
        }
    }

    private async Task HandleClient(TcpClient client)
    {
        using (client)
        using (var stream = client.GetStream())
        using (var reader = new StreamReader(stream))
        using (var writer = new StreamWriter(stream))
        {
            try
            {
                var message = await reader.ReadLineAsync();
                var (command, parameters) = Protocolo.ParseMessage(message);

                if (command == Protocolo.DATA && parameters.Length >= 3)
                {
                    var wavyId = parameters[0];
                    var dataType = parameters[1];
                    var data = parameters[2];

                    SaveData(wavyId, dataType, data);
                    await writer.WriteLineAsync(Protocolo.ACK);
                }
                else
                {
                    await writer.WriteLineAsync(Protocolo.NACK);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no servidor: {ex.Message}");
            }
        }
    }

    private void SaveData(string wavyId, string dataType, string data)
    {
        var today = DateTime.Now.ToString("yyyyMMdd");
        var dataDir = Path.Combine(dataPath, today);

        if (!Directory.Exists(dataDir))
            Directory.CreateDirectory(dataDir);

        var filePath = Path.Combine(dataDir, $"{dataType}.csv");
        var fileLock = $"{dataType}_lock";

        lock (fileLock)
        {
            File.AppendAllText(filePath, $"{DateTime.Now:HH:mm:ss},{wavyId},{data}{Environment.NewLine}");
            Console.WriteLine($"Dados recebidos: {dataType} de {wavyId}");
        }
    }
}
