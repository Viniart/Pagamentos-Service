using Microsoft.EntityFrameworkCore;
using PagamentoService.Core.Entity;

namespace PagamentoService.Infrastructure.Persistence;

public class PagamentoDbContext : DbContext
{
    public PagamentoDbContext(DbContextOptions<PagamentoDbContext> options) : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Pagamento> Pagamentos => Set<Pagamento>();
    public DbSet<WebhookEvent> WebhookEvents => Set<WebhookEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração de Cliente
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.CPF).IsRequired().HasMaxLength(11);
            entity.Property(e => e.DataCadastro).IsRequired();

            entity.HasIndex(e => e.CPF).IsUnique();
            entity.HasIndex(e => e.Email);
        });

        // Configuração de Pagamento
        modelBuilder.Entity<Pagamento>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PedidoId).IsRequired();
            entity.Property(e => e.ClienteId).IsRequired();
            entity.Property(e => e.Valor).IsRequired().HasPrecision(10, 2);
            entity.Property(e => e.QrCode).HasMaxLength(4000);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.IdPagamentoExterno).HasMaxLength(100);
            entity.Property(e => e.DataCriacao).IsRequired();
            entity.Property(e => e.DataAtualizacao).IsRequired();

            entity.HasIndex(e => e.PedidoId).IsUnique();
            entity.HasIndex(e => e.ClienteId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.IdPagamentoExterno);

            entity.HasOne(e => e.Cliente)
                .WithMany()
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuração de WebhookEvent
        modelBuilder.Entity<WebhookEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TipoEvento).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Payload).IsRequired();
            entity.Property(e => e.Processado).IsRequired();
            entity.Property(e => e.DataRecebimento).IsRequired();

            entity.HasIndex(e => e.Processado);
            entity.HasIndex(e => e.PagamentoId);
            entity.HasIndex(e => new { e.Processado, e.DataRecebimento });
        });
    }
}
