using PagamentoService.Core.Entity;
using PagamentoService.Core.Interfaces;

namespace PagamentoService.Core.UseCases;

public class ClienteUseCases
{
    private readonly IClienteRepository _repository;

    public ClienteUseCases(IClienteRepository repository)
    {
        _repository = repository;
    }

    public async Task<Cliente> CriarClienteAsync(
        string nome,
        string? email,
        string cpf,
        CancellationToken cancellationToken = default)
    {
        // Verificar se já existe cliente com este CPF
        var clienteExistente = await _repository.ObterPorCPFAsync(cpf, cancellationToken);
        if (clienteExistente != null)
            throw new InvalidOperationException("Já existe um cliente cadastrado com este CPF.");

        var cliente = new Cliente(nome, email, cpf);
        return await _repository.CriarAsync(cliente, cancellationToken);
    }

    public async Task<Cliente?> ObterClientePorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _repository.ObterPorIdAsync(id, cancellationToken);
    }

    public async Task<Cliente?> ObterClientePorCPFAsync(string cpf, CancellationToken cancellationToken = default)
    {
        return await _repository.ObterPorCPFAsync(cpf, cancellationToken);
    }

    public async Task<List<Cliente>> ObterTodosClientesAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.ObterTodosAsync(cancellationToken);
    }

    public async Task<Cliente> AtualizarClienteAsync(
        Guid id,
        string nome,
        string? email,
        CancellationToken cancellationToken = default)
    {
        var cliente = await _repository.ObterPorIdAsync(id, cancellationToken);
        if (cliente == null)
            throw new InvalidOperationException("Cliente não encontrado.");

        cliente.AtualizarDados(nome, email);
        await _repository.AtualizarAsync(cliente, cancellationToken);

        return cliente;
    }
}
