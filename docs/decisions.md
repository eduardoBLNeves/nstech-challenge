# Decisões

## Pedido nasce como Placed

O pedido é criado já em `Placed`.
Embora tenha o status `Draft` no enum, não houve necessidade de criar o pedido como rascunho.

O preço do item é o preço do produto naquele momento. O total é a soma de `unitPrice * quantity`.

## Estoque

Criar o pedido só confere se a quantidade cabe no saldo. Não diminui o valor no estoque.

Confirmar diminui o estoque e grava o pedido junto. Se o saldo não couber, a confirmação falha e nada é gravado (tudo é salvo no mesmo ponto).

Cancelar um pedido `Placed` só muda o status. Cancelar um pedido `Confirmed` devolve a quantidade ao estoque.

## Idempotência

Confirmar um pedido `Confirmed` ou cancelar um pedido que já está `Canceled` retorna o objeto sem fazer alterações.

Confirmar um pedido cancelado ou cancelar fora de `Placed` ou `Confirmed` retornam 409.

## Response da API

A API sempre retorna o mesmo JSON, ApiResponse<T>. A propriedade  `body` traz o conteudo do retorno, serializado. O `message` é preenchido quando ocorre um erro erro, e o campo statusCode informa o status do retorno.

```json
{ "success": true, "statusCode": 201, "message": null, "body": { } }
{ "success": false, "statusCode": 400, "message": "Dados inválidos.", "body": null }
```

Detalhes internos das exceções não vão para o front, são tratadas num middleware próprio.

## Índices


- `IX_Orders_CustomerId_Status_CreatedAt` para otimizar a pesquisa de orders por cliente, status e data.
- `IX_Orders_CreatedAt` para otimizar a pesquisa de orders apenas por data.
- `IX_OrderItems_ProductId` para otimizar a pesquisa do produto.
- `IX_OrderItems_OrderId` para otimizar a pesquisa de Order em OrderItems.
- `IX_Users_Email` para otimizar o login caso o sistema tenha muitos usuários.

## Autenticação

A senha fica com hash PBKDF2 (salt e SHA256). O login devolve um JWT assinado com HMAC-SHA256. 
Os endpoints de pedido exigem o token (Authorize). O endpoint de login `POST /auth/login` é AllowAnonymous.

O usuário do seed é Email:`user@example.com` / Senha: `12345678`.

## Banco no startup

A API aplica as migrations e o seed ao subir. O `docker compose up` sobe os dois juntos.
Os três produtos do seed usam ids fixos e só entram se a tabela estiver vazia. Reiniciar a API não devolve estoque.

## Swagger

O Swagger UI mostra a duração de cada request executado por ele (`DisplayRequestDuration`). 
O cadeado usa o esquema Bearer: o valor colado é só o token, sem o prefixo `Bearer`.

## Testes Unitarios

Os testes unitarios foram feitos com xUnit. Foram testados cenarios positivos e negativos da camada de application.

## Inteligencia Artificial

Todo o projeto passou por verificação de inteligência artificial (Grok 4.7), para evitar edge cases e falhas humanas.
Os testes unitarios foram feitos por IA, certificando que todos os cenarios foram atendidos.
Após análise da IA, houve outra análise humana para evitar alterações indevidas.