using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aggregator
{
    public class Aggregator
    {

        private readonly string _wavyServerAddress;
        private readonly int _wavyServerPort;
        private readonly string _mainServerAddress;
        private readonly int _mainServerPort;
        private readonly List<string> _pendingData = new List<string>();
        private readonly Timer _timer;

        public static async Task TestConnectionToMainServerAsync(string[] args)
        {
            int port = 14000;
            string serverAddress = "localhost";


            // Apenas mensagens de teste 
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    Console.WriteLine("[TEST] Connecting to MAIN server...");
                    await client.ConnectAsync(serverAddress, port);

                    using (NetworkStream stream = client.GetStream())
                    {
                        // Send ID to SERVER
                        await SendMessageAsync(stream, "ID:WAVY_1");

                        // Register data type (e.g., "TEMPERATURE")
                        await SendMessageAsync(stream, "DATA_REG:TEMPERATURE");

                        // Simulate sending data
                        for (int i = 0; i < 5; i++)
                        {
                            string data = $"Data_{i}";
                            await SendMessageAsync(stream, $"DATA:{data}");
                            await Task.Delay(1000); // Simulate time delay
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

        public Aggregator(string wavyServerAddress, int wavyServerPort, string mainServerAddress, int mainServerPort)
        {
            _wavyServerAddress = wavyServerAddress;
            _wavyServerPort = wavyServerPort;
            _mainServerAddress = mainServerAddress;
            _mainServerPort = mainServerPort;
            _pendingData = new List<string>();
            _timer = new Timer(SendDataToMainServer, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }

        public async Task StartAsync()
        {
            Console.WriteLine("Aggregator started. Waiting for WAVY connections...");
            var listener = new TcpListener(System.Net.IPAddress.Any, _wavyServerPort);
            listener.Start();

            _ = Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(30000);
                    SendDataToMainServer(null);
                }
            });

            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                _ = HandleWavyConnectionAsync(client);
            }
        }

        private async Task HandleWavyConnectionAsync(TcpClient client)
        {
            try
            {
                Console.WriteLine($"[CONNECTION] New WAVY connected: {client.Client.RemoteEndPoint}");
                using (client)
                using (var stream = client.GetStream())
                {
                    var buffer = new byte[1024];
                    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    var message = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();

                    lock (_pendingData)
                    {
                        _pendingData.Add(message);
                        Console.WriteLine($"[DEBUG] Added to queue: {message} (Total: {_pendingData.Count})");
                        Console.WriteLine($"Received from WAVY: {message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling WAVY connection: {ex.Message}");
            }
        }

        private async void SendDataToMainServer(object state)
        {
            // Console.WriteLine("[DEBUG] Timer triggered: checking for data to send...");
            Console.WriteLine($"[TIMER] Checking data at {DateTime.Now}");

            if (_pendingData.Count == 0) return;

            List<string> dataToSend;
            lock (_pendingData)
            {
                if (_pendingData.Count == 0) return;
                dataToSend = new List<string>(_pendingData);
                _pendingData.Clear();
            }

            try
            {
                using (var client = new TcpClient())
                {
                    await client.ConnectAsync(_mainServerAddress, _mainServerPort);
                    using (var stream = client.GetStream())
                    {
                        foreach (var data in dataToSend)
                        {
                            await SendMessageAsync(stream, $"DATA:{data}");
                        }
                        Console.WriteLine($"Sent {dataToSend.Count} items to main server.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending data to main server: {ex.Message}");
                Console.WriteLine($"[DEBUG] Re-adding {dataToSend.Count} items to queue...");
                lock (_pendingData)
                {
                    _pendingData.InsertRange(0, dataToSend); // Requeue dos dados falhados
                }
            }
        }
        private static async Task SendMessageAsync(NetworkStream stream, string message)
        {
            byte[] msg = Encoding.ASCII.GetBytes(message + "\n"); // Add '\n' delimitador
            await stream.WriteAsync(msg, 0, msg.Length);
            Console.WriteLine($"Sent: {message}");
        }

    }
}
