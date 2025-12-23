using RabbitMQ.Client;

namespace HSEGozon.PaymentsService.Abstractions;

public interface IRabbitMqConnection
{
    IConnection CreateConnection();
}

