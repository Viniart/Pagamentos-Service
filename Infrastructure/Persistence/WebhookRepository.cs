using Microsoft.EntityFrameworkCore;
using PagamentoService.Core.Entity;
using PagamentoService.Core.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace PagamentoService.Infrastructure.Persistence;

[ExcludeFromCodeCoverage]
public class WebhookRepository : IWebhookRepository
{
    private readonly PagamentoDbContext _context;

    public WebhookRepository(PagamentoDbContext context)
    {
        _context = context;
    }

    public async Task<WebhookEvent> CriarAsync(WebhookEvent webhookEvent, CancellationToken cancellationToken = default)
    {
        await _context.WebhookEvents.AddAsync(webhookEvent, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return webhookEvent;
    }

    public async Task<WebhookEvent?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.WebhookEvents
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<List<WebhookEvent>> ObterNaoProcessadosAsync(int limite = 100, CancellationToken cancellationToken = default)
    {
        return await _context.WebhookEvents
            .Where(w => !w.Processado)
            .OrderBy(w => w.DataRecebimento)
            .Take(limite)
            .ToListAsync(cancellationToken);
    }

    public async Task<WebhookEvent?> AtualizarAsync(WebhookEvent webhookEvent, CancellationToken cancellationToken = default)
    {
        _context.WebhookEvents.Update(webhookEvent);
        await _context.SaveChangesAsync(cancellationToken);
        return webhookEvent;
    }
}
