using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Aggregator.Preprocessing; // Namespace gerado pelo proto

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

            // Create an instance of Aggregator
            var aggregator = new Aggregator("localhost", 14001, serverAddress, port);

            try
            {
                using (TcpClient client = new TcpClient())
                {
                    Console.WriteLine("[TEST] Connecting to MAIN server...");
                    await client.ConnectAsync(serverAddress, port);

                    using (NetworkStream stream = client.GetStream())
                    {
                        // Use the instance to call SendMessageAsync
                        await aggregator.SendMessageAsync(stream, "ID:WAVY_1");
                        await aggregator.SendMessageAsync(stream, "DATA_REG:TEMPERATURE");

                        for (int i = 0; i < 5; i++)
                        {
                            string data = $"Data_{i}";
                            await aggregator.SendMessageAsync(stream, $"DATA:{data}");
                            await Task.Delay(1000); // Simulate time delay
                        }

                        await aggregator.SendMessageAsync(stream, "CLEAN");
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

            // Set the timer to trigger every 10 seconds
            _timer = new Timer(SendDataToMainServer, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
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
                    while (true)
                    {
                        var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break; // Connection closed by client

                        var message = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
                        lock (_pendingData)
                        {
                            _pendingData.Add(message);
                            Console.WriteLine($"[DEBUG] Added to queue: {message} (Total: {_pendingData.Count})");
                            Console.WriteLine($"Received from WAVY: {message}");
                        }
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
                            // Only prefix with "DATA:" if it's actual data, not a command
                            if (data.StartsWith("ID:") || data.StartsWith("DATA_REG:"))
                            {
                                await SendMessageAsync(stream, data);
                            }
                            else
                            {
                                await SendMessageAsync(stream, $"DATA:{data}");
                            }
                        }

                        // Send the END command after all other commands
                        await SendMessageAsync(stream, "END");
                        Console.WriteLine($"[INFO] Sent {dataToSend.Count + 1} items to main server (including END).");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error sending data to main server: {ex.Message}");
                Console.WriteLine($"[DEBUG] Re-adding {dataToSend.Count} items to queue...");
                lock (_pendingData)
                {
                    _pendingData.InsertRange(0, dataToSend); // Requeue failed data
                }
            }
        }

        private async Task SendMessageAsync(NetworkStream stream, string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message + "\n");
            await stream.WriteAsync(data, 0, data.Length);

            // Wait for acknowledgment
            var buffer = new byte[1024];
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            string response = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
            Console.WriteLine($"[DEBUG] Server response: {response}");
        }

        public async Task<string> PreprocessDataAsync(string rawData)
        {
            using var channel = GrpcChannel.ForAddress("http://localhost:5001"); // Endereço do serviço gRPC
            var client = new PreprocessingService.PreprocessingServiceClient(channel);

            var request = new PreprocessRequest { RawData = rawData };
            var reply = await client.PreprocessAsync(request);

            return reply.NormalizedData;
        }
    }
}
