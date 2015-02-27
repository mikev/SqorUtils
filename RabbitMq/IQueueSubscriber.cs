namespace Sqor.Utils.RabbitMq
{
    public interface IQueueSubscriber
    {
        void Start();
        void Stop();
    }
}