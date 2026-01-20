using Compartilhado.Enums;
using Compartilhado.Events;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PagamentoService.Api.Controllers;
using PagamentoService.Api.DTOs;
using PagamentoService.Core.Entity;
using PagamentoService.Core.Interfaces;
using PagamentoService.Core.UseCases;

namespace PagamentoService.Tests.Unit.ControllerTests;

public class PagamentosControllerTests
{
    private readonly Mock<IPagamentoRepository> _pagamentoRepositoryMock;
    private readonly Mock<IMercadoPagoService> _mercadoPagoServiceMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<ILogger<PagamentosController>> _loggerMock;
    private readonly PagamentoUseCases _pagamentoUseCases;
    private readonly PagamentosController _controller;

    public PagamentosControllerTests()
    {
        _pagamentoRepositoryMock = new Mock<IPagamentoRepository>();
        _mercadoPagoServiceMock = new Mock<IMercadoPagoService>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _loggerMock = new Mock<ILogger<PagamentosController>>();
        _pagamentoUseCases = new PagamentoUseCases(
            _pagamentoRepositoryMock.Object,
            _mercadoPagoServiceMock.Object);
        _controller = new PagamentosController(
            _pagamentoUseCases,
            _publishEndpointMock.Object,
            _loggerMock.Object);
    }

    #region GerarQrCode

    [Fact]
    public async Task GerarQrCode_DeveRetornarCreated_QuandoDadosValidos()
    {
        // Arrange
        var request = new GerarQrCodeRequest(Guid.NewGuid(), Guid.NewGuid(), 100.50m);
        var qrCodeResponse = new QrCodeResponse("qrcode123", "ext123", "pending");

        _pagamentoRepositoryMock
            .Setup(x => x.ObterPorPedidoIdAsync(request.PedidoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pagamento?)null);
        _pagamentoRepositoryMock
            .Setup(x => x.CriarAsync(It.IsAny<Pagamento>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pagamento p, CancellationToken _) => p);
        _pagamentoRepositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Pagamento>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pagamento p, CancellationToken _) => p);
        _mercadoPagoServiceMock
            .Setup(x => x.GerarQrCodeAsync(
                It.IsAny<decimal>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(qrCodeResponse);

        // Act
        var result = await _controller.GerarQrCode(request, CancellationToken.None);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        var response = createdResult.Value.Should().BeOfType<PagamentoResponse>().Subject;
        response.PedidoId.Should().Be(request.PedidoId);
        response.ClienteId.Should().Be(request.ClienteId);
        response.Valor.Should().Be(request.Valor);
    }

    [Fact]
    public async Task GerarQrCode_DeveRetornarConflict_QuandoPagamentoJaExiste()
    {
        // Arrange
        var request = new GerarQrCodeRequest(Guid.NewGuid(), Guid.NewGuid(), 100.50m);
        var pagamentoExistente = new Pagamento(request.PedidoId, request.ClienteId, request.Valor);

        _pagamentoRepositoryMock
            .Setup(x => x.ObterPorPedidoIdAsync(request.PedidoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagamentoExistente);

        // Act
        var result = await _controller.GerarQrCode(request, CancellationToken.None);

        // Assert
        var conflictResult = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflictResult.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task GerarQrCode_DeveRetornarInternalServerError_QuandoErroInesperado()
    {
        // Arrange
        var request = new GerarQrCodeRequest(Guid.NewGuid(), Guid.NewGuid(), 100.50m);

        _pagamentoRepositoryMock
            .Setup(x => x.ObterPorPedidoIdAsync(request.PedidoId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro de banco"));

        // Act
        var result = await _controller.GerarQrCode(request, CancellationToken.None);

        // Assert
        var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region ObterPorId

    [Fact]
    public async Task ObterPorId_DeveRetornarOk_QuandoPagamentoEncontrado()
    {
        // Arrange
        var pagamento = new Pagamento(Guid.NewGuid(), Guid.NewGuid(), 100.50m);
        _pagamentoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(pagamento.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagamento);

        // Act
        var result = await _controller.ObterPorId(pagamento.Id, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var response = okResult.Value.Should().BeOfType<PagamentoResponse>().Subject;
        response.Id.Should().Be(pagamento.Id);
    }

    [Fact]
    public async Task ObterPorId_DeveRetornarNotFound_QuandoPagamentoNaoEncontrado()
    {
        // Arrange
        var pagamentoId = Guid.NewGuid();
        _pagamentoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(pagamentoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pagamento?)null);

        // Act
        var result = await _controller.ObterPorId(pagamentoId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    #endregion

    #region ObterPorPedidoId

    [Fact]
    public async Task ObterPorPedidoId_DeveRetornarOk_QuandoPagamentoEncontrado()
    {
        // Arrange
        var pedidoId = Guid.NewGuid();
        var pagamento = new Pagamento(pedidoId, Guid.NewGuid(), 100.50m);
        _pagamentoRepositoryMock
            .Setup(x => x.ObterPorPedidoIdAsync(pedidoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagamento);

        // Act
        var result = await _controller.ObterPorPedidoId(pedidoId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var response = okResult.Value.Should().BeOfType<PagamentoResponse>().Subject;
        response.PedidoId.Should().Be(pedidoId);
    }

    [Fact]
    public async Task ObterPorPedidoId_DeveRetornarNotFound_QuandoPagamentoNaoEncontrado()
    {
        // Arrange
        var pedidoId = Guid.NewGuid();
        _pagamentoRepositoryMock
            .Setup(x => x.ObterPorPedidoIdAsync(pedidoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pagamento?)null);

        // Act
        var result = await _controller.ObterPorPedidoId(pedidoId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    #endregion

    #region ObterPorClienteId

    [Fact]
    public async Task ObterPorClienteId_DeveRetornarOk_QuandoPagamentosEncontrados()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var pagamentos = new List<Pagamento>
        {
            new Pagamento(Guid.NewGuid(), clienteId, 100.50m),
            new Pagamento(Guid.NewGuid(), clienteId, 200.00m)
        };
        _pagamentoRepositoryMock
            .Setup(x => x.ObterPorClienteIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagamentos);

        // Act
        var result = await _controller.ObterPorClienteId(clienteId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task ObterPorClienteId_DeveRetornarOkComListaVazia_QuandoNaoHaPagamentos()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        _pagamentoRepositoryMock
            .Setup(x => x.ObterPorClienteIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Pagamento>());

        // Act
        var result = await _controller.ObterPorClienteId(clienteId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    #endregion

    #region ConsultarStatus

    [Fact]
    public async Task ConsultarStatus_DeveRetornarOk_QuandoPagamentoEncontrado()
    {
        // Arrange
        var pagamento = new Pagamento(Guid.NewGuid(), Guid.NewGuid(), 100.50m);
        pagamento.DefinirQrCode("qrcode123", "ext123");
        var statusResponse = new StatusPagamentoResponse("ext123", "approved", DateTime.UtcNow);

        _pagamentoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(pagamento.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagamento);
        _mercadoPagoServiceMock
            .Setup(x => x.ConsultarPagamentoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(statusResponse);

        // Act
        var result = await _controller.ConsultarStatus(pagamento.Id, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task ConsultarStatus_DeveRetornarNotFound_QuandoPagamentoNaoEncontrado()
    {
        // Arrange
        var pagamentoId = Guid.NewGuid();
        _pagamentoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(pagamentoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pagamento?)null);

        // Act
        var result = await _controller.ConsultarStatus(pagamentoId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task ConsultarStatus_DeveRetornarNotFound_QuandoPagamentoSemIdExterno()
    {
        // Arrange
        var pagamento = new Pagamento(Guid.NewGuid(), Guid.NewGuid(), 100.50m);
        _pagamentoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(pagamento.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagamento);

        // Act
        var result = await _controller.ConsultarStatus(pagamento.Id, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task ConsultarStatus_DeveRetornarInternalServerError_QuandoErroInesperado()
    {
        // Arrange
        var pagamentoId = Guid.NewGuid();
        _pagamentoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(pagamentoId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro de banco"));

        // Act
        var result = await _controller.ConsultarStatus(pagamentoId, CancellationToken.None);

        // Assert
        var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region ConfirmarPagamento

    [Fact]
    public async Task ConfirmarPagamento_DeveRetornarOk_QuandoConfirmadoComSucesso()
    {
        // Arrange
        var idPagamentoExterno = "ext123";
        var pagamento = new Pagamento(Guid.NewGuid(), Guid.NewGuid(), 100.50m);
        pagamento.DefinirQrCode("qrcode123", idPagamentoExterno);

        _pagamentoRepositoryMock
            .Setup(x => x.ObterPorIdPagamentoExternoAsync(idPagamentoExterno, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagamento);
        _pagamentoRepositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Pagamento>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pagamento p, CancellationToken _) => p);
        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<PagamentoConfirmadoEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ConfirmarPagamento(idPagamentoExterno, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var response = okResult.Value.Should().BeOfType<PagamentoResponse>().Subject;
        response.Status.Should().Be(EStatusPagamento.Aprovado);

        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<PagamentoConfirmadoEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ConfirmarPagamento_DeveRetornarNotFound_QuandoPagamentoNaoEncontrado()
    {
        // Arrange
        var idPagamentoExterno = "ext123";
        _pagamentoRepositoryMock
            .Setup(x => x.ObterPorIdPagamentoExternoAsync(idPagamentoExterno, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pagamento?)null);

        // Act
        var result = await _controller.ConfirmarPagamento(idPagamentoExterno, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task ConfirmarPagamento_DeveRetornarInternalServerError_QuandoErroInesperado()
    {
        // Arrange
        var idPagamentoExterno = "ext123";
        _pagamentoRepositoryMock
            .Setup(x => x.ObterPorIdPagamentoExternoAsync(idPagamentoExterno, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro de banco"));

        // Act
        var result = await _controller.ConfirmarPagamento(idPagamentoExterno, CancellationToken.None);

        // Assert
        var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);
    }

    #endregion
}
