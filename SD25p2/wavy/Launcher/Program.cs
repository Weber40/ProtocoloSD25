using System;
using System.Diagnostics;
using Wavy.Serv; // Server

class Program
{
    static void Main(string[] args)
    {
        // Iniciar o aggregator numa nova janela
        Process.Start(new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = "/k dotnet run --project ../aggregator/aggregator.csproj",
            WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
            CreateNoWindow = false,
            UseShellExecute = true
        });

        // Iniciar o wavy numa nova janela
        Process.Start(new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = "/k dotnet run --project ../wavy/wavy.csproj",
            WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
            CreateNoWindow = false,
            UseShellExecute = true
        });

        // Arrancar o servidor na janela atual
        Server.Main();
    }
}
