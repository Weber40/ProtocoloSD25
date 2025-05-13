using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;

namespace Wavy.Serv
{
    public class Server
    {
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
        }

        private static void ProcessCommand(string command, NetworkStream stream, TcpClient client)
        {
            Console.WriteLine("Data Received: {0}", command);
            string[] parts = command.Split(':', 2); // Split into at most 2 parts
            if (parts.Length < 2 && command != "END") // Allow "END" as a standalone command
            {
                Console.WriteLine("Invalid data format received.");
                return;
            }

            switch (parts[0])
            {
                case "ID":
                    HandleID(parts[1], stream);
                    break;
                case "DATA_REG":
                    HandleDataReg(parts[1], stream);
                    break;
                case "DATA":
                    HandleData(parts[1], stream);
                    break;
                case "END":
                    HandleEnd(null, stream); // No additional data for END
                    break;
                default:
                    Console.WriteLine("Unknown command received: {0}", parts[0]);
                    break;
            }
        }

        private static void HandleClient(TcpClient client)
        {
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
                                ProcessCommand(commands[i].Trim(), stream, client);
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
    }
}