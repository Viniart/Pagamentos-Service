using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PagamentoService.Api.Controllers;
using PagamentoService.Api.DTOs;
using PagamentoService.Core.Entity;
using PagamentoService.Core.Interfaces;
using PagamentoService.Core.UseCases;

namespace PagamentoService.Tests.Unit.ControllerTests;

public class ClientesControllerTests
{
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly Mock<ILogger<ClientesController>> _loggerMock;
    private readonly ClienteUseCases _clienteUseCases;
    private readonly ClientesController _controller;

    public ClientesControllerTests()
    {
        _clienteRepositoryMock = new Mock<IClienteRepository>();
        _loggerMock = new Mock<ILogger<ClientesController>>();
        _clienteUseCases = new ClienteUseCases(_clienteRepositoryMock.Object);
        _controller = new ClientesController(_clienteUseCases, _loggerMock.Object);
    }

    #region CriarCliente

    [Fact]
    public async Task CriarCliente_DeveRetornarCreated_QuandoDadosValidos()
    {
        // Arrange
        var request = new CriarClienteRequest("João Silva", "joao@email.com", "12345678901");
        _clienteRepositoryMock
            .Setup(x => x.ObterPorCPFAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);
        _clienteRepositoryMock
            .Setup(x => x.CriarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente c, CancellationToken _) => c);

        // Act
        var result = await _controller.CriarCliente(request, CancellationToken.None);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        var response = createdResult.Value.Should().BeOfType<ClienteResponse>().Subject;
        response.Nome.Should().Be(request.Nome);
        response.Email.Should().Be(request.Email);
        response.CPF.Should().Be(request.CPF);
    }

    [Fact]
    public async Task CriarCliente_DeveRetornarConflict_QuandoCPFJaExiste()
    {
        // Arrange
        var request = new CriarClienteRequest("João Silva", "joao@email.com", "12345678901");
        var clienteExistente = new Cliente("Cliente Existente", "existente@email.com", "12345678901");
        _clienteRepositoryMock
            .Setup(x => x.ObterPorCPFAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteExistente);

        // Act
        var result = await _controller.CriarCliente(request, CancellationToken.None);

        // Assert
        var conflictResult = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflictResult.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task CriarCliente_DeveRetornarBadRequest_QuandoDadosInvalidos()
    {
        // Arrange
        var request = new CriarClienteRequest("", "joao@email.com", "12345678901");
        _clienteRepositoryMock
            .Setup(x => x.ObterPorCPFAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        // Act
        var result = await _controller.CriarCliente(request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task CriarCliente_DeveRetornarInternalServerError_QuandoErroInesperado()
    {
        // Arrange
        var request = new CriarClienteRequest("João Silva", "joao@email.com", "12345678901");
        _clienteRepositoryMock
            .Setup(x => x.ObterPorCPFAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro de banco"));

        // Act
        var result = await _controller.CriarCliente(request, CancellationToken.None);

        // Assert
        var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region ObterTodos

    [Fact]
    public async Task ObterTodos_DeveRetornarOkComLista_QuandoExistemClientes()
    {
        // Arrange
        var clientes = new List<Cliente>
        {
            new Cliente("João Silva", "joao@email.com", "12345678901"),
            new Cliente("Maria Santos", "maria@email.com", "98765432100")
        };
        _clienteRepositoryMock
            .Setup(x => x.ObterTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientes);

        // Act
        var result = await _controller.ObterTodos(CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task ObterTodos_DeveRetornarOkComListaVazia_QuandoNaoExistemClientes()
    {
        // Arrange
        _clienteRepositoryMock
            .Setup(x => x.ObterTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Cliente>());

        // Act
        var result = await _controller.ObterTodos(CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    #endregion

    #region ObterPorId

    [Fact]
    public async Task ObterPorId_DeveRetornarOk_QuandoClienteEncontrado()
    {
        // Arrange
        var cliente = new Cliente("João Silva", "joao@email.com", "12345678901");
        _clienteRepositoryMock
            .Setup(x => x.ObterPorIdAsync(cliente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        // Act
        var result = await _controller.ObterPorId(cliente.Id, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var response = okResult.Value.Should().BeOfType<ClienteResponse>().Subject;
        response.Id.Should().Be(cliente.Id);
        response.Nome.Should().Be(cliente.Nome);
    }

    [Fact]
    public async Task ObterPorId_DeveRetornarNotFound_QuandoClienteNaoEncontrado()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        _clienteRepositoryMock
            .Setup(x => x.ObterPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        // Act
        var result = await _controller.ObterPorId(clienteId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    #endregion

    #region ObterPorCPF

    [Fact]
    public async Task ObterPorCPF_DeveRetornarOk_QuandoClienteEncontrado()
    {
        // Arrange
        var cpf = "12345678901";
        var cliente = new Cliente("João Silva", "joao@email.com", cpf);
        _clienteRepositoryMock
            .Setup(x => x.ObterPorCPFAsync(cpf, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        // Act
        var result = await _controller.ObterPorCPF(cpf, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var response = okResult.Value.Should().BeOfType<ClienteResponse>().Subject;
        response.CPF.Should().Be(cpf);
    }

    [Fact]
    public async Task ObterPorCPF_DeveRetornarNotFound_QuandoClienteNaoEncontrado()
    {
        // Arrange
        var cpf = "12345678901";
        _clienteRepositoryMock
            .Setup(x => x.ObterPorCPFAsync(cpf, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        // Act
        var result = await _controller.ObterPorCPF(cpf, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    #endregion

    #region AtualizarCliente

    [Fact]
    public async Task AtualizarCliente_DeveRetornarOk_QuandoAtualizadoComSucesso()
    {
        // Arrange
        var cliente = new Cliente("João Silva", "joao@email.com", "12345678901");
        var request = new AtualizarClienteRequest("João Santos", "joao.santos@email.com");
        _clienteRepositoryMock
            .Setup(x => x.ObterPorIdAsync(cliente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _clienteRepositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente c, CancellationToken _) => c);

        // Act
        var result = await _controller.AtualizarCliente(cliente.Id, request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var response = okResult.Value.Should().BeOfType<ClienteResponse>().Subject;
        response.Nome.Should().Be(request.Nome);
        response.Email.Should().Be(request.Email);
    }

    [Fact]
    public async Task AtualizarCliente_DeveRetornarNotFound_QuandoClienteNaoEncontrado()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var request = new AtualizarClienteRequest("João Santos", "joao.santos@email.com");
        _clienteRepositoryMock
            .Setup(x => x.ObterPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        // Act
        var result = await _controller.AtualizarCliente(clienteId, request, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task AtualizarCliente_DeveRetornarBadRequest_QuandoDadosInvalidos()
    {
        // Arrange
        var cliente = new Cliente("João Silva", "joao@email.com", "12345678901");
        var request = new AtualizarClienteRequest("", "joao.santos@email.com");
        _clienteRepositoryMock
            .Setup(x => x.ObterPorIdAsync(cliente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        // Act
        var result = await _controller.AtualizarCliente(cliente.Id, request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task AtualizarCliente_DeveRetornarInternalServerError_QuandoErroInesperado()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var request = new AtualizarClienteRequest("João Santos", "joao.santos@email.com");
        _clienteRepositoryMock
            .Setup(x => x.ObterPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro de banco"));

        // Act
        var result = await _controller.AtualizarCliente(clienteId, request, CancellationToken.None);

        // Assert
        var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);
    }

    #endregion
}
