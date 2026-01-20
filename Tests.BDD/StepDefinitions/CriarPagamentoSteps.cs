using FluentAssertions;
using Moq;
using PagamentoService.Core.Entity;
using PagamentoService.Core.Interfaces;
using PagamentoService.Core.UseCases;
using Reqnroll;
using Compartilhado.Enums;

namespace PagamentoService.Tests.BDD.StepDefinitions;

[Binding]
public class CriarPagamentoSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly Mock<IPagamentoRepository> _mockPagamentoRepository;
    private readonly Mock<IMercadoPagoService> _mockMercadoPagoService;
    private readonly PagamentoUseCases _pagamentoUseCases;

    private Guid _pedidoId;
    private Guid _clienteId;
    private Pagamento? _pagamentoCriado;
    private Exception? _exception;

    public CriarPagamentoSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _mockPagamentoRepository = new Mock<IPagamentoRepository>();
        _mockMercadoPagoService = new Mock<IMercadoPagoService>();
        _pagamentoUseCases = new PagamentoUseCases(_mockPagamentoRepository.Object, _mockMercadoPagoService.Object);
    }

    [Given(@"que existe um cliente com CPF ""(.*)""")]
    public void DadoQueExisteUmClienteComCPF(string cpf)
    {
        _clienteId = Guid.NewGuid();
        // Cliente existe - não precisa fazer nada, apenas registrar o ID
    }

    [Given(@"que existe um pedido com ID ""(.*)""")]
    public void DadoQueExisteUmPedidoComID(string pedidoId)
    {
        _pedidoId = Guid.Parse(pedidoId);
    }

    [Given(@"que já existe um pagamento para o pedido ""(.*)""")]
    public void DadoQueJaExisteUmPagamentoParaOPedido(string pedidoId)
    {
        var pedidoGuid = Guid.Parse(pedidoId);
        var pagamentoExistente = new Pagamento(pedidoGuid, _clienteId, 100m);

        _mockPagamentoRepository
            .Setup(r => r.ObterPorPedidoIdAsync(pedidoGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagamentoExistente);
    }

    [Given(@"que o serviço do Mercado Pago está disponível")]
    public void DadoQueOServicoDoMercadoPagoEstaDisponivel()
    {
        var qrCodeResponse = new QrCodeResponse(
            "00020101021243650016COM.MERCADOLIBRE020130636f8c6742-5b05-4158-bf69-6bbb3fa8c1e",
            "MP123456789",
            "pending"
        );

        _mockMercadoPagoService
            .Setup(s => s.GerarQrCodeAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(qrCodeResponse);
    }

    [When(@"eu criar um pagamento com os seguintes dados:")]
    public async Task QuandoEuCriarUmPagamentoComOsSeguintesDados(Table table)
    {
        try
        {
            var row = table.Rows[0];
            var pedidoId = Guid.Parse(row["PedidoId"]);
            var clienteId = Guid.Parse(row["ClienteId"]);
            var valor = decimal.Parse(row["Valor"]);

            _clienteId = clienteId;

            // Setup: não existe pagamento para este pedido
            _mockPagamentoRepository
                .Setup(r => r.ObterPorPedidoIdAsync(pedidoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Pagamento?)null);

            // Setup: criar pagamento retorna o pagamento criado
            _mockPagamentoRepository
                .Setup(r => r.CriarAsync(It.IsAny<Pagamento>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Pagamento p, CancellationToken ct) => p);

            // Setup: atualizar pagamento retorna o pagamento atualizado
            _mockPagamentoRepository
                .Setup(r => r.AtualizarAsync(It.IsAny<Pagamento>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Pagamento p, CancellationToken ct) => p);

            _pagamentoCriado = await _pagamentoUseCases.CriarPagamentoAsync(pedidoId, clienteId, valor, false);
            _scenarioContext["Pagamento"] = _pagamentoCriado;
        }
        catch (Exception ex)
        {
            _exception = ex;
            _scenarioContext["Exception"] = ex;
        }
    }

    [When(@"eu tentar criar um pagamento com valor ""(.*)""")]
    public async Task QuandoEuTentarCriarUmPagamentoComValor(string valorStr)
    {
        try
        {
            var valor = decimal.Parse(valorStr);
            _pagamentoCriado = await _pagamentoUseCases.CriarPagamentoAsync(_pedidoId, _clienteId, valor, false);
            _scenarioContext["Pagamento"] = _pagamentoCriado;
        }
        catch (Exception ex)
        {
            _exception = ex;
            _scenarioContext["Exception"] = ex;
        }
    }

    [When(@"eu tentar criar um novo pagamento para o mesmo pedido")]
    public async Task QuandoEuTentarCriarUmNovoPagamentoParaOMesmoPedido()
    {
        try
        {
            _pagamentoCriado = await _pagamentoUseCases.CriarPagamentoAsync(_pedidoId, _clienteId, 100m, false);
            _scenarioContext["Pagamento"] = _pagamentoCriado;
        }
        catch (Exception ex)
        {
            _exception = ex;
            _scenarioContext["Exception"] = ex;
        }
    }

    [When(@"eu criar um pagamento com geração de QR Code:")]
    public async Task QuandoEuCriarUmPagamentoComGeracaoDeQRCode(Table table)
    {
        try
        {
            var row = table.Rows[0];
            var pedidoId = Guid.Parse(row["PedidoId"]);
            var clienteId = Guid.Parse(row["ClienteId"]);
            var valor = decimal.Parse(row["Valor"]);

            _clienteId = clienteId;

            // Setup: não existe pagamento para este pedido
            _mockPagamentoRepository
                .Setup(r => r.ObterPorPedidoIdAsync(pedidoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Pagamento?)null);

            // Setup: criar pagamento retorna o pagamento criado
            _mockPagamentoRepository
                .Setup(r => r.CriarAsync(It.IsAny<Pagamento>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Pagamento p, CancellationToken ct) => p);

            // Setup: atualizar pagamento retorna o pagamento atualizado
            _mockPagamentoRepository
                .Setup(r => r.AtualizarAsync(It.IsAny<Pagamento>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Pagamento p, CancellationToken ct) => p);

            _pagamentoCriado = await _pagamentoUseCases.CriarPagamentoAsync(pedidoId, clienteId, valor, true);
            _scenarioContext["Pagamento"] = _pagamentoCriado;
        }
        catch (Exception ex)
        {
            _exception = ex;
            _scenarioContext["Exception"] = ex;
        }
    }

    [Then(@"o pagamento deve ser criado com sucesso")]
    public void EntaoOPagamentoDeveSerCriadoComSucesso()
    {
        _exception.Should().BeNull();
        _pagamentoCriado.Should().NotBeNull();
    }

    [Then(@"o pagamento deve ter um ID válido")]
    public void EntaoOPagamentoDeveTerUmIDValido()
    {
        _pagamentoCriado.Should().NotBeNull();
        _pagamentoCriado!.Id.Should().NotBeEmpty();
    }


    [Then(@"o QR Code deve ser gerado")]
    public void EntaoOQRCodeDeveSerGerado()
    {
        _pagamentoCriado.Should().NotBeNull();
        _pagamentoCriado!.QrCode.Should().NotBeNull();
    }

    [Then(@"o QR Code não deve estar vazio")]
    public void EntaoOQRCodeNaoDeveEstarVazio()
    {
        _pagamentoCriado.Should().NotBeNull();
        _pagamentoCriado!.QrCode.Should().NotBeEmpty();
    }

    [Then(@"deve ter um ID de pagamento externo")]
    public void EntaoDeveTerUmIDDePagamentoExterno()
    {
        _pagamentoCriado.Should().NotBeNull();
        _pagamentoCriado!.IdPagamentoExterno.Should().NotBeNullOrEmpty();
    }
}
