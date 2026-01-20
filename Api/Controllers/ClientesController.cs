using Microsoft.AspNetCore.Mvc;
using PagamentoService.Core.UseCases;
using PagamentoService.Core.Entity;
using PagamentoService.Api.DTOs;

namespace PagamentoService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly ClienteUseCases _clienteUseCases;
    private readonly ILogger<ClientesController> _logger;

    public ClientesController(
        ClienteUseCases clienteUseCases,
        ILogger<ClientesController> logger)
    {
        _clienteUseCases = clienteUseCases;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ClienteResponse>> CriarCliente(
        [FromBody] CriarClienteRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var cliente = await _clienteUseCases.CriarClienteAsync(
                request.Nome,
                request.Email,
                request.CPF,
                cancellationToken);

            _logger.LogInformation("Cliente {ClienteId} criado com sucesso", cliente.Id);

            return CreatedAtAction(
                nameof(ObterPorId),
                new { id = cliente.Id },
                MapearParaResponse(cliente));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cliente já existe com CPF {CPF}", request.CPF);
            return Conflict(new { erro = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao criar cliente");
            return BadRequest(new { erro = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar cliente");
            return StatusCode(500, new { erro = "Erro interno ao criar cliente" });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<ClienteResponse>>> ObterTodos(CancellationToken cancellationToken)
    {
        var clientes = await _clienteUseCases.ObterTodosClientesAsync(cancellationToken);
        return Ok(clientes.Select(MapearParaResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClienteResponse>> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var cliente = await _clienteUseCases.ObterClientePorIdAsync(id, cancellationToken);
        if (cliente == null)
            return NotFound(new { erro = "Cliente não encontrado" });

        return Ok(MapearParaResponse(cliente));
    }

    [HttpGet("cpf/{cpf}")]
    public async Task<ActionResult<ClienteResponse>> ObterPorCPF(string cpf, CancellationToken cancellationToken)
    {
        var cliente = await _clienteUseCases.ObterClientePorCPFAsync(cpf, cancellationToken);
        if (cliente == null)
            return NotFound(new { erro = "Cliente não encontrado" });

        return Ok(MapearParaResponse(cliente));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ClienteResponse>> AtualizarCliente(
        Guid id,
        [FromBody] AtualizarClienteRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var cliente = await _clienteUseCases.AtualizarClienteAsync(
                id,
                request.Nome,
                request.Email,
                cancellationToken);

            _logger.LogInformation("Cliente {ClienteId} atualizado com sucesso", id);

            return Ok(MapearParaResponse(cliente));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { erro = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar cliente {ClienteId}", id);
            return StatusCode(500, new { erro = "Erro interno ao atualizar cliente" });
        }
    }

    private static ClienteResponse MapearParaResponse(Cliente cliente)
    {
        return new ClienteResponse(
            cliente.Id,
            cliente.Nome,
            cliente.Email,
            cliente.CPF,
            cliente.DataCadastro
        );
    }
}
