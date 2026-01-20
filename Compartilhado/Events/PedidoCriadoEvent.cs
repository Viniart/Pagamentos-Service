namespace Compartilhado.Events;

public record PedidoCriadoEvent(
    Guid PedidoId,
    Guid ClienteId,
    decimal ValorTotal,
    DateTime DataCriacao
);
