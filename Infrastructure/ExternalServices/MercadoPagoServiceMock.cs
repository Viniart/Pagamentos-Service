using Microsoft.Extensions.Logging;
using PagamentoService.Core.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace PagamentoService.Infrastructure.ExternalServices;

/// <summary>
/// Mock do serviço Mercado Pago para demonstração.
/// Em produção, substituir por integração real com a API do Mercado Pago.
/// </summary>
[ExcludeFromCodeCoverage]
public class MercadoPagoServiceMock : IMercadoPagoService
{
    private readonly ILogger<MercadoPagoServiceMock> _logger;

    public MercadoPagoServiceMock(ILogger<MercadoPagoServiceMock> logger)
    {
        _logger = logger;
    }

    public Task<QrCodeResponse> GerarQrCodeAsync(
        decimal valor,
        string descricao,
        Guid pedidoId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Gerando QR Code mock para pedido {PedidoId}, valor: {Valor}",
            pedidoId,
            valor);

        // Simulação de QR Code
        var idPagamentoExterno = $"MP-{Guid.NewGuid().ToString("N")[..16].ToUpper()}";
        var qrCodeData = $"00020126580014br.gov.bcb.pix0136{idPagamentoExterno}520400005303986540{valor:F2}5802BR5925Tech Challenge Restaurant6009SAO PAULO62070503***6304";

        var response = new QrCodeResponse(
            qrCodeData,
            idPagamentoExterno,
            "pending"
        );

        _logger.LogInformation(
            "QR Code mock gerado: {IdPagamento}",
            idPagamentoExterno);

        return Task.FromResult(response);
    }

    public Task<StatusPagamentoResponse> ConsultarPagamentoAsync(
        string idPagamentoExterno,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Consultando status mock do pagamento {IdPagamento}",
            idPagamentoExterno);

        // Simulação: retornar sempre como pendente
        var response = new StatusPagamentoResponse(
            idPagamentoExterno,
            "pending",
            null
        );

        return Task.FromResult(response);
    }
}

/*
 * NOTA: Para implementação real do Mercado Pago, utilize:
 *
 * 1. Instalar pacote NuGet: MercadoPago SDK
 * 2. Configurar credenciais (Access Token)
 * 3. Usar endpoints da API:
 *    - POST /v1/payments - Criar pagamento
 *    - GET /v1/payments/{id} - Consultar status
 * 4. Processar webhooks de notificação
 *
 * Documentação: https://www.mercadopago.com.br/developers/pt/docs
 */
