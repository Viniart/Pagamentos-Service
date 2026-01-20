using MassTransit;
using Compartilhado.Events;
using PagamentoService.Core.UseCases;

namespace PagamentoService.Api.Consumers;

public class PedidoCriadoConsumer : IConsumer<PedidoCriadoEvent>
{
    private readonly PagamentoUseCases _pagamentoUseCases;
    private readonly ILogger<PedidoCriadoConsumer> _logger;

    public PedidoCriadoConsumer(
        PagamentoUseCases pagamentoUseCases,
        ILogger<PedidoCriadoConsumer> logger)
    {
        _pagamentoUseCases = pagamentoUseCases;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PedidoCriadoEvent> context)
    {
        var evento = context.Message;

        _logger.LogInformation(
            "Processando PedidoCriado: PedidoId={PedidoId}, ClienteId={ClienteId}, Valor={Valor}",
            evento.PedidoId,
            evento.ClienteId,
            evento.ValorTotal);

        try
        {
            var pagamento = await _pagamentoUseCases.CriarPagamentoAsync(
                evento.PedidoId,
                evento.ClienteId,
                evento.ValorTotal,
                gerarQrCode: true,
                context.CancellationToken);

            _logger.LogInformation(
                "Pagamento criado: PagamentoId={PagamentoId}",
                pagamento.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erro ao processar PedidoCriadoEvent: PedidoId={PedidoId}",
                evento.PedidoId);
            throw;
        }
    }
}
