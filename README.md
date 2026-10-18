# Order Service

API de pedidos com itens, estoque e autenticação JWT.

## Como rodar

### Docker

Na raiz do repositório:

```bash
docker compose up --build
```

A API roda em http://localhost:18482. 
O banco de dados Postgres sobe junto, na porta 15432, e a API aplica as migrations sozinha na primeira vez que roda, subindo também o seed com alguns dados para consulta.
As credenciais do banco de dados de desenvolvimento são: usuário: `orders` e senha:`orders`.
Após o `up`, o Swagger estará disponivel em http://localhost:18482/swagger/index.html


Para parar:

```bash
docker compose down
```

O volume `pgdata` guarda o banco entre um `up` e outro. `docker compose down -v` apaga esse volume.

### Local

Pra subir apenas o banco e rodar a API na máquina:

```bash
docker compose up db -d
dotnet run --project src/Nstech.OrderService.Api
```

## Variáveis de ambiente

| Variável | Uso |
| --- | --- |
| `ConnectionStrings__Default` | Connection string do Postgres |
| `Jwt__Issuer` | Emissor do token |
| `Jwt__Audience` | Audiência do token |
| `Jwt__Key` | Key de no mínimo 32 caracteres |
| `Jwt__ExpiresMinutes` | Validade do token (minutos) |

No Compose e no perfil local esses valores já estão definidos. As chaves servem só para este projeto.

## Autenticação

`POST /auth/login`

```json
{
  "email": "user@example.com",
  "password": "12345678"
}
```

A resposta traz `accessToken` e `expiresAt`. Nos outros endpoints envie `Authorization: Bearer {accessToken}`.

No Swagger, use o botão Authorize e cole só o token (não é necessario o prefixo `Bearer`).

## Dados do Seed (Criados no primeiro Run da API)

Usuário: `user@example.com` / `12345678`.

Produtos:

| Produto | Id | Preço | Estoque |
| --- | --- | --- | --- |
| Notebook | `11111111-1111-1111-1111-111111111111` | 3500.00 | 20 |
| Mouse | `22222222-2222-2222-2222-222222222222` | 80.00 | 100 |
| Keyboard | `33333333-3333-3333-3333-333333333333` | 220.00 | 50 |


## Endpoints

### Criar pedido

`POST /orders`

```json
{
  "customerId": "44444444-4444-4444-4444-444444444444",
  "currency": "BRL",
  "items": [
    { "productId": "22222222-2222-2222-2222-222222222222", "quantity": 2 }
  ]
}
```

O pedido nasce `Placed`. Precisa ter item, quantidade maior que zero, produto existente e saldo suficiente. Criação de itens responde 201.

### Confirmar

`POST /orders/{id}/confirm`

Só confirma `Placed` e baixa o estoque. Chamar de novo devolve o mesmo pedido, sem baixar outra vez. Confirmar um pedido cancelado responde 409.

### Cancelar

`POST /orders/{id}/cancel`

Cancela `Placed` ou `Confirmed`. Se estava confirmado, o estoque volta. Chamar de novo devolve o pedido já cancelado.

### Consultar

`GET /orders/{id}`

Devolve o pedido e os itens. 404 se não existir.

### Listar

`GET /orders?customerId=&status=&from=&to=&page=&pageSize=`

`status` aceita `Draft`, `Placed`, `Confirmed` ou `Canceled`. `from` e `to` filtram `createdAt` e são inclusivos.

Exemplo:

```bash
curl.exe http://localhost:18482/orders?page=1&pageSize=20 -H "Authorization: Bearer SEU_TOKEN"
```

## Migrations

Elas rodam no startup. Também pode ser rodada manualmente:

```bash
dotnet tool restore
dotnet ef migrations add Nome --project src/Nstech.OrderService.Infrastructure --startup-project src/Nstech.OrderService.Api --output-dir Persistence/Migrations
```

## Decisões

As demais decisões técnicas foram documentadas em: [docs/decisions.md](docs/decisions.md).
