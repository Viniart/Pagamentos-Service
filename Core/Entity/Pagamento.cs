using Compartilhado.Enums;

namespace PagamentoService.Core.Entity;

public class Pagamento
{
    public Guid Id { get; private set; }
    public Guid PedidoId { get; private set; }
    public Guid ClienteId { get; private set; }
    public decimal Valor { get; private set; }
    public string? QrCode { get; private set; }
    public EStatusPagamento Status { get; private set; }
    public string? IdPagamentoExterno { get; private set; }
    public DateTime? DataPagamento { get; private set; }
    public DateTime DataCriacao { get; private set; } = DateTime.UtcNow;
    public DateTime DataAtualizacao { get; private set; } = DateTime.UtcNow;

    public virtual Cliente Cliente { get; private set; } = null!;

    protected Pagamento() { }

    public Pagamento(Guid pedidoId, Guid clienteId, decimal valor)
    {
        if (valor <= 0)
            throw new ArgumentException("Valor do pagamento deve ser maior que zero.");

        Id = Guid.NewGuid();
        PedidoId = pedidoId;
        ClienteId = clienteId;
        Valor = valor;
        Status = EStatusPagamento.Pendente;
        DataCriacao = DateTime.UtcNow;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void DefinirQrCode(string qrCode, string? idPagamentoExterno = null)
    {
        if (string.IsNullOrWhiteSpace(qrCode))
            throw new ArgumentException("QR Code não pode ser vazio.");

        QrCode = qrCode;
        IdPagamentoExterno = idPagamentoExterno;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void ConfirmarPagamento(DateTime dataPagamento)
    {
        if (Status == EStatusPagamento.Aprovado)
            throw new InvalidOperationException("Pagamento já foi confirmado.");

        Status = EStatusPagamento.Aprovado;
        DataPagamento = dataPagamento;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void RejeitarPagamento()
    {
        if (Status == EStatusPagamento.Aprovado)
            throw new InvalidOperationException("Não é possível rejeitar um pagamento já aprovado.");

        Status = EStatusPagamento.Rejeitado;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void CancelarPagamento()
    {
        if (Status == EStatusPagamento.Aprovado)
            throw new InvalidOperationException("Não é possível cancelar um pagamento já aprovado.");

        Status = EStatusPagamento.Cancelado;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void AtualizarStatus(EStatusPagamento novoStatus)
    {
        if (!Enum.IsDefined(typeof(EStatusPagamento), novoStatus))
            throw new ArgumentException("Status inválido.");

        Status = novoStatus;
        DataAtualizacao = DateTime.UtcNow;

        if (novoStatus == EStatusPagamento.Aprovado && DataPagamento == null)
        {
            DataPagamento = DateTime.UtcNow;
        }
    }
}
