using System;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            ShowHelp();
            return;
        }

        try
        {
            switch (args[0].ToLower())
            {
                case "servidor":
                    if (args.Length < 2) throw new ArgumentException("Porta do servidor não especificada");
                    new Servidor(int.Parse(args[1])).Start();
                    break;
                    
                case "agregador":
                    if (args.Length < 2) throw new ArgumentException("Porta do agregador não especificada");
                    new Agregador(int.Parse(args[1])).Start();
                    break;
                    
                case "wavy":
                    if (args.Length < 4) throw new ArgumentException("Argumentos da WAVY incompletos");
                    new Wavy(args[1], args[2], int.Parse(args[3])).Start();
                    break;
                    
                default:
                    ShowHelp();
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro: {ex.Message}");
            ShowHelp();
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine("Uso correto:");
        Console.WriteLine("  Servidor: dotnet run servidor <porta>");
        Console.WriteLine("  Agregador: dotnet run agregador <porta>");
        Console.WriteLine("  WAVY: dotnet run wavy <id> <ip_agregador> <porta_agregador>");
    }
}