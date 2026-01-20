using FluentAssertions;
using PagamentoService.Core.Entity;

namespace PagamentoService.Tests.Unit.EntityTests;

public class WebhookEventTests
{
    [Fact]
    public void Construtor_DeveCriarWebhookEventValido_QuandoDadosValidos()
    {
        // Arrange
        var tipoEvento = "payment.created";
        var payload = "{\"id\": \"123\"}";

        // Act
        var webhook = new WebhookEvent(tipoEvento, payload);

        // Assert
        webhook.Should().NotBeNull();
        webhook.Id.Should().NotBeEmpty();
        webhook.TipoEvento.Should().Be(tipoEvento);
        webhook.Payload.Should().Be(payload);
        webhook.Processado.Should().BeFalse();
        webhook.DataRecebimento.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        webhook.DataProcessamento.Should().BeNull();
        webhook.ErroProcessamento.Should().BeNull();
        webhook.PagamentoId.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Construtor_DeveLancarExcecao_QuandoTipoEventoInvalido(string? tipoEventoInvalido)
    {
        // Arrange
        var payload = "{\"id\": \"123\"}";

        // Act
        Action act = () => new WebhookEvent(tipoEventoInvalido!, payload);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Tipo de evento é obrigatório.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Construtor_DeveLancarExcecao_QuandoPayloadInvalido(string? payloadInvalido)
    {
        // Arrange
        var tipoEvento = "payment.created";

        // Act
        Action act = () => new WebhookEvent(tipoEvento, payloadInvalido!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Payload é obrigatório.");
    }

    [Fact]
    public void MarcarComoProcessado_DeveAtualizarStatus_QuandoChamadoSemPagamentoId()
    {
        // Arrange
        var webhook = new WebhookEvent("payment.created", "{\"id\": \"123\"}");

        // Act
        webhook.MarcarComoProcessado();

        // Assert
        webhook.Processado.Should().BeTrue();
        webhook.PagamentoId.Should().BeNull();
        webhook.DataProcessamento.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        webhook.ErroProcessamento.Should().BeNull();
    }

    [Fact]
    public void MarcarComoProcessado_DeveAtualizarStatusComPagamentoId_QuandoChamadoComPagamentoId()
    {
        // Arrange
        var webhook = new WebhookEvent("payment.created", "{\"id\": \"123\"}");
        var pagamentoId = Guid.NewGuid();

        // Act
        webhook.MarcarComoProcessado(pagamentoId);

        // Assert
        webhook.Processado.Should().BeTrue();
        webhook.PagamentoId.Should().Be(pagamentoId);
        webhook.DataProcessamento.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        webhook.ErroProcessamento.Should().BeNull();
    }

    [Fact]
    public void MarcarComoProcessado_DeveLimparErroAnterior_QuandoHaviaErro()
    {
        // Arrange
        var webhook = new WebhookEvent("payment.created", "{\"id\": \"123\"}");
        webhook.RegistrarErro("Erro anterior");

        // Act
        webhook.MarcarComoProcessado();

        // Assert
        webhook.Processado.Should().BeTrue();
        webhook.ErroProcessamento.Should().BeNull();
    }

    [Fact]
    public void RegistrarErro_DeveAtualizarStatusComErro_QuandoChamado()
    {
        // Arrange
        var webhook = new WebhookEvent("payment.created", "{\"id\": \"123\"}");
        var mensagemErro = "Falha ao processar pagamento";

        // Act
        webhook.RegistrarErro(mensagemErro);

        // Assert
        webhook.Processado.Should().BeFalse();
        webhook.ErroProcessamento.Should().Be(mensagemErro);
        webhook.DataProcessamento.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void RegistrarErro_DeveManterProcessadoFalso_AposMultiplasChaiadas()
    {
        // Arrange
        var webhook = new WebhookEvent("payment.created", "{\"id\": \"123\"}");

        // Act
        webhook.RegistrarErro("Erro 1");
        webhook.RegistrarErro("Erro 2");

        // Assert
        webhook.Processado.Should().BeFalse();
        webhook.ErroProcessamento.Should().Be("Erro 2");
    }

    [Fact]
    public void RegistrarErro_DeveReverterProcessado_QuandoJaEstavaMarcadoComoProcessado()
    {
        // Arrange
        var webhook = new WebhookEvent("payment.created", "{\"id\": \"123\"}");
        webhook.MarcarComoProcessado(Guid.NewGuid());

        // Act
        webhook.RegistrarErro("Erro ao reprocessar");

        // Assert
        webhook.Processado.Should().BeFalse();
        webhook.ErroProcessamento.Should().Be("Erro ao reprocessar");
    }
}
