# language: pt-BR
Funcionalidade: Criação de Pagamento
  Como um sistema de pedidos
  Eu quero criar pagamentos
  Para processar as transações dos clientes

  Cenário: Criar pagamento com sucesso
    Dado que existe um cliente com CPF "12345678901"
    E que existe um pedido com ID "550e8400-e29b-41d4-a716-446655440000"
    Quando eu criar um pagamento com os seguintes dados:
      | PedidoId                             | ClienteId                            | Valor  |
      | 550e8400-e29b-41d4-a716-446655440000 | 550e8400-e29b-41d4-a716-446655440001 | 100.00 |
    Então o pagamento deve ser criado com sucesso
    E o status do pagamento deve ser "Pendente"
    E o pagamento deve ter um ID válido

  Cenário: Criar pagamento com valor inválido
    Dado que existe um cliente com CPF "12345678901"
    E que existe um pedido com ID "550e8400-e29b-41d4-a716-446655440000"
    Quando eu tentar criar um pagamento com valor "0"
    Então deve ocorrer um erro com a mensagem "Valor do pagamento deve ser maior que zero."

  Cenário: Criar pagamento com valor negativo
    Dado que existe um cliente com CPF "12345678901"
    E que existe um pedido com ID "550e8400-e29b-41d4-a716-446655440000"
    Quando eu tentar criar um pagamento com valor "-50"
    Então deve ocorrer um erro com a mensagem "Valor do pagamento deve ser maior que zero."

  Cenário: Criar pagamento duplicado para o mesmo pedido
    Dado que existe um cliente com CPF "12345678901"
    E que existe um pedido com ID "550e8400-e29b-41d4-a716-446655440000"
    E que já existe um pagamento para o pedido "550e8400-e29b-41d4-a716-446655440000"
    Quando eu tentar criar um novo pagamento para o mesmo pedido
    Então deve ocorrer um erro com a mensagem "Já existe um pagamento para este pedido."

  Cenário: Gerar QR Code para pagamento
    Dado que existe um cliente com CPF "12345678901"
    E que existe um pedido com ID "550e8400-e29b-41d4-a716-446655440000"
    E que o serviço do Mercado Pago está disponível
    Quando eu criar um pagamento com geração de QR Code:
      | PedidoId                             | ClienteId                            | Valor  |
      | 550e8400-e29b-41d4-a716-446655440000 | 550e8400-e29b-41d4-a716-446655440001 | 150.00 |
    Então o pagamento deve ser criado com sucesso
    E o QR Code deve ser gerado
    E o QR Code não deve estar vazio
    E deve ter um ID de pagamento externo
