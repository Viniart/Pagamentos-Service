using Compartilhado.Enums;
using FluentAssertions;
using PagamentoService.Core.Entity;

namespace PagamentoService.Tests.Unit.EntityTests;

public class PagamentoTests
{
    [Fact]
    public void Construtor_DeveCrearPagamentoValido_QuandoDadosValidos()
    {
        // Arrange
        var pedidoId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var valor = 100.50m;

        // Act
        var pagamento = new Pagamento(pedidoId, clienteId, valor);

        // Assert
        pagamento.Should().NotBeNull();
        pagamento.Id.Should().NotBeEmpty();
        pagamento.PedidoId.Should().Be(pedidoId);
        pagamento.ClienteId.Should().Be(clienteId);
        pagamento.Valor.Should().Be(valor);
        pagamento.Status.Should().Be(EStatusPagamento.Pendente);
        pagamento.QrCode.Should().BeNull();
        pagamento.IdPagamentoExterno.Should().BeNull();
        pagamento.DataPagamento.Should().BeNull();
        pagamento.DataCriacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        pagamento.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void Construtor_DeveLancarExcecao_QuandoValorInvalido(decimal valorInvalido)
    {
        // Arrange
        var pedidoId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();

        // Act
        Action act = () => new Pagamento(pedidoId, clienteId, valorInvalido);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Valor do pagamento deve ser maior que zero.");
    }

    [Fact]
    public void DefinirQrCode_DeveDefinirQrCodeEIdExterno_QuandoDadosValidos()
    {
        // Arrange
        var pagamento = new Pagamento(Guid.NewGuid(), Guid.NewGuid(), 100m);
        var qrCode = "00020126580014br.gov.bcb.pix";
        var idExterno = "MP-123456";

        // Act
        pagamento.DefinirQrCode(qrCode, idExterno);

        // Assert
        pagamento.QrCode.Should().Be(qrCode);
        pagamento.IdPagamentoExterno.Should().Be(idExterno);
        pagamento.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void DefinirQrCode_DeveDefinirQrCodeSemIdExterno_QuandoIdExternoNaoInformado()
    {
        // Arrange
        var pagamento = new Pagamento(Guid.NewGuid(), Guid.NewGuid(), 100m);
        var qrCode = "00020126580014br.gov.bcb.pix";

        // Act
        pagamento.DefinirQrCode(qrCode);

        // Assert
        pagamento.QrCode.Should().Be(qrCode);
        pagamento.IdPagamentoExterno.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void DefinirQrCode_DeveLancarExcecao_QuandoQrCodeInvalido(string qrCodeInvalido)
    {
        // Arrange
        var pagamento = new Pagamento(Guid.NewGuid(), Guid.NewGuid(), 100m);

        // Act
        Action act = () => pagamento.DefinirQrCode(qrCodeInvalido);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("QR Code não pode ser vazio.");
    }

    [Fact]
    public void ConfirmarPagamento_DeveConfirmarPagamento_QuandoStatusPendente()
    {
        // Arrange
        var pagamento = new Pagamento(Guid.NewGuid(), Guid.NewGuid(), 100m);
        var dataPagamento = DateTime.UtcNow;

        // Act
        pagamento.ConfirmarPagamento(dataPagamento);

        // Assert
        pagamento.Status.Should().Be(EStatusPagamento.Aprovado);
        pagamento.DataPagamento.Should().Be(dataPagamento);
        pagamento.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ConfirmarPagamento_DeveLancarExcecao_QuandoPagamentoJaConfirmado()
    {
        // Arrange
        var pagamento = new Pagamento(Guid.NewGuid(), Guid.NewGuid(), 100m);
        pagamento.ConfirmarPagamento(DateTime.UtcNow);

        // Act
        Action act = () => pagamento.ConfirmarPagamento(DateTime.UtcNow);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Pagamento já foi confirmado.");
    }

    [Fact]
    public void RejeitarPagamento_DeveRejeitarPagamento_QuandoStatusPendente()
    {
        // Arrange
        var pagamento = new Pagamento(Guid.NewGuid(), Guid.NewGuid(), 100m);

        // Act
        pagamento.RejeitarPagamento();

        // Assert
        pagamento.Status.Should().Be(EStatusPagamento.Rejeitado);
        pagamento.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void RejeitarPagamento_DeveLancarExcecao_QuandoPagamentoJaAprovado()
    {
        // Arrange
        var pagamento = new Pagamento(Guid.NewGuid(), Guid.NewGuid(), 100m);
        pagamento.ConfirmarPagamento(DateTime.UtcNow);

        // Act
        Action act = () => pagamento.RejeitarPagamento();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Não é possível rejeitar um pagamento já aprovado.");
    }

    [Fact]
    public void CancelarPagamento_DeveCancelarPagamento_QuandoStatusPendente()
    {
        // Arrange
        var pagamento = new Pagamento(Guid.NewGuid(), Guid.NewGuid(), 100m);

        // Act
        pagamento.CancelarPagamento();

        // Assert
        pagamento.Status.Should().Be(EStatusPagamento.Cancelado);
        pagamento.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void CancelarPagamento_DeveLancarExcecao_QuandoPagamentoJaAprovado()
    {
        // Arrange
        var pagamento = new Pagamento(Guid.NewGuid(), Guid.NewGuid(), 100m);
        pagamento.ConfirmarPagamento(DateTime.UtcNow);

        // Act
        Action act = () => pagamento.CancelarPagamento();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Não é possível cancelar um pagamento já aprovado.");
    }

    [Theory]
    [InlineData(EStatusPagamento.Pendente)]
    [InlineData(EStatusPagamento.Aprovado)]
    [InlineData(EStatusPagamento.Rejeitado)]
    [InlineData(EStatusPagamento.Cancelado)]
    public void AtualizarStatus_DeveAtualizarStatus_QuandoStatusValido(EStatusPagamento novoStatus)
    {
        // Arrange
        var pagamento = new Pagamento(Guid.NewGuid(), Guid.NewGuid(), 100m);

        // Act
        pagamento.AtualizarStatus(novoStatus);

        // Assert
        pagamento.Status.Should().Be(novoStatus);
        pagamento.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void AtualizarStatus_DeveDefinirDataPagamento_QuandoStatusAprovadoEDataNula()
    {
        // Arrange
        var pagamento = new Pagamento(Guid.NewGuid(), Guid.NewGuid(), 100m);

        // Act
        pagamento.AtualizarStatus(EStatusPagamento.Aprovado);

        // Assert
        pagamento.Status.Should().Be(EStatusPagamento.Aprovado);
        pagamento.DataPagamento.Should().NotBeNull();
        pagamento.DataPagamento.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void AtualizarStatus_DeveLancarExcecao_QuandoStatusInvalido()
    {
        // Arrange
        var pagamento = new Pagamento(Guid.NewGuid(), Guid.NewGuid(), 100m);
        var statusInvalido = (EStatusPagamento)999;

        // Act
        Action act = () => pagamento.AtualizarStatus(statusInvalido);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Status inválido.");
    }
}
