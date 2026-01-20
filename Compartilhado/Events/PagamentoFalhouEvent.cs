namespace Compartilhado.Events;

public record PagamentoFalhouEvent(
    Guid PagamentoId,
    Guid PedidoId,
    string Motivo,
    DateTime DataFalha
);
