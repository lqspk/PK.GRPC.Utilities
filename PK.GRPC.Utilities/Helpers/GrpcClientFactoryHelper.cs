using Grpc.Core;
using System;
using System.Collections.Generic;

namespace PK.GRPC.Utilities
{
    /// <summary>
    /// Grpc客户端操作类
    /// </summary>
    public sealed class GrpcClientFactoryHelper : IDisposable
    {
        /// <summary>
        /// 频道
        /// </summary>
        private Channel channel;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="serverAddress">连接服务端地址</param>
        /// <param name="options">设置项，如果为null，则默认设置接收和发送数据最大为2147483647字节</param>
        public GrpcClientFactoryHelper(string serverAddress, List<ChannelOption> options = null)
        {
            if (options == null)
            {
                options = new List<ChannelOption>();
                options.Add(new ChannelOption(ChannelOptions.MaxSendMessageLength, int.MaxValue));
                options.Add(new ChannelOption(ChannelOptions.MaxReceiveMessageLength, int.MaxValue));
            }

            this.channel = new Channel(serverAddress, ChannelCredentials.Insecure, options);
        }

        /// <summary>
        /// 创建一个客户端
        /// </summary>
        /// <returns></returns>
        public GrpcClient Create(IGrpcMultiMessageHandler multiMessageHandler = null)
        {
            return new GrpcClient(new GrpcService.GrpcServiceClient(channel), multiMessageHandler);
        }

        public void Dispose()
        {
            //关闭连接
            if (channel != null)
            {
                channel.ShutdownAsync();
                channel = null;
            }
        }
    }
}
