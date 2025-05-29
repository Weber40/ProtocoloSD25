using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Linq;
using System.Collections.Generic;

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

            // Save the data to the CSV file
            string filePath = "received_data.csv";
            try
            {
                using (var writer = new StreamWriter(filePath, append: true))
                {
                    writer.WriteLine($"{DateTime.Now},DATA:{data}");
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

        public static void DisplayMenu()
        {
            // ...
        }

        private static void ListarDados()
        {
            string filePath = "received_data.csv";
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Nenhum dado registado.");
                return;
            }
            Console.WriteLine("\n--- Dados Recebidos ---");
            foreach (var line in File.ReadAllLines(filePath))
            {
                Console.WriteLine(line);
            }
        }

        private static void ListarAnalises()
        {
            string filePath = "analysis_results.csv";
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Nenhuma análise registada.");
                return;
            }
            Console.WriteLine("\n--- Resultados das Análises ---");
            foreach (var line in File.ReadAllLines(filePath))
            {
                Console.WriteLine(line);
            }
        }

        private static void NovaAnalise()
        {
            Console.WriteLine("\nTipo de análise disponível: Média de temperatura");
            Console.Write("Introduza o intervalo inicial (yyyy-MM-dd HH:mm:ss): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime inicio))
            {
                Console.WriteLine("Data inválida.");
                return;
            }
            Console.Write("Introduza o intervalo final (yyyy-MM-dd HH:mm:ss): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime fim))
            {
                Console.WriteLine("Data inválida.");
                return;
            }

            string filePath = "received_data.csv";
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Nenhum dado registado.");
                return;
            }

            var temperaturas = new List<double>();
            foreach (var line in File.ReadAllLines(filePath))
            {
                var parts = line.Split(',');
                if (parts.Length < 3) continue;
                if (!DateTime.TryParse(parts[0], out DateTime dataHora)) continue;
                if (dataHora < inicio || dataHora > fim) continue;
                if (parts[1].Contains("DATA:TEMPERATURE"))
                {
                    var tempStr = parts[1].Split(':');
                    if (tempStr.Length == 3 && double.TryParse(tempStr[2], out double temp))
                    {
                        temperaturas.Add(temp);
                    }
                    else if (parts[2].StartsWith("DATA:TEMPERATURE"))
                    {
                        var tempVal = parts[2].Split(':');
                        if (tempVal.Length == 3 && double.TryParse(tempVal[2], out double temp2))
                            temperaturas.Add(temp2);
                    }
                }
                else if (parts[1].StartsWith("DATA:TEMPERATURE"))
                {
                    var tempVal = parts[1].Split(':');
                    if (tempVal.Length == 3 && double.TryParse(tempVal[2], out double temp2))
                        temperaturas.Add(temp2);
                }
            }

            if (temperaturas.Count == 0)
            {
                Console.WriteLine("Nenhuma leitura de temperatura encontrada no intervalo.");
                return;
            }

            double media = temperaturas.Average();
            string resultado = $"{DateTime.Now},MEDIA_TEMPERATURA,{inicio} a {fim},{media:F2}";
            Console.WriteLine("Resultado: " + resultado);

            // Guarda o resultado da análise
            string analysisFile = "analysis_results.csv";
            using (var writer = new StreamWriter(analysisFile, append: true))
            {
                writer.WriteLine(resultado);
            }
            Console.WriteLine("Resultado guardado em analysis_results.csv");
        }
    }
}