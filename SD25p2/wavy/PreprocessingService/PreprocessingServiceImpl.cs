using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Aggregator.Preprocessing;

public class PreprocessingServiceImpl : Aggregator.Preprocessing.PreprocessingService.PreprocessingServiceBase
{
    // Guarda os dados processados por ID
    private static readonly ConcurrentDictionary<string, List<string>> processedDataById = new();

    public override Task<PreprocessReply> Preprocess(PreprocessRequest request, ServerCallContext context)
    {
        // Exemplo simples: converter para maiúsculas
        var normalized = request.RawData.ToUpperInvariant();

        // Extrai o ID da mensagem (assumindo formato "valor:ID:timestamp")
        var parts = request.RawData.Split(':');
        var id = parts.Length > 1 ? parts[1] : "UNKNOWN";

        // Guarda o dado processado para o ID
        processedDataById.AddOrUpdate(
            id,
            new List<string> { normalized },
            (key, list) => { list.Add(normalized); return list; }
        );

        return Task.FromResult(new PreprocessReply { NormalizedData = normalized });
    }

    // Método auxiliar para o Aggregator obter todos os dados prontos para enviar ao servidor
    public static Dictionary<string, List<string>> GetAllProcessedDataAndClear()
    {
        var snapshot = new Dictionary<string, List<string>>(processedDataById);
        processedDataById.Clear();
        return snapshot;
    }
}