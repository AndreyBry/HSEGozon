using HSEGozon.PaymentsService.Abstractions;
using RabbitMQ.Client;

namespace HSEGozon.PaymentsService.Infrastructure.Messaging;

public class RabbitMqConnection : IRabbitMqConnection, IDisposable
{
    private readonly string _hostName;
    private readonly int _port;
    private readonly string _userName;
    private readonly string _password;
    private IConnection? _connection;

    public RabbitMqConnection(string hostName, string port, string userName, string password)
    {
        _hostName = hostName;
        _port = int.Parse(port);
        _userName = userName;
        _password = password;
    }

    public IConnection CreateConnection()
    {
        if (_connection?.IsOpen == true)
        {
            return _connection;
        }

        var factory = new ConnectionFactory
        {
            HostName = _hostName,
            Port = _port,
            UserName = _userName,
            Password = _password,
            AutomaticRecoveryEnabled = true
        };

        _connection = factory.CreateConnection();
        return _connection;
    }

    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();
    }
}

