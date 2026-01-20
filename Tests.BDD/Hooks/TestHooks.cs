using Reqnroll;

namespace PagamentoService.Tests.BDD.Hooks;

[Binding]
public class TestHooks
{
    [AfterScenario]
    public void AfterScenario(ScenarioContext scenarioContext)
    {
        if (scenarioContext.TestError != null)
        {
            Console.WriteLine($"Cenário falhou: {scenarioContext.ScenarioInfo.Title}");
            Console.WriteLine($"Erro: {scenarioContext.TestError.Message}");
        }
    }

    [AfterStep]
    public void AfterStep(ScenarioContext scenarioContext)
    {
        if (scenarioContext.TestError != null)
        {
            Console.WriteLine($"Step falhou: {scenarioContext.StepContext.StepInfo.Text}");
        }
    }
}
