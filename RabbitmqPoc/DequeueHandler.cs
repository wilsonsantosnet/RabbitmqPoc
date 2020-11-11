using Domain;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitmqPoc
{
    public class DequeueHandler : IHostedService
    {
        //private Timer _timer;
        private IModel _chanel;
        private IConnection _conn;
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            this._conn = factory.CreateConnection();
            this._chanel = _conn.CreateModel();
            this._chanel.QueueDeclare(queue: "Messages",
                                   durable: false,
                                   exclusive: false,
                                   autoDelete: false,
                                   arguments: null);

            //_timer = new Timer(Consumer, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3));
            this.Consumer();
            return Task.CompletedTask;

        }

        private void Consumer()
        {

          

            var consumer = new EventingBasicConsumer(_chanel);
            var item = default(Message);
            consumer.Received += (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    item = System.Text.Json.JsonSerializer.Deserialize<Message>(message);
                    Console.WriteLine($"Message : {item.Name}, {item.Value}");
                    _chanel.BasicAck(ea.DeliveryTag, false);

                }
                catch (Exception ex)
                {
                    _chanel.BasicNack(ea.DeliveryTag, false, true);

                }
            };
            _chanel.BasicConsume(queue: "Messages",
                                     autoAck: false,
                                     consumer: consumer);



        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this._conn.Dispose();
            this._chanel.Dispose();
            //_timer.Dispose();
            return Task.CompletedTask;
        }
    }
}
