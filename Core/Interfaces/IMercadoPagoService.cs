namespace PagamentoService.Core.Interfaces;

public interface IMercadoPagoService
{
    Task<QrCodeResponse> GerarQrCodeAsync(decimal valor, string descricao, Guid pedidoId, CancellationToken cancellationToken = default);
    Task<StatusPagamentoResponse> ConsultarPagamentoAsync(string idPagamentoExterno, CancellationToken cancellationToken = default);
}

public record QrCodeResponse(string QrCode, string IdPagamentoExterno, string Status);
public record StatusPagamentoResponse(string IdPagamento, string Status, DateTime? DataPagamento);
