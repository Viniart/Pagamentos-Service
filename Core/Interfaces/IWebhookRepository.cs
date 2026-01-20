using PagamentoService.Core.Entity;

namespace PagamentoService.Core.Interfaces;

public interface IWebhookRepository
{
    Task<WebhookEvent> CriarAsync(WebhookEvent webhookEvent, CancellationToken cancellationToken = default);
    Task<WebhookEvent?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<WebhookEvent>> ObterNaoProcessadosAsync(int limite = 100, CancellationToken cancellationToken = default);
    Task<WebhookEvent?> AtualizarAsync(WebhookEvent webhookEvent, CancellationToken cancellationToken = default);
}
