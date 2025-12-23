using RabbitMQ.Client;

namespace HSEGozon.OrdersService.Abstractions;

public interface IRabbitMqConnection
{
    IConnection CreateConnection();
}

