using PagamentoService.Core.Entity;
using Compartilhado.Enums;

namespace PagamentoService.Core.Interfaces;

public interface IPagamentoRepository
{
    Task<Pagamento> CriarAsync(Pagamento pagamento, CancellationToken cancellationToken = default);
    Task<Pagamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Pagamento?> ObterPorPedidoIdAsync(Guid pedidoId, CancellationToken cancellationToken = default);
    Task<Pagamento?> ObterPorIdPagamentoExternoAsync(string idPagamentoExterno, CancellationToken cancellationToken = default);
    Task<List<Pagamento>> ObterPorClienteIdAsync(Guid clienteId, CancellationToken cancellationToken = default);
    Task<List<Pagamento>> ObterPorStatusAsync(EStatusPagamento status, CancellationToken cancellationToken = default);
    Task<Pagamento?> AtualizarAsync(Pagamento pagamento, CancellationToken cancellationToken = default);
}
