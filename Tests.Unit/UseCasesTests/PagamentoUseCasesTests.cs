using Compartilhado.Enums;
using FluentAssertions;
using Moq;
using PagamentoService.Core.Entity;
using PagamentoService.Core.Interfaces;
using PagamentoService.Core.UseCases;

namespace PagamentoService.Tests.Unit.UseCasesTests;

public class PagamentoUseCasesTests
{
    private readonly Mock<IPagamentoRepository> _repositoryMock;
    private readonly Mock<IMercadoPagoService> _mercadoPagoServiceMock;
    private readonly PagamentoUseCases _pagamentoUseCases;

    public PagamentoUseCasesTests()
    {
        _repositoryMock = new Mock<IPagamentoRepository>();
        _mercadoPagoServiceMock = new Mock<IMercadoPagoService>();
        _pagamentoUseCases = new PagamentoUseCases(_repositoryMock.Object, _mercadoPagoServiceMock.Object);
    }

    [Fact]
    public async Task CriarPagamentoAsync_DeveCriarPagamentoComQrCode_QuandoDadosValidos()
    {
        // Arrange
        var pedidoId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var valor = 100.50m;
        var qrCodeResponse = new QrCodeResponse("QR_CODE_123", "MP_EXTERNAL_123", "pending");

        _repositoryMock
            .Setup(r => r.ObterPorPedidoIdAsync(pedidoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pagamento?)null);

        _repositoryMock
            .Setup(r => r.CriarAsync(It.IsAny<Pagamento>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pagamento p, CancellationToken ct) => p);

        _mercadoPagoServiceMock
            .Setup(m => m.GerarQrCodeAsync(valor, It.IsAny<string>(), pedidoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(qrCodeResponse);

        _repositoryMock
            .Setup(r => r.AtualizarAsync(It.IsAny<Pagamento>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pagamento p, CancellationToken ct) => p);

        // Act
        var resultado = await _pagamentoUseCases.CriarPagamentoAsync(pedidoId, clienteId, valor);

        // Assert
        resultado.Should().NotBeNull();
        resultado.PedidoId.Should().Be(pedidoId);
        resultado.ClienteId.Should().Be(clienteId);
        resultado.Valor.Should().Be(valor);
        resultado.QrCode.Should().Be(qrCodeResponse.QrCode);
        resultado.IdPagamentoExterno.Should().Be(qrCodeResponse.IdPagamentoExterno);

        _repositoryMock.Verify(r => r.ObterPorPedidoIdAsync(pedidoId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.CriarAsync(It.IsAny<Pagamento>(), It.IsAny<CancellationToken>()), Times.Once);
        _mercadoPagoServiceMock.Verify(m => m.GerarQrCodeAsync(valor, It.IsAny<string>(), pedidoId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<Pagamento>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CriarPagamentoAsync_DeveCriarPagamentoSemQrCode_QuandoGerarQrCodeFalse()
    {
        // Arrange
        var pedidoId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var valor = 100.50m;

        _repositoryMock
            .Setup(r => r.ObterPorPedidoIdAsync(pedidoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pagamento?)null);

        _repositoryMock
            .Setup(r => r.CriarAsync(It.IsAny<Pagamento>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pagamento p, CancellationToken ct) => p);

        // Act
        var resultado = await _pagamentoUseCases.CriarPagamentoAsync(pedidoId, clienteId, valor, gerarQrCode: false);

        // Assert
        resultado.Should().NotBeNull();
        resultado.PedidoId.Should().Be(pedidoId);
        resultado.ClienteId.Should().Be(clienteId);
        resultado.Valor.Should().Be(valor);
        resultado.QrCode.Should().BeNull();

        _mercadoPagoServiceMock.Verify(m => m.GerarQrCodeAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CriarPagamentoAsync_DeveLancarExcecao_QuandoPagamentoDuplicado()
    {
        // Arrange
        var pedidoId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var valor = 100.50m;
        var pagamentoExistente = new Pagamento(pedidoId, clienteId, valor);

        _repositoryMock
            .Setup(r => r.ObterPorPedidoIdAsync(pedidoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagamentoExistente);

        // Act
        Func<Task> act = async () => await _pagamentoUseCases.CriarPagamentoAsync(pedidoId, clienteId, valor);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Já existe um pagamento para este pedido.");

        _repositoryMock.Verify(r => r.CriarAsync(It.IsAny<Pagamento>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ObterPagamentoPorIdAsync_DeveRetornarPagamento_QuandoEncontrado()
    {
        // Arrange
        var id = Guid.NewGuid();
        var pagamento = new Pagamento(Guid.NewGuid(), Guid.NewGuid(), 100m);

        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagamento);

        // Act
        var resultado = await _pagamentoUseCases.ObterPagamentoPorIdAsync(id);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().Be(pagamento);

        _repositoryMock.Verify(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObterPagamentoPorIdAsync_DeveRetornarNull_QuandoNaoEncontrado()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pagamento?)null);

        // Act
        var resultado = await _pagamentoUseCases.ObterPagamentoPorIdAsync(id);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObterPagamentoPorPedidoIdAsync_DeveRetornarPagamento_QuandoEncontrado()
    {
        // Arrange
        var pedidoId = Guid.NewGuid();
        var pagamento = new Pagamento(pedidoId, Guid.NewGuid(), 100m);

        _repositoryMock
            .Setup(r => r.ObterPorPedidoIdAsync(pedidoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagamento);

        // Act
        var resultado = await _pagamentoUseCases.ObterPagamentoPorPedidoIdAsync(pedidoId);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().Be(pagamento);

        _repositoryMock.Verify(r => r.ObterPorPedidoIdAsync(pedidoId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObterPagamentoPorPedidoIdAsync_DeveRetornarNull_QuandoNaoEncontrado()
    {
        // Arrange
        var pedidoId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.ObterPorPedidoIdAsync(pedidoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pagamento?)null);

        // Act
        var resultado = await _pagamentoUseCases.ObterPagamentoPorPedidoIdAsync(pedidoId);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ConfirmarPagamentoAsync_DeveConfirmarPagamento_QuandoPagamentoEncontrado()
    {
        // Arrange
        var idExterno = "MP_EXTERNAL_123";
        var dataPagamento = DateTime.UtcNow;
        var pagamento = new Pagamento(Guid.NewGuid(), Guid.NewGuid(), 100m);

        _repositoryMock
            .Setup(r => r.ObterPorIdPagamentoExternoAsync(idExterno, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagamento);

        _repositoryMock
            .Setup(r => r.AtualizarAsync(It.IsAny<Pagamento>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pagamento p, CancellationToken ct) => p);

        // Act
        var resultado = await _pagamentoUseCases.ConfirmarPagamentoAsync(idExterno, dataPagamento);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Status.Should().Be(EStatusPagamento.Aprovado);
        resultado.DataPagamento.Should().Be(dataPagamento);

        _repositoryMock.Verify(r => r.ObterPorIdPagamentoExternoAsync(idExterno, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<Pagamento>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConfirmarPagamentoAsync_DeveLancarExcecao_QuandoPagamentoNaoEncontrado()
    {
        // Arrange
        var idExterno = "MP_EXTERNAL_123";
        var dataPagamento = DateTime.UtcNow;

        _repositoryMock
            .Setup(r => r.ObterPorIdPagamentoExternoAsync(idExterno, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pagamento?)null);

        // Act
        Func<Task> act = async () => await _pagamentoUseCases.ConfirmarPagamentoAsync(idExterno, dataPagamento);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Pagamento não encontrado.");

        _repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<Pagamento>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RejeitarPagamentoAsync_DeveRejeitarPagamento_QuandoPagamentoEncontrado()
    {
        // Arrange
        var idExterno = "MP_EXTERNAL_123";
        var pagamento = new Pagamento(Guid.NewGuid(), Guid.NewGuid(), 100m);

        _repositoryMock
            .Setup(r => r.ObterPorIdPagamentoExternoAsync(idExterno, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagamento);

        _repositoryMock
            .Setup(r => r.AtualizarAsync(It.IsAny<Pagamento>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pagamento p, CancellationToken ct) => p);

        // Act
        var resultado = await _pagamentoUseCases.RejeitarPagamentoAsync(idExterno);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Status.Should().Be(EStatusPagamento.Rejeitado);

        _repositoryMock.Verify(r => r.ObterPorIdPagamentoExternoAsync(idExterno, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<Pagamento>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RejeitarPagamentoAsync_DeveLancarExcecao_QuandoPagamentoNaoEncontrado()
    {
        // Arrange
        var idExterno = "MP_EXTERNAL_123";

        _repositoryMock
            .Setup(r => r.ObterPorIdPagamentoExternoAsync(idExterno, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pagamento?)null);

        // Act
        Func<Task> act = async () => await _pagamentoUseCases.RejeitarPagamentoAsync(idExterno);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Pagamento não encontrado.");

        _repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<Pagamento>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConsultarStatusPagamentoAsync_DeveRetornarStatus_QuandoPagamentoComIdExterno()
    {
        // Arrange
        var id = Guid.NewGuid();
        var idExterno = "MP_EXTERNAL_123";
        var pagamento = new Pagamento(Guid.NewGuid(), Guid.NewGuid(), 100m);
        pagamento.DefinirQrCode("QR_CODE", idExterno);

        var statusResponse = new StatusPagamentoResponse(idExterno, "approved", DateTime.UtcNow);

        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagamento);

        _mercadoPagoServiceMock
            .Setup(m => m.ConsultarPagamentoAsync(idExterno, It.IsAny<CancellationToken>()))
            .ReturnsAsync(statusResponse);

        // Act
        var resultado = await _pagamentoUseCases.ConsultarStatusPagamentoAsync(id);

        // Assert
        resultado.Should().NotBeNull();
        resultado.IdPagamento.Should().Be(idExterno);
        resultado.Status.Should().Be("approved");

        _repositoryMock.Verify(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        _mercadoPagoServiceMock.Verify(m => m.ConsultarPagamentoAsync(idExterno, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConsultarStatusPagamentoAsync_DeveLancarExcecao_QuandoPagamentoNaoEncontrado()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pagamento?)null);

        // Act
        Func<Task> act = async () => await _pagamentoUseCases.ConsultarStatusPagamentoAsync(id);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Pagamento não encontrado.");
    }

    [Fact]
    public async Task ConsultarStatusPagamentoAsync_DeveLancarExcecao_QuandoPagamentoSemIdExterno()
    {
        // Arrange
        var id = Guid.NewGuid();
        var pagamento = new Pagamento(Guid.NewGuid(), Guid.NewGuid(), 100m);

        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagamento);

        // Act
        Func<Task> act = async () => await _pagamentoUseCases.ConsultarStatusPagamentoAsync(id);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Pagamento não possui ID externo.");

        _mercadoPagoServiceMock.Verify(m => m.ConsultarPagamentoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
