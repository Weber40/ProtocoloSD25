using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

public class Wavy
{
    private readonly string wavyId;
    private readonly string agregadorIp;
    private readonly int agregadorPort;

    public Wavy(string id, string ip, int port)
    {
        wavyId = id ?? throw new ArgumentNullException(nameof(id));
        agregadorIp = ip ?? throw new ArgumentNullException(nameof(ip));
        agregadorPort = port;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine($"=== WAVY {wavyId} ===");
        Console.WriteLine($"Conectando ao agregador {agregadorIp}:{agregadorPort}");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using (var client = new TcpClient(agregadorIp, agregadorPort))
                using (var stream = client.GetStream())
                using (var reader = new StreamReader(stream))
                using (var writer = new StreamWriter(stream))
                {
                    // Envia mensagem de conexão
                    await writer.WriteLineAsync(Protocolo.BuildConnectMessage(wavyId));
                    await writer.FlushAsync();

                    var response = await reader.ReadLineAsync();
                    var (command, parameters) = Protocolo.ParseMessage(response);

                    if (command == Protocolo.ACK)
                    {
                        Console.WriteLine($"Conectado ao agregador. Status: {parameters[0]}");

                        bool running = true;
                        while (running)
                        {
                            Console.WriteLine("\nOpções:");
                            Console.WriteLine("1. Enviar dados");
                            Console.WriteLine("2. Ver status");
                            Console.WriteLine("3. Desconectar");
                            Console.Write("Escolha: ");

                            var choice = Console.ReadLine();

                            switch (choice)
                            {
                                case "1":
                                    await EnviarDados(writer, reader);
                                    break;

                                case "2":
                                    await VerStatus(writer, reader);
                                    break;

                                case "3":
                                    await Desconectar(writer);
                                    running = false;
                                    break;

                                default:
                                    Console.WriteLine("Opção inválida.");
                                    break;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Falha na conexão com o agregador.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }

            Console.WriteLine("Tentando reconectar em 5 segundos...");
            await Task.Delay(5000, cancellationToken);
        }
    }

    private async Task EnviarDados(StreamWriter writer, StreamReader reader)
    {
        Console.Write("Tipo de dado: ");
        var dataType = Console.ReadLine() ?? "default";

        Console.Write("Dados a enviar: ");
        var data = Console.ReadLine() ?? string.Empty;

        await writer.WriteLineAsync(Protocolo.BuildDataMessage(wavyId, dataType, data));
        await writer.FlushAsync();

        var response = await reader.ReadLineAsync();
        Console.WriteLine(response == Protocolo.ACK ? 
            "Dados enviados com sucesso!" : "Falha no envio dos dados.");
    }

    private async Task VerStatus(StreamWriter writer, StreamReader reader)
    {
        await writer.WriteLineAsync(Protocolo.BuildStatusMessage(wavyId));
        await writer.FlushAsync();

        var response = await reader.ReadLineAsync();
        if (response.StartsWith(Protocolo.ACK))
            Console.WriteLine($"Status atual: {response.Split(Protocolo.SEPARATOR)[1]}");
        else
            Console.WriteLine("Falha ao obter status.");
    }

    private async Task Desconectar(StreamWriter writer)
    {
        await writer.WriteLineAsync(Protocolo.BuildDisconnectMessage(wavyId));
        await writer.FlushAsync();
    }
}
