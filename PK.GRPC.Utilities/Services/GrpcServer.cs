using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PK.GRPC.Utilities
{
    /// <summary>
    /// Grpc服务端操作类
    /// </summary>
    internal sealed class GrpcServer : IDisposable
    {
        /// <summary>
        /// Grpc服务端
        /// </summary>
        private Server grpcServer;

        /// <summary>
        /// Grpc服务端消息处理者
        /// </summary>
        private GrpcServiceMessageHandler serviceMessageHandler;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="requestHandler">请求消息处理方法</param>
        /// <param name="port">监听端口</param>
        /// <param name="host">监听地址</param>
        /// <param name="options">设置项，如果为空，则自动设置最大发送和接收数据大小为2147483647字节</param>
        internal GrpcServer(IGrpcMessageHandler messageHandler, int port, string host = "0.0.0.0", List<ChannelOption> options = null)
        {
            if (messageHandler == null)
                throw new Exception("GrpcMessageHandler is null");

            this.serviceMessageHandler = new GrpcServiceMessageHandler(messageHandler);

            if (options == null)
            {
                options = new List<ChannelOption>();
                options.Add(new ChannelOption(ChannelOptions.MaxSendMessageLength, int.MaxValue));
                options.Add(new ChannelOption(ChannelOptions.MaxReceiveMessageLength, int.MaxValue));
            }

            this.grpcServer = new Server(options)
            {
                Services = { GrpcService.BindService(this.serviceMessageHandler) },
                Ports = { new ServerPort(host, port, ServerCredentials.Insecure) }
            };
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        internal void Start()
        {
            this.grpcServer.Start();
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        internal async Task StopAsync()
        {
            if (this.grpcServer != null)
            {
                await this.grpcServer.ShutdownAsync();
            }
        }

        public void Dispose()
        {
            if (this.grpcServer != null)
            {
                StopAsync().Wait();
                this.grpcServer = null;

                this.serviceMessageHandler.Dispose();
                this.serviceMessageHandler = null;
            }
        }
    }

    /// <summary>
    /// Grpc服务端服务消息处理类
    /// </summary>
    internal sealed class GrpcServiceMessageHandler : GrpcService.GrpcServiceBase, IDisposable
    {
        /// <summary>
        /// 处理请求消息服务
        /// </summary>
        private readonly IGrpcMessageHandler messageHandler;

        internal GrpcServiceMessageHandler(IGrpcMessageHandler messageHandler)
        {
            this.messageHandler = messageHandler;
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="request">请求数据</param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<StringData> SendString(StringData request, ServerCallContext context)
        {
            var result = await this.messageHandler.Handle(request.Data, context);

            return await Task.FromResult(new StringData()
            {
                Data = result
            });
        }

        /// <summary>
        /// 双向流
        /// </summary>
        /// <param name="requestStream"></param>
        /// <param name="responseStream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task MultiStream(IAsyncStreamReader<RequestStreamData> requestStream, IServerStreamWriter<ResponseStreamData> responseStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var requestContent = requestStream.Current;

                var response = await this.messageHandler.MultiHandle(requestContent.Data.ToByteArray(), context);

                await responseStream.WriteAsync(new ResponseStreamData()
                {
                    Data = ByteString.CopyFrom(response)
                });
            }
        }

        public void Dispose()
        {

        }
    }
}
