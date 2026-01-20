namespace Compartilhado.Events;

public record StatusPedidoAlteradoEvent(
    Guid PedidoId,
    int NovoStatus,
    DateTime DataAlteracao
);
