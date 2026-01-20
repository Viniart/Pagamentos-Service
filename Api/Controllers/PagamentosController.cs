using Microsoft.AspNetCore.Mvc;
using PagamentoService.Core.UseCases;
using PagamentoService.Core.Entity;
using PagamentoService.Api.DTOs;
using Compartilhado.Events;
using MassTransit;

namespace PagamentoService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PagamentosController : ControllerBase
{
    private readonly PagamentoUseCases _pagamentoUseCases;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<PagamentosController> _logger;

    public PagamentosController(
        PagamentoUseCases pagamentoUseCases,
        IPublishEndpoint publishEndpoint,
        ILogger<PagamentosController> logger)
    {
        _pagamentoUseCases = pagamentoUseCases;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    [HttpPost("gerar-qrcode")]
    public async Task<ActionResult<PagamentoResponse>> GerarQrCode(
        [FromBody] GerarQrCodeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var pagamento = await _pagamentoUseCases.CriarPagamentoAsync(
                request.PedidoId,
                request.ClienteId,
                request.Valor,
                gerarQrCode: true,
                cancellationToken);

            _logger.LogInformation(
                "Pagamento {PagamentoId} criado para pedido {PedidoId}",
                pagamento.Id,
                request.PedidoId);

            return CreatedAtAction(
                nameof(ObterPorId),
                new { id = pagamento.Id },
                MapearParaResponse(pagamento));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { erro = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar QR Code");
            return StatusCode(500, new { erro = "Erro ao gerar QR Code" });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PagamentoResponse>> ObterPorId(
        Guid id,
        CancellationToken cancellationToken)
    {
        var pagamento = await _pagamentoUseCases.ObterPagamentoPorIdAsync(id, cancellationToken);
        if (pagamento == null)
            return NotFound(new { erro = "Pagamento não encontrado" });

        return Ok(MapearParaResponse(pagamento));
    }

    [HttpGet("pedido/{pedidoId:guid}")]
    public async Task<ActionResult<PagamentoResponse>> ObterPorPedidoId(
        Guid pedidoId,
        CancellationToken cancellationToken)
    {
        var pagamento = await _pagamentoUseCases.ObterPagamentoPorPedidoIdAsync(pedidoId, cancellationToken);
        if (pagamento == null)
            return NotFound(new { erro = "Pagamento não encontrado para este pedido" });

        return Ok(MapearParaResponse(pagamento));
    }

    [HttpGet("cliente/{clienteId:guid}")]
    public async Task<ActionResult<List<PagamentoResponse>>> ObterPorClienteId(
        Guid clienteId,
        CancellationToken cancellationToken)
    {
        var pagamentos = await _pagamentoUseCases.ObterPagamentosPorClienteAsync(clienteId, cancellationToken);
        return Ok(pagamentos.Select(MapearParaResponse));
    }

    [HttpGet("{id:guid}/status")]
    public async Task<ActionResult<object>> ConsultarStatus(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var status = await _pagamentoUseCases.ConsultarStatusPagamentoAsync(id, cancellationToken);
            return Ok(new
            {
                idPagamento = status.IdPagamento,
                status = status.Status,
                dataPagamento = status.DataPagamento
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { erro = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar status do pagamento {PagamentoId}", id);
            return StatusCode(500, new { erro = "Erro ao consultar status" });
        }
    }

    [HttpPost("{idPagamentoExterno}/confirmar")]
    public async Task<ActionResult<PagamentoResponse>> ConfirmarPagamento(
        string idPagamentoExterno,
        CancellationToken cancellationToken)
    {
        try
        {
            var pagamento = await _pagamentoUseCases.ConfirmarPagamentoAsync(
                idPagamentoExterno,
                DateTime.UtcNow,
                cancellationToken);

            // Publicar evento de pagamento confirmado
            await _publishEndpoint.Publish(new PagamentoConfirmadoEvent(
                pagamento.Id,
                pagamento.PedidoId,
                pagamento.Valor,
                DateTime.UtcNow
            ), cancellationToken);

            _logger.LogInformation(
                "Pagamento {PagamentoId} confirmado para pedido {PedidoId}",
                pagamento.Id,
                pagamento.PedidoId);

            return Ok(MapearParaResponse(pagamento));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { erro = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao confirmar pagamento {IdPagamentoExterno}", idPagamentoExterno);
            return StatusCode(500, new { erro = "Erro ao confirmar pagamento" });
        }
    }

    private static PagamentoResponse MapearParaResponse(Pagamento pagamento)
    {
        return new PagamentoResponse(
            pagamento.Id,
            pagamento.PedidoId,
            pagamento.ClienteId,
            pagamento.Cliente?.Nome ?? "",
            pagamento.Valor,
            pagamento.QrCode,
            pagamento.Status,
            pagamento.IdPagamentoExterno,
            pagamento.DataPagamento,
            pagamento.DataCriacao
        );
    }
}
