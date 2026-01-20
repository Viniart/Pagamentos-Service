# language: pt-BR
Funcionalidade: Confirmação de Pagamento
  Como um sistema de pagamentos
  Eu quero confirmar pagamentos pendentes
  Para atualizar o status das transações

  Cenário: Confirmar pagamento pendente com sucesso
    Dado que existe um pagamento pendente com ID externo "MP123456"
    Quando eu confirmar o pagamento com ID externo "MP123456"
    Então o pagamento deve ser confirmado com sucesso
    E o status do pagamento deve ser "Aprovado"
    E a data de pagamento deve estar definida

  Cenário: Tentar confirmar pagamento já aprovado
    Dado que existe um pagamento aprovado com ID externo "MP789012"
    Quando eu tentar confirmar o pagamento com ID externo "MP789012"
    Então deve ocorrer um erro com a mensagem "Pagamento já foi confirmado."
    E o status do pagamento deve permanecer "Aprovado"

  Cenário: Tentar confirmar pagamento inexistente
    Dado que não existe um pagamento com ID externo "MP999999"
    Quando eu tentar confirmar o pagamento com ID externo "MP999999"
    Então deve ocorrer um erro com a mensagem "Pagamento não encontrado."

  Cenário: Rejeitar pagamento pendente
    Dado que existe um pagamento pendente com ID externo "MP111222"
    Quando eu rejeitar o pagamento com ID externo "MP111222"
    Então o pagamento deve ser rejeitado com sucesso
    E o status do pagamento deve ser "Rejeitado"

  Cenário: Tentar rejeitar pagamento já aprovado
    Dado que existe um pagamento aprovado com ID externo "MP333444"
    Quando eu tentar rejeitar o pagamento com ID externo "MP333444"
    Então deve ocorrer um erro com a mensagem "Não é possível rejeitar um pagamento já aprovado."
