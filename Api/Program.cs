using Microsoft.EntityFrameworkCore;
using PagamentoService.Core.Interfaces;
using PagamentoService.Core.UseCases;
using PagamentoService.Infrastructure.Persistence;
using PagamentoService.Infrastructure.ExternalServices;
using PagamentoService.Api.Consumers;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// SQL Server Configuration
builder.Services.AddDbContext<PagamentoDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("PagamentosDb"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)));

// Registrar repositórios e use cases
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IPagamentoRepository, PagamentoRepository>();
builder.Services.AddScoped<IWebhookRepository, WebhookRepository>();
builder.Services.AddScoped<IMercadoPagoService, MercadoPagoServiceMock>(); // Em produção, usar implementação real
builder.Services.AddScoped<ClienteUseCases>();
builder.Services.AddScoped<PagamentoUseCases>();

// MassTransit / RabbitMQ
builder.Services.AddMassTransit(x =>
{
    // Registrar consumers
    x.AddConsumer<PedidoCriadoConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

// Add controllers
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Pagamento Service API", Version = "v1" });
});

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PagamentoDbContext>("database");

var app = builder.Build();

// Criar banco de dados automaticamente
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PagamentoDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/healthz");
app.MapHealthChecks("/readyz");

app.Run();
