using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Sqor.Utils.Logging;

namespace Sqor.Utils.RabbitMq
{
    public abstract class QueueSubscriber<TPacket> : DefaultBasicConsumer, IQueueSubscriber
    {
        protected readonly ConnectionFactory connectionFactory;

        private IConnection connection;
        private string consumerTag;
        private string queueName;

        protected QueueSubscriber(ConnectionFactory connectionFactory, string queueName)
        {
            this.connectionFactory = connectionFactory;
            this.queueName = queueName;
        }

        public string QueueName
        {
            get { return queueName; }
        }

        public virtual void Start()
        {
            connection = connectionFactory.CreateConnection();
            Model = connection.CreateModel();
            DeclareQueues();
            consumerTag = Model.BasicConsume(queueName, false, this);
        }

        protected virtual void DeclareQueues()
        {
            Model.QueueDeclare(queueName, true, false, false, null);            
        }

        public virtual void Stop()
        {
            Model.BasicCancel(consumerTag);
            connection.Dispose();
        }

        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
        {
            this.LogInfo("Received packet on " + exchange);

            base.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);

            using (var stream = new MemoryStream(body))
            using (var reader = new StreamReader(stream))
            {
                var json = reader.ReadToEnd();

                TPacket packet;
                try
                {
                    packet = JsonConvert.DeserializeObject<TPacket>(json);
                }
                catch (Exception e)
                {
                    this.LogError("Error deserializing packet on exchange" + exchange, e);                    
                    return;
                }

                // It is *imperative* that this happen on another thread.  If you invoke any blocking operations
                // on the queue, it will cause a deadlock.
                Task.Run(async () =>
                {
                    try
                    {
                        await HandlePacket(consumerTag, deliveryTag, redelivered, packet, body);
                    }
                    catch (Exception e)
                    {
                        this.LogError("Error handling packet " + json, e);
                    }
                });
            }
        }

        protected abstract Task HandlePacket(string consumerTag, ulong deliveryTag, bool redelivered, TPacket packet, byte[] body);

        protected void Acknowledge(ulong deliveryTag)
        {
            Model.BasicAck(deliveryTag, false);
/*
            using (var connection = connectionFactory.CreateConnection())
            {
                var model = connection.CreateModel();
                model.BasicAck(deliveryTag, false);
            }            
*/
        }
    }
}