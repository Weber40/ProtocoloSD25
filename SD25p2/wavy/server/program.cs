using System;
using System.Threading.Tasks;

namespace Wavy.Server.ProgramNamespace
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Server Running");
            await SomeAsyncMethod();
        }

        private static async Task SomeAsyncMethod()
        {
            // Simulate an asynchronous operation
            await Task.Delay(1000);
            Console.WriteLine("Async method completed.");
        }
    }
}