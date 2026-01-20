using Microsoft.EntityFrameworkCore;
using PagamentoService.Core.Entity;
using PagamentoService.Core.Interfaces;
using Compartilhado.Enums;

namespace PagamentoService.Infrastructure.Persistence;

public class PagamentoRepository : IPagamentoRepository
{
    private readonly PagamentoDbContext _context;

    public PagamentoRepository(PagamentoDbContext context)
    {
        _context = context;
    }

    public async Task<Pagamento> CriarAsync(Pagamento pagamento, CancellationToken cancellationToken = default)
    {
        await _context.Pagamentos.AddAsync(pagamento, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return pagamento;
    }

    public async Task<Pagamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Pagamentos
            .Include(p => p.Cliente)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Pagamento?> ObterPorPedidoIdAsync(Guid pedidoId, CancellationToken cancellationToken = default)
    {
        return await _context.Pagamentos
            .Include(p => p.Cliente)
            .FirstOrDefaultAsync(p => p.PedidoId == pedidoId, cancellationToken);
    }

    public async Task<Pagamento?> ObterPorIdPagamentoExternoAsync(string idPagamentoExterno, CancellationToken cancellationToken = default)
    {
        return await _context.Pagamentos
            .Include(p => p.Cliente)
            .FirstOrDefaultAsync(p => p.IdPagamentoExterno == idPagamentoExterno, cancellationToken);
    }

    public async Task<List<Pagamento>> ObterPorClienteIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        return await _context.Pagamentos
            .Include(p => p.Cliente)
            .Where(p => p.ClienteId == clienteId)
            .OrderByDescending(p => p.DataCriacao)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Pagamento>> ObterPorStatusAsync(EStatusPagamento status, CancellationToken cancellationToken = default)
    {
        return await _context.Pagamentos
            .Include(p => p.Cliente)
            .Where(p => p.Status == status)
            .OrderBy(p => p.DataCriacao)
            .ToListAsync(cancellationToken);
    }

    public async Task<Pagamento?> AtualizarAsync(Pagamento pagamento, CancellationToken cancellationToken = default)
    {
        _context.Pagamentos.Update(pagamento);
        await _context.SaveChangesAsync(cancellationToken);
        return pagamento;
    }
}
