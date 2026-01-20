namespace PagamentoService.Core.Entity;

public class WebhookEvent
{
    public Guid Id { get; private set; }
    public Guid? PagamentoId { get; private set; }
    public string TipoEvento { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public bool Processado { get; private set; }
    public DateTime? DataProcessamento { get; private set; }
    public string? ErroProcessamento { get; private set; }
    public DateTime DataRecebimento { get; private set; } = DateTime.UtcNow;

    // Construtor para EF Core
    protected WebhookEvent() { }

    public WebhookEvent(string tipoEvento, string payload)
    {
        if (string.IsNullOrWhiteSpace(tipoEvento))
            throw new ArgumentException("Tipo de evento é obrigatório.");

        if (string.IsNullOrWhiteSpace(payload))
            throw new ArgumentException("Payload é obrigatório.");

        Id = Guid.NewGuid();
        TipoEvento = tipoEvento;
        Payload = payload;
        Processado = false;
        DataRecebimento = DateTime.UtcNow;
    }

    public void MarcarComoProcessado(Guid? pagamentoId = null)
    {
        Processado = true;
        PagamentoId = pagamentoId;
        DataProcessamento = DateTime.UtcNow;
        ErroProcessamento = null;
    }

    public void RegistrarErro(string erro)
    {
        Processado = false;
        ErroProcessamento = erro;
        DataProcessamento = DateTime.UtcNow;
    }
}
