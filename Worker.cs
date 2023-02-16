using PCSC;
using PCSC.Monitoring;
using RabbitMQ.Client;
using System.Text;
namespace PcscCardReader
{
    public class Worker : BackgroundService
    {
        IMonitorFactory monitorFactory;
        ISCardMonitor monitor;
        ConnectionFactory factory;
        IModel? channel;
        public Worker()
        {
            monitorFactory = MonitorFactory.Instance;
            monitor = monitorFactory.Create(SCardScope.System);
            factory = new ConnectionFactory { HostName = "localhost" };
            var connection = factory.CreateConnection();
            channel = connection.CreateModel();
            channel.ExchangeDeclare(Constants.ExchangeName, type: ExchangeType.Direct);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            monitor.Cancel();
            monitor.Dispose();
            return base.StopAsync(cancellationToken);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            
            monitor.CardInserted += (sender, args) =>
            {
                channel.BasicPublish(
                    Constants.ExchangeName,
                    routingKey: Constants.RoutingKey,
                    basicProperties: null,
                    body: args.Atr
                    );

            };

            monitor.Start(Constants.ReaderName);
        }
    }
}