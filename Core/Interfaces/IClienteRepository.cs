using PagamentoService.Core.Entity;

namespace PagamentoService.Core.Interfaces;

public interface IClienteRepository
{
    Task<Cliente> CriarAsync(Cliente cliente, CancellationToken cancellationToken = default);
    Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Cliente?> ObterPorCPFAsync(string cpf, CancellationToken cancellationToken = default);
    Task<List<Cliente>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task<Cliente?> AtualizarAsync(Cliente cliente, CancellationToken cancellationToken = default);
}
