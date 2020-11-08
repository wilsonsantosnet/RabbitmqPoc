using Domain;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Worker
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Worker Run");
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "Messages",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                var item = default(Message);
                consumer.Received += (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        item = System.Text.Json.JsonSerializer.Deserialize<Message>(message);
                        Console.WriteLine($"Message : {item.Name}, {item.Value}");
                        channel.BasicAck(ea.DeliveryTag,false);

                    }
                    catch (Exception ex)
                    {
                        channel.BasicNack(ea.DeliveryTag, false, true);
                        
                    }
                };
                channel.BasicConsume(queue: "Messages",
                                     autoAck: false,
                                     consumer: consumer);

                Console.WriteLine("Press [Enter] to exit");
                Console.ReadLine();
            }

            
        }
    }
}
