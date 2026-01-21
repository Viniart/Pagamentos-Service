using Microsoft.EntityFrameworkCore;
using PagamentoService.Core.Entity;
using PagamentoService.Core.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace PagamentoService.Infrastructure.Persistence;

[ExcludeFromCodeCoverage]
public class ClienteRepository : IClienteRepository
{
    private readonly PagamentoDbContext _context;

    public ClienteRepository(PagamentoDbContext context)
    {
        _context = context;
    }

    public async Task<Cliente> CriarAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        await _context.Clientes.AddAsync(cliente, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return cliente;
    }

    public async Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Clientes
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Cliente?> ObterPorCPFAsync(string cpf, CancellationToken cancellationToken = default)
    {
        // Limpar CPF para busca
        var cpfLimpo = new string(cpf.Where(char.IsDigit).ToArray());

        return await _context.Clientes
            .FirstOrDefaultAsync(c => c.CPF == cpfLimpo, cancellationToken);
    }

    public async Task<List<Cliente>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Clientes
            .OrderBy(c => c.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<Cliente?> AtualizarAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        _context.Clientes.Update(cliente);
        await _context.SaveChangesAsync(cancellationToken);
        return cliente;
    }
}
