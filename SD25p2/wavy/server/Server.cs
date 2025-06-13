using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Collections.Concurrent;
using Grpc.Net.Client;
using AnalysisService; // Namespace gerado pelo proto

namespace Wavy.Serv
{
    public class Server
    {
        // Guarda o estado de cada cliente (pelo endpoint)
        private static ConcurrentDictionary<string, ClientState> clientStates = new();

        private enum ClientState
        {
            AwaitingID,
            AwaitingDataReg,
            AwaitingData,
            Ended
        }

        public static void Main()
        {
            TcpListener server = null;
            try
            {
                Int32 port = 13000;
                IPAddress localAddr = IPAddress.Any;

                server = new TcpListener(localAddr, port);
                server.Start();
                Console.WriteLine("Server Started...");

                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    HandleClient(client);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException : {0}", e);
            }
            finally
            {
                server?.Stop();
                Console.WriteLine("Server stopped.");
            }

            // No fim do ciclo, ou noutro terminal, podes chamar:
            VisualizarDados();
        }

        private static void ProcessCommand(string command, NetworkStream stream, TcpClient client, string clientKey)
        {
            if (!clientStates.TryGetValue(clientKey, out var state))
                state = ClientState.AwaitingID;

            string[] parts = command.Split(':', 2);
            string cmd = parts[0];
            string arg = parts.Length > 1 ? parts[1] : null;

            switch (state)
            {
                case ClientState.AwaitingID:
                    if (cmd == "ID" && arg != null)
                    {
                        HandleID(arg, stream);
                        clientStates[clientKey] = ClientState.AwaitingDataReg;
                    }
                    else
                    {
                        SendError(stream, "Expected ID");
                    }
                    break;
                case ClientState.AwaitingDataReg:
                    if (cmd == "DATA_REG" && arg != null)
                    {
                        HandleDataReg(arg, stream);
                        clientStates[clientKey] = ClientState.AwaitingData;
                    }
                    else
                    {
                        SendError(stream, "Expected DATA_REG");
                    }
                    break;
                case ClientState.AwaitingData:
                    if (cmd == "DATA" && arg != null)
                    {
                        HandleData(arg, stream);
                    }
                    else if (cmd == "END")
                    {
                        HandleEnd(null, stream);
                        clientStates[clientKey] = ClientState.Ended;
                        // Opcional: fechar a ligação aqui
                    }
                    else
                    {
                        SendError(stream, "Expected DATA or END");
                    }
                    break;
                case ClientState.Ended:
                    SendError(stream, "Session already ended");
                    break;
            }
        }

        private static void SendError(NetworkStream stream, string msg)
        {
            byte[] errorMsg = Encoding.ASCII.GetBytes($"ERROR:{msg}\n");
            stream.Write(errorMsg, 0, errorMsg.Length);
        }

        private static void HandleClient(TcpClient client)
        {
            string clientKey = client.Client.RemoteEndPoint.ToString();
            clientStates[clientKey] = ClientState.AwaitingID;

            try
            {
                Console.WriteLine($"[CONNECTION] New client connected: {client.Client.RemoteEndPoint}");
                using (client)
                using (var stream = client.GetStream())
                {
                    var buffer = new byte[1024];
                    var dataBuffer = new StringBuilder();

                    while (true)
                    {
                        var bytesRead = stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break; // Connection closed by client

                        dataBuffer.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));
                        string[] commands = dataBuffer.ToString().Split('\n');
                        dataBuffer.Clear();

                        for (int i = 0; i < commands.Length - 1; i++)
                        {
                            try
                            {
                                ProcessCommand(commands[i].Trim(), stream, client, clientKey);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error processing command: {ex.Message}");
                            }
                        }
                        // Keep the last incomplete command in the buffer
                        if (!string.IsNullOrWhiteSpace(commands[^1]))
                        {
                            dataBuffer.Append(commands[^1]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while handling client: {ex.Message}");
            }
            finally
            {
                clientStates.TryRemove(clientKey, out _);
            }
        }

        private static void HandleID(string id, NetworkStream stream)
        {
            Console.WriteLine("ID received and acknowledged: {0}", id);

            // Save the ID to the CSV file
            string filePath = "received_data.csv";
            try
            {
                using (var writer = new StreamWriter(filePath, append: true))
                {
                    writer.WriteLine($"{DateTime.Now},ID:{id}");
                }
                Console.WriteLine("ID saved to CSV.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving ID to CSV: {ex.Message}");
            }

            // Send acknowledgment back to the client
            byte[] msg = Encoding.ASCII.GetBytes("ID_ACK\n");
            try
            {
                stream.Write(msg, 0, msg.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending ID_ACK: {ex.Message}");
            }
        }

        private static void HandleDataReg(string dataType, NetworkStream stream)
        {
            Console.WriteLine("Data type registered: {0}", dataType);

            // Save the data registration to the CSV file
            string filePath = "received_data.csv";
            try
            {
                using (var writer = new StreamWriter(filePath, append: true))
                {
                    writer.WriteLine($"{DateTime.Now},DATA_REG:{dataType}");
                }
                Console.WriteLine("Data registration saved to CSV.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving DATA_REG to CSV: {ex.Message}");
            }

            // Send acknowledgment back to the client
            byte[] msg = Encoding.ASCII.GetBytes("DATA_REG_ACK\n");
            try
            {
                stream.Write(msg, 0, msg.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending DATA_REG_ACK: {ex.Message}");
            }
        }

        private static void HandleData(string data, NetworkStream stream)
        {
            Console.WriteLine("Data received and acknowledged: {0}", data);

            // Save the data to a CSV file
            string filePath = "received_data.csv";
            try
            {
                using (var writer = new StreamWriter(filePath, append: true))
                {
                    writer.WriteLine($"{DateTime.Now},{data}");
                }
                Console.WriteLine("Data saved to CSV.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data to CSV: {ex.Message}");
            }

            // Send acknowledgment back to the client
            byte[] msg = Encoding.ASCII.GetBytes("DATA_ACK\n");
            try
            {
                stream.Write(msg, 0, msg.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending DATA_ACK: {ex.Message}");
            }
        }

        private static void HandleEnd(string clientId, NetworkStream stream)
        {
            Console.WriteLine("Session ended for client ID: {0}", clientId ?? "Unknown");
            byte[] msg = Encoding.ASCII.GetBytes("END_ACK\n");
            try
            {
                stream.Write(msg, 0, msg.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SUCCESS] Data received and saved.");
            }
        }

        public static async Task<string> AnalyzeDataAsync(string data)
        {
            using var channel = GrpcChannel.ForAddress("http://localhost:5002");
            var client = new AnalysisService.AnalysisServiceClient(channel);

            var request = new AnalysisRequest { Data = data };
            var reply = await client.AnalyzeAsync(request);

            return reply.Result;
        }

        public static void VisualizarDados()
        {
            string filePath = "received_data.csv";
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Ainda não existem dados guardados.");
                return;
            }

            Console.WriteLine("=== Dados Recebidos ===");
            var linhas = File.ReadAllLines(filePath);
            foreach (var linha in linhas)
            {
                Console.WriteLine(linha);
            }
            Console.WriteLine("=======================");
        }
    }
}