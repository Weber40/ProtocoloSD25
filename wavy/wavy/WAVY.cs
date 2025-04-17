using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class WAVY
{
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
                    // Send ID to SERVER
                    await SendMessageAsync(stream, "ID:WAVY_2");

                    // Register data type (e.g., "PRESSURE")
                    await SendMessageAsync(stream, "DATA_REG:PRESSURE");

                    // Simulate sending data
                    for (int i = 0; i < 5; i++)
                    {
                        string data = $"Data_{i}";
                        await SendMessageAsync(stream, $"DATA:{data}");
                        await Task.Delay(200); // Simulate time delay
                    }

                    // End session
                    await SendMessageAsync(stream, "END");
                }
            }

            Console.WriteLine("Disconnected from the server.");
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"SocketException: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
    }

    private static async Task SendMessageAsync(NetworkStream stream, string message)
    {
        byte[] msg = Encoding.ASCII.GetBytes(message + "\n"); // Add '\n' delimiter
        await stream.WriteAsync(msg, 0, msg.Length);
        Console.WriteLine($"Sent: {message}");
    }
}
