using PagamentoService.Core.Entity;
using PagamentoService.Core.Interfaces;
using Compartilhado.Enums;

namespace PagamentoService.Core.UseCases;

public class PagamentoUseCases
{
    private readonly IPagamentoRepository _repository;
    private readonly IMercadoPagoService _mercadoPagoService;

    public PagamentoUseCases(
        IPagamentoRepository repository,
        IMercadoPagoService mercadoPagoService)
    {
        _repository = repository;
        _mercadoPagoService = mercadoPagoService;
    }

    public async Task<Pagamento> CriarPagamentoAsync(
        Guid pedidoId,
        Guid clienteId,
        decimal valor,
        bool gerarQrCode = true,
        CancellationToken cancellationToken = default)
    {
        // Verificar se já existe pagamento para este pedido
        var pagamentoExistente = await _repository.ObterPorPedidoIdAsync(pedidoId, cancellationToken);
        if (pagamentoExistente != null)
            throw new InvalidOperationException("Já existe um pagamento para este pedido.");

        var pagamento = new Pagamento(pedidoId, clienteId, valor);
        var pagamentoCriado = await _repository.CriarAsync(pagamento, cancellationToken);

        // Gerar QR Code via Mercado Pago
        if (gerarQrCode)
        {
            var qrCodeResponse = await _mercadoPagoService.GerarQrCodeAsync(
                valor,
                $"Pagamento Pedido #{pedidoId}",
                pedidoId,
                cancellationToken);

            pagamentoCriado.DefinirQrCode(qrCodeResponse.QrCode, qrCodeResponse.IdPagamentoExterno);
            await _repository.AtualizarAsync(pagamentoCriado, cancellationToken);
        }

        return pagamentoCriado;
    }

    public async Task<Pagamento?> ObterPagamentoPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _repository.ObterPorIdAsync(id, cancellationToken);
    }

    public async Task<Pagamento?> ObterPagamentoPorPedidoIdAsync(Guid pedidoId, CancellationToken cancellationToken = default)
    {
        return await _repository.ObterPorPedidoIdAsync(pedidoId, cancellationToken);
    }

    public async Task<List<Pagamento>> ObterPagamentosPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        return await _repository.ObterPorClienteIdAsync(clienteId, cancellationToken);
    }

    public async Task<Pagamento> ConfirmarPagamentoAsync(
        string idPagamentoExterno,
        DateTime dataPagamento,
        CancellationToken cancellationToken = default)
    {
        var pagamento = await _repository.ObterPorIdPagamentoExternoAsync(idPagamentoExterno, cancellationToken);
        if (pagamento == null)
            throw new InvalidOperationException("Pagamento não encontrado.");

        pagamento.ConfirmarPagamento(dataPagamento);
        await _repository.AtualizarAsync(pagamento, cancellationToken);

        return pagamento;
    }

    public async Task<Pagamento> RejeitarPagamentoAsync(
        string idPagamentoExterno,
        CancellationToken cancellationToken = default)
    {
        var pagamento = await _repository.ObterPorIdPagamentoExternoAsync(idPagamentoExterno, cancellationToken);
        if (pagamento == null)
            throw new InvalidOperationException("Pagamento não encontrado.");

        pagamento.RejeitarPagamento();
        await _repository.AtualizarAsync(pagamento, cancellationToken);

        return pagamento;
    }

    public async Task<StatusPagamentoResponse> ConsultarStatusPagamentoAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var pagamento = await _repository.ObterPorIdAsync(id, cancellationToken);
        if (pagamento == null)
            throw new InvalidOperationException("Pagamento não encontrado.");

        if (string.IsNullOrEmpty(pagamento.IdPagamentoExterno))
            throw new InvalidOperationException("Pagamento não possui ID externo.");

        return await _mercadoPagoService.ConsultarPagamentoAsync(
            pagamento.IdPagamentoExterno,
            cancellationToken);
    }
}
