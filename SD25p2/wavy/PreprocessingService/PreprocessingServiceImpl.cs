using System.Threading.Tasks;
using Grpc.Core;
using Aggregator.Preprocessing;

public class PreprocessingServiceImpl : Aggregator.Preprocessing.PreprocessingService.PreprocessingServiceBase
{
    public override Task<PreprocessReply> Preprocess(PreprocessRequest request, ServerCallContext context)
    {
        // Aqui fazes o pré-processamento dos dados recebidos
        // Exemplo simples: converter para maiúsculas
        var normalized = request.RawData.ToUpperInvariant();

        return Task.FromResult(new PreprocessReply { NormalizedData = normalized });
    }
}