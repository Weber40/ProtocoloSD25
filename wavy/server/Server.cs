using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Wavy.Server
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
                    Console.WriteLine("Client connected.");

                    NetworkStream stream = client.GetStream();
                    Byte[] bytes = new Byte[256];
                    StringBuilder dataBuffer = new StringBuilder();
                    string clientId = null; // Store the client ID
                    string dataType = null; // Store the registered data type

                    try
                    {
                        int i;
                        while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            // Append received data to the buffer
                            dataBuffer.Append(Encoding.ASCII.GetString(bytes, 0, i));

                            // Process complete commands separated by '\n'
                            string[] commands = dataBuffer.ToString().Split('\n');
                            dataBuffer.Clear(); // Clear the buffer

                            for (int j = 0; j < commands.Length; j++)
                            {
                                string command = commands[j].Trim();
                                if (string.IsNullOrEmpty(command))
                                    continue;

                                Console.WriteLine("Data Received : {0}", command);

                                string[] parts = command.Split(':');
                                if (parts.Length < 2 && parts[0] != "END")
                                {
                                    Console.WriteLine("Invalid data format received.");
                                    continue;
                                }

                                switch (parts[0])
                                {
                                    case "ID":
                                        clientId = parts[1]; // Store the client ID
                                        HandleID(clientId, stream);
                                        break;
                                    case "DATA_REG":
                                        dataType = parts[1]; // Store the registered data type
                                        HandleDataReg(dataType, stream);
                                        break;
                                    case "DATA":
                                        HandleData(parts[1], stream);
                                        break;
                                    case "END":
                                        HandleEnd(clientId, stream);
                                        client.Close(); // Close the client connection
                                        return; // Exit the loop
                                    default:
                                        Console.WriteLine("Unknown command received: {0}", parts[0]);
                                        break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error while handling client: {0}", ex.Message);
                    }
                    finally
                    {
                        client.Close();
                    }
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

        private static void HandleID(string id, NetworkStream stream)
        {
            Console.WriteLine("ID received and acknowledged: {0}", id);
            byte[] msg = Encoding.ASCII.GetBytes("ID_ACK\n");
            stream.Write(msg, 0, msg.Length);
        }

        private static void HandleDataReg(string dataType, NetworkStream stream)
        {
            Console.WriteLine("Data type registered: {0}", dataType);
            byte[] msg = Encoding.ASCII.GetBytes("DATA_REG_ACK\n");
            stream.Write(msg, 0, msg.Length);
        }

        private static void HandleData(string data, NetworkStream stream)
        {
            Console.WriteLine("Data received and acknowledged: {0}", data);
            byte[] msg = Encoding.ASCII.GetBytes("DATA_ACK\n");
            stream.Write(msg, 0, msg.Length);
        }

        private static void HandleEnd(string clientId, NetworkStream stream)
        {
            Console.WriteLine("Session ended for client ID: {0}", clientId ?? "Unknown");
            byte[] msg = Encoding.ASCII.GetBytes("END_ACK\n");
            stream.Write(msg, 0, msg.Length);
        }
    }
}