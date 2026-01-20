namespace Compartilhado.Events;

public record PagamentoConfirmadoEvent(
    Guid PagamentoId,
    Guid PedidoId,
    decimal Valor,
    DateTime DataConfirmacao
);
