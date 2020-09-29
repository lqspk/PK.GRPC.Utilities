using System;
using Grpc.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PK.GRPC.Utilities
{
    /// <summary>
    /// Grpc服务帮助类
    /// </summary>
    public sealed class GrpcServerHelper : IDisposable
    {
        /// <summary>
        /// Grpc服务端操作类
        /// </summary>
        private GrpcServer grpcServer;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="requestHandler">请求消息处理方法</param>
        /// <param name="port">监听端口</param>
        /// <param name="host">监听地址</param>
        /// <param name="options">设置项</param>
        public GrpcServerHelper(IGrpcMessageHandler messageHandler, int port, string host = "0.0.0.0", List<ChannelOption> options = null)
        {
            this.grpcServer = new GrpcServer(messageHandler, port, host, options);
        }

        public void Dispose()
        {
            if (this.grpcServer != null)
            {
                this.grpcServer.Dispose();
                this.grpcServer = null;
            }
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        public void Start()
        {
            this.grpcServer.Start();
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public async Task StopAsync()
        {
            await this.grpcServer.StopAsync();
        }
    }
}
