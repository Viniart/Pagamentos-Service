using Compartilhado.Enums;

namespace PagamentoService.Api.DTOs;

public record ClienteResponse(
    Guid Id,
    string Nome,
    string? Email,
    string CPF,
    DateTime DataCadastro
);

public record PagamentoResponse(
    Guid Id,
    Guid PedidoId,
    Guid ClienteId,
    string ClienteNome,
    decimal Valor,
    string? QrCode,
    EStatusPagamento Status,
    string? IdPagamentoExterno,
    DateTime? DataPagamento,
    DateTime DataCriacao
);

public record CriarClienteRequest(string Nome, string? Email, string CPF);
public record AtualizarClienteRequest(string Nome, string? Email);
public record GerarQrCodeRequest(Guid PedidoId, Guid ClienteId, decimal Valor);
