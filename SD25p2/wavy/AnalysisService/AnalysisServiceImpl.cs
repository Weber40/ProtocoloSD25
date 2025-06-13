using Analysis;
using Grpc.Core;
using AnalysisServiceBase;

1public class AnalysisServiceImpl : AnalysisService.AnalysisServiceBase
    public override Task<AnalysisReply> Analyze(AnalysisRequest request, ServerCallContext context)
{
    // Exemplo: análise simples
    var result = $"Análise: {request.Data.Length} caracteres";
    return Task.FromResult(new AnalysisReply { Result = result });
}
