using FluentAssertions;
using Moq;
using PagamentoService.Core.Entity;
using PagamentoService.Core.Interfaces;
using PagamentoService.Core.UseCases;
using Reqnroll;
using Compartilhado.Enums;

namespace PagamentoService.Tests.BDD.StepDefinitions;

[Binding]
public class ConfirmarPagamentoSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly Mock<IPagamentoRepository> _mockPagamentoRepository;
    private readonly Mock<IMercadoPagoService> _mockMercadoPagoService;
    private readonly PagamentoUseCases _pagamentoUseCases;

    private Pagamento? _pagamento;
    private string _idPagamentoExterno = string.Empty;
    private Exception? _exception;

    public ConfirmarPagamentoSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _mockPagamentoRepository = new Mock<IPagamentoRepository>();
        _mockMercadoPagoService = new Mock<IMercadoPagoService>();
        _pagamentoUseCases = new PagamentoUseCases(_mockPagamentoRepository.Object, _mockMercadoPagoService.Object);
    }

    [Given(@"que existe um pagamento pendente com ID externo ""(.*)""")]
    public void DadoQueExisteUmPagamentoPendenteComIDExterno(string idExterno)
    {
        _idPagamentoExterno = idExterno;
        var pedidoId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();

        _pagamento = new Pagamento(pedidoId, clienteId, 100m);
        _pagamento.DefinirQrCode("QR_CODE_EXEMPLO", idExterno);

        _mockPagamentoRepository
            .Setup(r => r.ObterPorIdPagamentoExternoAsync(idExterno, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_pagamento);

        _mockPagamentoRepository
            .Setup(r => r.AtualizarAsync(It.IsAny<Pagamento>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pagamento p, CancellationToken ct) => p);
    }

    [Given(@"que existe um pagamento aprovado com ID externo ""(.*)""")]
    public void DadoQueExisteUmPagamentoAprovadoComIDExterno(string idExterno)
    {
        _idPagamentoExterno = idExterno;
        var pedidoId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();

        _pagamento = new Pagamento(pedidoId, clienteId, 100m);
        _pagamento.DefinirQrCode("QR_CODE_EXEMPLO", idExterno);
        _pagamento.ConfirmarPagamento(DateTime.UtcNow);

        _mockPagamentoRepository
            .Setup(r => r.ObterPorIdPagamentoExternoAsync(idExterno, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_pagamento);

        _mockPagamentoRepository
            .Setup(r => r.AtualizarAsync(It.IsAny<Pagamento>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pagamento p, CancellationToken ct) => p);
    }

    [Given(@"que não existe um pagamento com ID externo ""(.*)""")]
    public void DadoQueNaoExisteUmPagamentoComIDExterno(string idExterno)
    {
        _idPagamentoExterno = idExterno;

        _mockPagamentoRepository
            .Setup(r => r.ObterPorIdPagamentoExternoAsync(idExterno, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pagamento?)null);
    }

    [When(@"eu confirmar o pagamento com ID externo ""(.*)""")]
    public async Task QuandoEuConfirmarOPagamentoComIDExterno(string idExterno)
    {
        try
        {
            var dataPagamento = DateTime.UtcNow;
            _pagamento = await _pagamentoUseCases.ConfirmarPagamentoAsync(idExterno, dataPagamento);
            _scenarioContext["Pagamento"] = _pagamento;
        }
        catch (Exception ex)
        {
            _exception = ex;
            _scenarioContext["Exception"] = ex;
        }
    }

    [When(@"eu tentar confirmar o pagamento com ID externo ""(.*)""")]
    public async Task QuandoEuTentarConfirmarOPagamentoComIDExterno(string idExterno)
    {
        try
        {
            var dataPagamento = DateTime.UtcNow;
            _pagamento = await _pagamentoUseCases.ConfirmarPagamentoAsync(idExterno, dataPagamento);
            _scenarioContext["Pagamento"] = _pagamento;
        }
        catch (Exception ex)
        {
            _exception = ex;
            _scenarioContext["Exception"] = ex;
        }
    }

    [When(@"eu rejeitar o pagamento com ID externo ""(.*)""")]
    public async Task QuandoEuRejeitarOPagamentoComIDExterno(string idExterno)
    {
        try
        {
            _pagamento = await _pagamentoUseCases.RejeitarPagamentoAsync(idExterno);
            _scenarioContext["Pagamento"] = _pagamento;
        }
        catch (Exception ex)
        {
            _exception = ex;
            _scenarioContext["Exception"] = ex;
        }
    }

    [When(@"eu tentar rejeitar o pagamento com ID externo ""(.*)""")]
    public async Task QuandoEuTentarRejeitarOPagamentoComIDExterno(string idExterno)
    {
        try
        {
            _pagamento = await _pagamentoUseCases.RejeitarPagamentoAsync(idExterno);
            _scenarioContext["Pagamento"] = _pagamento;
        }
        catch (Exception ex)
        {
            _exception = ex;
            _scenarioContext["Exception"] = ex;
        }
    }

    [Then(@"o pagamento deve ser confirmado com sucesso")]
    public void EntaoOPagamentoDeveSerConfirmadoComSucesso()
    {
        _exception.Should().BeNull();
        _pagamento.Should().NotBeNull();
    }

    [Then(@"a data de pagamento deve estar definida")]
    public void EntaoADataDePagamentoDeveEstarDefinida()
    {
        _pagamento.Should().NotBeNull();
        _pagamento!.DataPagamento.Should().NotBeNull();
    }

    [Then(@"o status do pagamento deve permanecer ""(.*)""")]
    public void EntaoOStatusDoPagamentoDevePermanecer(string status)
    {
        _pagamento.Should().NotBeNull();

        var statusEnum = status switch
        {
            "Pendente" => EStatusPagamento.Pendente,
            "Aprovado" => EStatusPagamento.Aprovado,
            "Rejeitado" => EStatusPagamento.Rejeitado,
            "Cancelado" => EStatusPagamento.Cancelado,
            _ => throw new ArgumentException($"Status inválido: {status}")
        };

        _pagamento!.Status.Should().Be(statusEnum);
    }

    [Then(@"o pagamento deve ser rejeitado com sucesso")]
    public void EntaoOPagamentoDeveSerRejeitadoComSucesso()
    {
        _exception.Should().BeNull();
        _pagamento.Should().NotBeNull();
    }
}
