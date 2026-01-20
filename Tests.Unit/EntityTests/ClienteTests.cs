using FluentAssertions;
using PagamentoService.Core.Entity;

namespace PagamentoService.Tests.Unit.EntityTests;

public class ClienteTests
{
    [Fact]
    public void Construtor_DeveCriarClienteValido_QuandoDadosValidos()
    {
        // Arrange
        var nome = "João Silva";
        var email = "joao@example.com";
        var cpf = "12345678901";

        // Act
        var cliente = new Cliente(nome, email, cpf);

        // Assert
        cliente.Should().NotBeNull();
        cliente.Id.Should().NotBeEmpty();
        cliente.Nome.Should().Be(nome);
        cliente.Email.Should().Be(email);
        cliente.CPF.Should().Be(cpf);
        cliente.DataCadastro.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Construtor_DeveCriarClienteSemEmail_QuandoEmailNulo()
    {
        // Arrange
        var nome = "João Silva";
        var cpf = "12345678901";

        // Act
        var cliente = new Cliente(nome, null, cpf);

        // Assert
        cliente.Email.Should().BeNull();
    }

    [Fact]
    public void Construtor_DeveLimparCPF_QuandoCPFComMascara()
    {
        // Arrange
        var nome = "João Silva";
        var email = "joao@example.com";
        var cpfComMascara = "123.456.789-01";

        // Act
        var cliente = new Cliente(nome, email, cpfComMascara);

        // Assert
        cliente.CPF.Should().Be("12345678901");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_DeveLancarExcecao_QuandoNomeVazio(string nomeInvalido)
    {
        // Arrange
        var email = "joao@example.com";
        var cpf = "12345678901";

        // Act
        Action act = () => new Cliente(nomeInvalido, email, cpf);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Nome do cliente é obrigatório.");
    }

    [Fact]
    public void Construtor_DeveLancarExcecao_QuandoNomeMaiorQue200Caracteres()
    {
        // Arrange
        var nomeGrande = new string('a', 201);
        var email = "joao@example.com";
        var cpf = "12345678901";

        // Act
        Action act = () => new Cliente(nomeGrande, email, cpf);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Nome não pode exceder 200 caracteres.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_DeveLancarExcecao_QuandoCPFVazio(string cpfInvalido)
    {
        // Arrange
        var nome = "João Silva";
        var email = "joao@example.com";

        // Act
        Action act = () => new Cliente(nome, email, cpfInvalido);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("CPF é obrigatório.");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("123456789012")] // 12 dígitos
    public void Construtor_DeveLancarExcecao_QuandoCPFNaoTem11Digitos(string cpfInvalido)
    {
        // Arrange
        var nome = "João Silva";
        var email = "joao@example.com";

        // Act
        Action act = () => new Cliente(nome, email, cpfInvalido);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("CPF deve conter 11 dígitos.");
    }

    [Theory]
    [InlineData("00000000000")]
    [InlineData("11111111111")]
    [InlineData("22222222222")]
    [InlineData("99999999999")]
    public void Construtor_DeveLancarExcecao_QuandoCPFTodosDigitosIguais(string cpfInvalido)
    {
        // Arrange
        var nome = "João Silva";
        var email = "joao@example.com";

        // Act
        Action act = () => new Cliente(nome, email, cpfInvalido);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("CPF inválido.");
    }

    [Fact]
    public void Construtor_DeveLancarExcecao_QuandoEmailInvalido()
    {
        // Arrange
        var nome = "João Silva";
        var emailInvalido = "emailinvalido";
        var cpf = "12345678901";

        // Act
        Action act = () => new Cliente(nome, emailInvalido, cpf);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Formato de email inválido.");
    }

    [Fact]
    public void Construtor_DeveLancarExcecao_QuandoEmailMaiorQue200Caracteres()
    {
        // Arrange
        var nome = "João Silva";
        var emailGrande = new string('a', 190) + "@example.com";
        var cpf = "12345678901";

        // Act
        Action act = () => new Cliente(nome, emailGrande, cpf);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Email não pode exceder 200 caracteres.");
    }

    [Fact]
    public void AtualizarDados_DeveAtualizarNomeEEmail_QuandoDadosValidos()
    {
        // Arrange
        var cliente = new Cliente("João Silva", "joao@example.com", "12345678901");
        var novoNome = "João da Silva Santos";
        var novoEmail = "joao.santos@example.com";

        // Act
        cliente.AtualizarDados(novoNome, novoEmail);

        // Assert
        cliente.Nome.Should().Be(novoNome);
        cliente.Email.Should().Be(novoEmail);
    }

    [Fact]
    public void AtualizarDados_DevePermitirEmailNulo()
    {
        // Arrange
        var cliente = new Cliente("João Silva", "joao@example.com", "12345678901");
        var novoNome = "João da Silva Santos";

        // Act
        cliente.AtualizarDados(novoNome, null);

        // Assert
        cliente.Nome.Should().Be(novoNome);
        cliente.Email.Should().BeNull();
    }

    [Fact]
    public void AtualizarDados_DeveApagarEspacos_QuandoNomeComEspacos()
    {
        // Arrange
        var cliente = new Cliente("João Silva", "joao@example.com", "12345678901");
        var novoNome = "  João da Silva Santos  ";
        var novoEmail = "joao.santos@example.com";

        // Act
        cliente.AtualizarDados(novoNome, novoEmail);

        // Assert
        cliente.Nome.Should().Be(novoNome.Trim());
        cliente.Email.Should().Be(novoEmail);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AtualizarDados_DeveLancarExcecao_QuandoNomeVazio(string nomeInvalido)
    {
        // Arrange
        var cliente = new Cliente("João Silva", "joao@example.com", "12345678901");

        // Act
        Action act = () => cliente.AtualizarDados(nomeInvalido, "email@example.com");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Nome do cliente é obrigatório.");
    }

    [Fact]
    public void AtualizarDados_DeveLancarExcecao_QuandoEmailInvalido()
    {
        // Arrange
        var cliente = new Cliente("João Silva", "joao@example.com", "12345678901");

        // Act
        Action act = () => cliente.AtualizarDados("João Santos", "emailinvalido");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Formato de email inválido.");
    }
}
