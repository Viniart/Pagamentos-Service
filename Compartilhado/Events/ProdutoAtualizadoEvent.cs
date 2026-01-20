namespace Compartilhado.Events;

public record ProdutoAtualizadoEvent(
    Guid ProdutoId,
    string Nome,
    decimal Preco,
    int Categoria,
    DateTime DataAtualizacao
);
