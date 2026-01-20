# PagamentoService

Microsserviço responsável pelo processamento de pagamentos do sistema Tech Challenge.

## Tecnologias

- .NET 8.0
- SQL Server
- Entity Framework Core
- RabbitMQ (MassTransit)
- Reqnroll (BDD)
- Docker

## Estrutura

```
PagamentoService/
├── Api/                 # Controllers, DTOs, Program.cs
├── Core/                # Entidades, Interfaces, UseCases
├── Infrastructure/      # Repositórios, EF Core, Migrations
├── Compartilhado/       # Enums e Events compartilhados
├── Tests.Unit/          # Testes unitários
├── Tests.BDD/           # Testes BDD (Gherkin)
└── .github/workflows/   # CI/CD
```

## Execução

### Subir SQL Server

```
docker run -d --name sqlserver -p 1433:1433 -e ACCEPT_EULA=Y -e SA_PASSWORD=SenhaBanco!123 mcr.microsoft.com/mssql/server:2022-latest
```

### Executar serviço

```
dotnet run --project Api
```

### Executar com Docker

```
docker build -t pagamento-service . && docker run -p 5003:8080 -e ConnectionStrings__PagamentosDb="Server=host.docker.internal;Database=PagamentosDb;User ID=sa;Password=SenhaBanco!123;TrustServerCertificate=True;" pagamento-service
```

## Testes

### Executar testes unitários

```
dotnet test Tests.Unit/Tests.Unit.csproj
```

### Executar testes BDD

```
dotnet test Tests.BDD/Tests.BDD.csproj
```

### Executar todos os testes

```
dotnet test Tests.Unit/Tests.Unit.csproj && dotnet test Tests.BDD/Tests.BDD.csproj
```

### Testes com cobertura (relatório HTML)

```
dotnet test Tests.Unit/Tests.Unit.csproj --collect:"XPlat Code Coverage" --results-directory ./TestResults -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura && reportgenerator -reports:"./TestResults/**/coverage.cobertura.xml" -targetdir:"./TestResults/CoverageReport" -reporttypes:Html && start ./TestResults/CoverageReport/index.html
```

### Testes com cobertura (resumo no terminal)

```
dotnet test Tests.Unit/Tests.Unit.csproj --collect:"XPlat Code Coverage" --results-directory ./TestResults -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura && reportgenerator -reports:"./TestResults/**/coverage.cobertura.xml" -targetdir:"./TestResults/CoverageReport" -reporttypes:TextSummary && cat ./TestResults/CoverageReport/Summary.txt
```

## Endpoints

| Método | Rota | Descrição |
|--------|------|-----------|
| POST | /api/Pagamentos/gerar-qrcode | Gerar QR Code de pagamento |
| GET | /api/Pagamentos/{id} | Obter pagamento por ID |
| GET | /api/Pagamentos/pedido/{pedidoId} | Obter pagamento por pedido |
| GET | /api/Pagamentos/cliente/{clienteId} | Listar pagamentos do cliente |
| GET | /api/Pagamentos/{id}/status | Verificar status |
| POST | /api/Pagamentos/{id}/confirmar | Confirmar pagamento |
| GET | /api/Clientes | Listar clientes |
| POST | /api/Clientes | Criar cliente |
| GET | /api/Clientes/{id} | Obter cliente por ID |
| GET | /api/Clientes/cpf/{cpf} | Obter cliente por CPF |
| GET | /health | Health check |

## Eventos

**Consome:**
- `PedidoCriadoEvent` - Cria pagamento pendente quando um pedido é criado

**Publica:**
- `PagamentoConfirmadoEvent` - Quando um pagamento é confirmado
- `PagamentoFalhouEvent` - Quando um pagamento falha

## Variáveis de Ambiente

| Variável | Descrição |
|----------|-----------|
| ConnectionStrings__PagamentosDb | String de conexão SQL Server |
| RabbitMQ__Host | Host do RabbitMQ |
| RabbitMQ__Username | Usuário RabbitMQ |
| RabbitMQ__Password | Senha RabbitMQ |
| MercadoPago__AccessToken | Token do Mercado Pago |
