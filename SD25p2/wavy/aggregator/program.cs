// filepath: c:\Users\tiago\wavy\aggregator\program.cs
using System;
using System.Threading.Tasks;


namespace Aggregator
{
    class Program
    {


        static async Task Main(string[] args)
        {
            //Modo servidor STD
            Console.WriteLine("Aggregator is running...");
            var aggregator = new Aggregator("localhost", 13001, "localhost", 13000);
            await aggregator.StartAsync();

            // Modo teste (opcional)
            //Aggregator.TestConnectionToMainServer();


        }
    }
}
