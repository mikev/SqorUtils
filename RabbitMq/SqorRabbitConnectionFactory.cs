﻿#if FULL
using RabbitMQ.Client;

namespace Sqor.Utils.RabbitMq
{
    public class SqorRabbitConnectionFactory : ConnectionFactory
    {
        public SqorRabbitConnectionFactory()
        {
            RequestedConnectionTimeout = 20000;
            HostName = CommonSettings.RabbitMqHost;
            UserName = CommonSettings.RabbitMqUserName;
            Password = CommonSettings.RabbitMqPassword;
            VirtualHost = "/";
            Protocol = Protocols.FromEnvironment();
            RequestedHeartbeat = 30;
        }
    }
}
#endif