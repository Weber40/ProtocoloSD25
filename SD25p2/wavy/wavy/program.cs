// filepath: c:\Users\tiago\wavy\wavy\program.cs
using System;

namespace Wavy
{
    public class Program
    {
        public static void Main(string[] args)
        {
            SimulateOperation(args);
        }

        static void SimulateOperation(string[] args)
        {
            Console.WriteLine("wavy is running...");
            // Simulate wavy operations

            var publisher = new WavyPublisher();
            var rand = new Random();

            // Gera 10 leituras simuladas
            for (int i = 0; i < 10; i++)
            {
                // Temperatura realista do oceano: 10°C a 30°C
                double temp = Math.Round(rand.NextDouble() * 20 + 10, 2);
                // Pressão atmosférica realista: 980 a 1050 hPa
                double pressure = Math.Round(rand.NextDouble() * 70 + 980, 2);

                publisher.Publish("TEMPERATURE", temp.ToString());
                publisher.Publish("PRESSURE", pressure.ToString());

                System.Threading.Thread.Sleep(1000); // Espera 1 segundo entre leituras
            }
            publisher.Close();
        }
    }
}
