# language: pt-BR
Funcionalidade: Gerenciamento de Cliente
  Como um sistema de cadastro
  Eu quero gerenciar clientes
  Para manter os dados dos usuários atualizados

  Cenário: Cadastrar cliente com sucesso
    Dado que não existe um cliente com CPF "12345678901"
    Quando eu cadastrar um cliente com os seguintes dados:
      | Nome          | Email                  | CPF         |
      | João da Silva | joao.silva@example.com | 12345678901 |
    Então o cliente deve ser cadastrado com sucesso
    E o cliente deve ter um ID válido
    E o nome do cliente deve ser "João da Silva"
    E o CPF do cliente deve ser "12345678901"

  Cenário: Cadastrar cliente sem email
    Dado que não existe um cliente com CPF "98765432100"
    Quando eu cadastrar um cliente sem email:
      | Nome           | CPF         |
      | Maria dos Santos | 98765432100 |
    Então o cliente deve ser cadastrado com sucesso
    E o email do cliente deve estar vazio

  Cenário: Cadastrar cliente com CPF duplicado
    Dado que já existe um cliente com CPF "11122233344"
    Quando eu tentar cadastrar um novo cliente com o mesmo CPF "11122233344"
    Então deve ocorrer um erro com a mensagem "Já existe um cliente cadastrado com este CPF."

  Cenário: Cadastrar cliente com CPF inválido
    Quando eu tentar cadastrar um cliente com CPF inválido "00000000000"
    Então deve ocorrer um erro com a mensagem "CPF inválido."

  Cenário: Cadastrar cliente com nome vazio
    Quando eu tentar cadastrar um cliente com nome vazio
    Então deve ocorrer um erro com a mensagem "Nome do cliente é obrigatório."

  Cenário: Atualizar dados do cliente
    Dado que existe um cliente cadastrado com ID "550e8400-e29b-41d4-a716-446655440010"
    Quando eu atualizar os dados do cliente:
      | Nome                | Email                    |
      | João Silva Atualizado | joao.novo@example.com   |
    Então os dados do cliente devem ser atualizados com sucesso
    E o nome do cliente deve ser "João Silva Atualizado"
    E o email do cliente deve ser "joao.novo@example.com"

  Cenário: Tentar atualizar cliente inexistente
    Dado que não existe um cliente com ID "550e8400-e29b-41d4-a716-446655440099"
    Quando eu tentar atualizar os dados do cliente "550e8400-e29b-41d4-a716-446655440099"
    Então deve ocorrer um erro com a mensagem "Cliente não encontrado."

  Cenário: Buscar cliente por CPF
    Dado que existe um cliente cadastrado com CPF "55566677788"
    Quando eu buscar o cliente por CPF "55566677788"
    Então o cliente deve ser encontrado
    E o CPF do cliente deve ser "55566677788"

  Cenário: Buscar cliente por CPF inexistente
    Dado que não existe um cliente com CPF "99988877766"
    Quando eu buscar o cliente por CPF "99988877766"
    Então nenhum cliente deve ser encontrado

  Cenário: Cadastrar cliente com email inválido
    Quando eu tentar cadastrar um cliente com email inválido "email-invalido"
    Então deve ocorrer um erro com a mensagem "Formato de email inválido."
