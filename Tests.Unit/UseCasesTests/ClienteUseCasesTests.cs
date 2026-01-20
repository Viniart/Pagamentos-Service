using FluentAssertions;
using Moq;
using PagamentoService.Core.Entity;
using PagamentoService.Core.Interfaces;
using PagamentoService.Core.UseCases;

namespace PagamentoService.Tests.Unit.UseCasesTests;

public class ClienteUseCasesTests
{
    private readonly Mock<IClienteRepository> _repositoryMock;
    private readonly ClienteUseCases _clienteUseCases;

    public ClienteUseCasesTests()
    {
        _repositoryMock = new Mock<IClienteRepository>();
        _clienteUseCases = new ClienteUseCases(_repositoryMock.Object);
    }

    [Fact]
    public async Task CriarClienteAsync_DeveCriarCliente_QuandoDadosValidos()
    {
        // Arrange
        var nome = "João Silva";
        var email = "joao@example.com";
        var cpf = "12345678901";

        _repositoryMock
            .Setup(r => r.ObterPorCPFAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        _repositoryMock
            .Setup(r => r.CriarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente c, CancellationToken ct) => c);

        // Act
        var resultado = await _clienteUseCases.CriarClienteAsync(nome, email, cpf);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Nome.Should().Be(nome);
        resultado.Email.Should().Be(email);
        resultado.CPF.Should().Be(cpf);

        _repositoryMock.Verify(r => r.ObterPorCPFAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.CriarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CriarClienteAsync_DeveLancarExcecao_QuandoCPFDuplicado()
    {
        // Arrange
        var nome = "João Silva";
        var email = "joao@example.com";
        var cpf = "12345678901";
        var clienteExistente = new Cliente("Maria Santos", "maria@example.com", cpf);

        _repositoryMock
            .Setup(r => r.ObterPorCPFAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteExistente);

        // Act
        Func<Task> act = async () => await _clienteUseCases.CriarClienteAsync(nome, email, cpf);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Já existe um cliente cadastrado com este CPF.");

        _repositoryMock.Verify(r => r.CriarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ObterClientePorIdAsync_DeveRetornarCliente_QuandoEncontrado()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cliente = new Cliente("João Silva", "joao@example.com", "12345678901");

        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        // Act
        var resultado = await _clienteUseCases.ObterClientePorIdAsync(id);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().Be(cliente);

        _repositoryMock.Verify(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObterClientePorIdAsync_DeveRetornarNull_QuandoNaoEncontrado()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        // Act
        var resultado = await _clienteUseCases.ObterClientePorIdAsync(id);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObterClientePorCPFAsync_DeveRetornarCliente_QuandoEncontrado()
    {
        // Arrange
        var cpf = "12345678901";
        var cliente = new Cliente("João Silva", "joao@example.com", cpf);

        _repositoryMock
            .Setup(r => r.ObterPorCPFAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        // Act
        var resultado = await _clienteUseCases.ObterClientePorCPFAsync(cpf);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().Be(cliente);

        _repositoryMock.Verify(r => r.ObterPorCPFAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObterClientePorCPFAsync_DeveRetornarNull_QuandoNaoEncontrado()
    {
        // Arrange
        var cpf = "12345678901";

        _repositoryMock
            .Setup(r => r.ObterPorCPFAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        // Act
        var resultado = await _clienteUseCases.ObterClientePorCPFAsync(cpf);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task AtualizarClienteAsync_DeveAtualizarCliente_QuandoClienteEncontrado()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cliente = new Cliente("João Silva", "joao@example.com", "12345678901");
        var novoNome = "João da Silva Santos";
        var novoEmail = "joao.santos@example.com";

        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _repositoryMock
            .Setup(r => r.AtualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente c, CancellationToken ct) => c);

        // Act
        var resultado = await _clienteUseCases.AtualizarClienteAsync(id, novoNome, novoEmail);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Nome.Should().Be(novoNome);
        resultado.Email.Should().Be(novoEmail);

        _repositoryMock.Verify(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AtualizarClienteAsync_DeveLancarExcecao_QuandoClienteNaoEncontrado()
    {
        // Arrange
        var id = Guid.NewGuid();
        var novoNome = "João da Silva Santos";
        var novoEmail = "joao.santos@example.com";

        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        // Act
        Func<Task> act = async () => await _clienteUseCases.AtualizarClienteAsync(id, novoNome, novoEmail);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cliente não encontrado.");

        _repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
