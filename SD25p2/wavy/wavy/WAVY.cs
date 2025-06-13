// WAVY Publisher
using RabbitMQ.Client;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;

class WAVY
{
    private static async Task<string> SendMessageAndWaitAckAsync(NetworkStream stream, string message)
    {
        byte[] msg = Encoding.ASCII.GetBytes(message + "\n");
        await stream.WriteAsync(msg, 0, msg.Length);

        var buffer = new byte[1024];
        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
        string response = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
        Console.WriteLine($"Sent: {message} | Received: {response}");
        return response;
    }

    public static async Task Main(string[] args)
    {
        int port = 13001;
        string serverAddress = "localhost";

        try
        {
            using (TcpClient client = new TcpClient())
            {
                Console.WriteLine("Connecting to the server...");
                await client.ConnectAsync(serverAddress, port);
                Console.WriteLine("Connected to the server.");

                using (NetworkStream stream = client.GetStream())
                {
                    // Send ID to SERVER and wait for ACK
                    await SendMessageAndWaitAckAsync(stream, "ID:WAVY_2");

                    // Register data type (e.g., "PRESSURE") and wait for ACK
                    await SendMessageAndWaitAckAsync(stream, "DATA_REG:PRESSURE");

                    // Simulate sending data and wait for ACK each time
                    for (int i = 0; i < 5; i++)
                    {
                        string data = $"Data_{i}";
                        await SendMessageAndWaitAckAsync(stream, $"DATA:{data}");
                        await Task.Delay(200); // Simulate time delay
                    }

                    // End session and wait for ACK
                    await SendMessageAndWaitAckAsync(stream, "END");
                }
            }
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"SocketException: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }

        // RabbitMQ Publisher
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.ExchangeDeclare(exchange: "wavy_data", type: ExchangeType.Topic);

        string routingKey = "sensor.pressure"; // ou outro tipo
        string message = "DATA:123.45";
        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(exchange: "wavy_data", routingKey: routingKey, body: body);
        Console.WriteLine($"[WAVY] Sent '{routingKey}':'{message}'");
    }
}
