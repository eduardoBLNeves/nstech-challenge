namespace Nstech.OrderService.Domain;

public static class Messages
{
    public const string InvalidCredentials = "E-mail ou senha inválidos.";
    public const string OrderNotFound = "Pedido não encontrado.";
    public const string ProductNotFound = "Produto não encontrado.";
    public const string InsufficientStock = "Estoque insuficiente.";
    public const string CantUpdateOrderStatus = "Não é possível alterar o status deste pedido.";
    public const string CantDeleteProduct = "Não é possível excluir um produto usado em pedidos.";
    public const string ValidationError = "Dados inválidos.";
    public const string UnexpectedError = "Ocorreu um erro inesperado.";
    public const string ConnectionStringNotFound = "A conexão com o banco de dados não foi encontrada.";
}
