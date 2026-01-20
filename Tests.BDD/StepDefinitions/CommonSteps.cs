using FluentAssertions;
using Reqnroll;
using PagamentoService.Core.Entity;
using Compartilhado.Enums;

namespace PagamentoService.Tests.BDD.StepDefinitions;

[Binding]
public class CommonSteps
{
    private readonly ScenarioContext _scenarioContext;

    public CommonSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [Then(@"deve ocorrer um erro com a mensagem ""(.*)""")]
    public void EntaoDeveOcorrerUmErroComAMensagem(string mensagemEsperada)
    {
        var exception = _scenarioContext.Get<Exception?>("Exception");
        exception.Should().NotBeNull();
        exception!.Message.Should().Contain(mensagemEsperada);
    }

    [Then(@"o status do pagamento deve ser ""(.*)""")]
    public void EntaoOStatusDoPagamentoDeveSer(string status)
    {
        var pagamento = _scenarioContext.Get<Pagamento?>("Pagamento");
        pagamento.Should().NotBeNull();

        var statusEnum = status switch
        {
            "Pendente" => EStatusPagamento.Pendente,
            "Aprovado" => EStatusPagamento.Aprovado,
            "Rejeitado" => EStatusPagamento.Rejeitado,
            "Cancelado" => EStatusPagamento.Cancelado,
            _ => throw new ArgumentException($"Status inválido: {status}")
        };

        pagamento!.Status.Should().Be(statusEnum);
    }
}
