namespace PagamentoService.Core.Entity;

public class Cliente
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string CPF { get; private set; } = string.Empty;
    public DateTime DataCadastro { get; private set; } = DateTime.UtcNow;

    // Construtor para EF Core
    protected Cliente() { }

    public Cliente(string nome, string? email, string cpf)
    {
        ValidarCPF(cpf);
        ValidarNome(nome);
        ValidarEmail(email);

        Id = Guid.NewGuid();
        Nome = nome.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        CPF = LimparCPF(cpf);
        DataCadastro = DateTime.UtcNow;
    }

    public void AtualizarDados(string nome, string? email)
    {
        ValidarNome(nome);
        ValidarEmail(email);

        Nome = nome.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
    }

    private void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do cliente é obrigatório.");

        if (nome.Length > 200)
            throw new ArgumentException("Nome não pode exceder 200 caracteres.");
    }

    private void ValidarEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return;

        if (email.Length > 200)
            throw new ArgumentException("Email não pode exceder 200 caracteres.");

        if (!IsValidEmail(email))
            throw new ArgumentException("Formato de email inválido.");
    }

    private void ValidarCPF(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            throw new ArgumentException("CPF é obrigatório.");

        var cpfLimpo = LimparCPF(cpf);

        if (cpfLimpo.Length != 11)
            throw new ArgumentException("CPF deve conter 11 dígitos.");

        if (!cpfLimpo.All(char.IsDigit))
            throw new ArgumentException("CPF deve conter apenas números.");

        // Validação básica de CPF
        if (cpfLimpo.All(c => c == cpfLimpo[0]))
            throw new ArgumentException("CPF inválido.");
    }

    private string LimparCPF(string cpf)
    {
        return new string(cpf.Where(char.IsDigit).ToArray());
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
