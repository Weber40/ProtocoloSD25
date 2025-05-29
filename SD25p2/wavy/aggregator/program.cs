// filepath: c:\Users\tiago\wavy\aggregator\program.cs
using System;
using System.Threading.Tasks;

namespace Aggregator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        public static async Task MainAsync(string[] args)
        {
            //Modo servidor STD
            Console.WriteLine("Aggregator is running...");
            var aggregator = new Aggregator("localhost", 14001, "localhost", 14000);
            await aggregator.StartAsync();

            var subscriber = new WavySubscriber();
            subscriber.Subscribe("TEMPERATURE", aggregator);
            subscriber.Subscribe("PRESSURE", aggregator);
            // Mantém a aplicação a correr para receber mensagens
            Console.ReadLine();
            subscriber.Close();

            // Modo teste (opcional)
            //Aggregator.TestConnectionToMainServer();
        }
    }
}
