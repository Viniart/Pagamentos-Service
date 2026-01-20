using FluentAssertions;
using Moq;
using PagamentoService.Core.Entity;
using PagamentoService.Core.Interfaces;
using PagamentoService.Core.UseCases;
using Reqnroll;

namespace PagamentoService.Tests.BDD.StepDefinitions;

[Binding]
public class GerenciarClienteSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly Mock<IClienteRepository> _mockClienteRepository;
    private readonly ClienteUseCases _clienteUseCases;

    private Cliente? _cliente;
    private Guid _clienteId;
    private string _cpf = string.Empty;
    private Exception? _exception;

    public GerenciarClienteSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _mockClienteRepository = new Mock<IClienteRepository>();
        _clienteUseCases = new ClienteUseCases(_mockClienteRepository.Object);
    }

    [Given(@"que não existe um cliente com CPF ""(.*)""")]
    public void DadoQueNaoExisteUmClienteComCPF(string cpf)
    {
        _cpf = cpf;

        _mockClienteRepository
            .Setup(r => r.ObterPorCPFAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);
    }

    [Given(@"que já existe um cliente com CPF ""(.*)""")]
    public void DadoQueJaExisteUmClienteComCPF(string cpf)
    {
        _cpf = cpf;
        var clienteExistente = new Cliente("Cliente Existente", "existente@example.com", cpf);

        _mockClienteRepository
            .Setup(r => r.ObterPorCPFAsync(cpf, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteExistente);
    }

    [Given(@"que existe um cliente cadastrado com ID ""(.*)""")]
    public void DadoQueExisteUmClienteCadastradoComID(string clienteId)
    {
        _clienteId = Guid.Parse(clienteId);
        _cliente = new Cliente("João da Silva", "joao@example.com", "12345678901");

        _mockClienteRepository
            .Setup(r => r.ObterPorIdAsync(_clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_cliente);

        _mockClienteRepository
            .Setup(r => r.AtualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente c, CancellationToken ct) => c);
    }

    [Given(@"que não existe um cliente com ID ""(.*)""")]
    public void DadoQueNaoExisteUmClienteComID(string clienteId)
    {
        _clienteId = Guid.Parse(clienteId);

        _mockClienteRepository
            .Setup(r => r.ObterPorIdAsync(_clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);
    }

    [Given(@"que existe um cliente cadastrado com CPF ""(.*)""")]
    public void DadoQueExisteUmClienteCadastradoComCPF(string cpf)
    {
        _cpf = cpf;
        _cliente = new Cliente("Cliente Teste", "teste@example.com", cpf);

        _mockClienteRepository
            .Setup(r => r.ObterPorCPFAsync(cpf, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_cliente);
    }

    [When(@"eu cadastrar um cliente com os seguintes dados:")]
    public async Task QuandoEuCadastrarUmClienteComOsSeguintesDados(Table table)
    {
        try
        {
            var row = table.Rows[0];
            var nome = row["Nome"];
            var email = row["Email"];
            var cpf = row["CPF"];

            _mockClienteRepository
                .Setup(r => r.CriarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Cliente c, CancellationToken ct) => c);

            _cliente = await _clienteUseCases.CriarClienteAsync(nome, email, cpf);
        }
        catch (Exception ex)
        {
            _exception = ex;
            _scenarioContext["Exception"] = ex;
        }
    }

    [When(@"eu cadastrar um cliente sem email:")]
    public async Task QuandoEuCadastrarUmClienteSemEmail(Table table)
    {
        try
        {
            var row = table.Rows[0];
            var nome = row["Nome"];
            var cpf = row["CPF"];

            _mockClienteRepository
                .Setup(r => r.CriarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Cliente c, CancellationToken ct) => c);

            _cliente = await _clienteUseCases.CriarClienteAsync(nome, null, cpf);
        }
        catch (Exception ex)
        {
            _exception = ex;
            _scenarioContext["Exception"] = ex;
        }
    }

    [When(@"eu tentar cadastrar um novo cliente com o mesmo CPF ""(.*)""")]
    public async Task QuandoEuTentarCadastrarUmNovoClienteComOMesmoCPF(string cpf)
    {
        try
        {
            _cliente = await _clienteUseCases.CriarClienteAsync("Novo Cliente", "novo@example.com", cpf);
        }
        catch (Exception ex)
        {
            _exception = ex;
            _scenarioContext["Exception"] = ex;
        }
    }

    [When(@"eu tentar cadastrar um cliente com CPF inválido ""(.*)""")]
    public async Task QuandoEuTentarCadastrarUmClienteComCPFInvalido(string cpf)
    {
        try
        {
            _mockClienteRepository
                .Setup(r => r.CriarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Cliente c, CancellationToken ct) => c);

            _cliente = await _clienteUseCases.CriarClienteAsync("Cliente Teste", "teste@example.com", cpf);
        }
        catch (Exception ex)
        {
            _exception = ex;
            _scenarioContext["Exception"] = ex;
        }
    }

    [When(@"eu tentar cadastrar um cliente com nome vazio")]
    public async Task QuandoEuTentarCadastrarUmClienteComNomeVazio()
    {
        try
        {
            _mockClienteRepository
                .Setup(r => r.CriarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Cliente c, CancellationToken ct) => c);

            _cliente = await _clienteUseCases.CriarClienteAsync("", "teste@example.com", "12345678901");
        }
        catch (Exception ex)
        {
            _exception = ex;
            _scenarioContext["Exception"] = ex;
        }
    }

    [When(@"eu atualizar os dados do cliente:")]
    public async Task QuandoEuAtualizarOsDadosDoCliente(Table table)
    {
        try
        {
            var row = table.Rows[0];
            var nome = row["Nome"];
            var email = row["Email"];

            _cliente = await _clienteUseCases.AtualizarClienteAsync(_clienteId, nome, email);
        }
        catch (Exception ex)
        {
            _exception = ex;
            _scenarioContext["Exception"] = ex;
        }
    }

    [When(@"eu tentar atualizar os dados do cliente ""(.*)""")]
    public async Task QuandoEuTentarAtualizarOsDadosDoCliente(string clienteId)
    {
        try
        {
            var id = Guid.Parse(clienteId);
            _cliente = await _clienteUseCases.AtualizarClienteAsync(id, "Novo Nome", "novo@example.com");
        }
        catch (Exception ex)
        {
            _exception = ex;
            _scenarioContext["Exception"] = ex;
        }
    }

    [When(@"eu buscar o cliente por CPF ""(.*)""")]
    public async Task QuandoEuBuscarOClientePorCPF(string cpf)
    {
        try
        {
            _cliente = await _clienteUseCases.ObterClientePorCPFAsync(cpf);
        }
        catch (Exception ex)
        {
            _exception = ex;
            _scenarioContext["Exception"] = ex;
        }
    }

    [When(@"eu tentar cadastrar um cliente com email inválido ""(.*)""")]
    public async Task QuandoEuTentarCadastrarUmClienteComEmailInvalido(string email)
    {
        try
        {
            _mockClienteRepository
                .Setup(r => r.CriarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Cliente c, CancellationToken ct) => c);

            _cliente = await _clienteUseCases.CriarClienteAsync("Cliente Teste", email, "12345678901");
        }
        catch (Exception ex)
        {
            _exception = ex;
            _scenarioContext["Exception"] = ex;
        }
    }

    [Then(@"o cliente deve ser cadastrado com sucesso")]
    public void EntaoOClienteDeveSerCadastradoComSucesso()
    {
        _exception.Should().BeNull();
        _cliente.Should().NotBeNull();
    }

    [Then(@"o cliente deve ter um ID válido")]
    public void EntaoOClienteDeveTerUmIDValido()
    {
        _cliente.Should().NotBeNull();
        _cliente!.Id.Should().NotBeEmpty();
    }

    [Then(@"o nome do cliente deve ser ""(.*)""")]
    public void EntaoONomeDoClienteDeveSer(string nomeEsperado)
    {
        _cliente.Should().NotBeNull();
        _cliente!.Nome.Should().Be(nomeEsperado);
    }

    [Then(@"o CPF do cliente deve ser ""(.*)""")]
    public void EntaoOCPFDoClienteDeveSer(string cpfEsperado)
    {
        _cliente.Should().NotBeNull();
        _cliente!.CPF.Should().Be(cpfEsperado);
    }

    [Then(@"o email do cliente deve estar vazio")]
    public void EntaoOEmailDoClienteDeveEstarVazio()
    {
        _cliente.Should().NotBeNull();
        _cliente!.Email.Should().BeNullOrEmpty();
    }

    [Then(@"os dados do cliente devem ser atualizados com sucesso")]
    public void EntaoOsDadosDoClienteDevemSerAtualizadosComSucesso()
    {
        _exception.Should().BeNull();
        _cliente.Should().NotBeNull();
    }

    [Then(@"o email do cliente deve ser ""(.*)""")]
    public void EntaoOEmailDoClienteDeveSer(string emailEsperado)
    {
        _cliente.Should().NotBeNull();
        _cliente!.Email.Should().Be(emailEsperado);
    }

    [Then(@"o cliente deve ser encontrado")]
    public void EntaoOClienteDeveSerEncontrado()
    {
        _exception.Should().BeNull();
        _cliente.Should().NotBeNull();
    }

    [Then(@"nenhum cliente deve ser encontrado")]
    public void EntaoNenhumClienteDeveSerEncontrado()
    {
        _exception.Should().BeNull();
        _cliente.Should().BeNull();
    }
}
