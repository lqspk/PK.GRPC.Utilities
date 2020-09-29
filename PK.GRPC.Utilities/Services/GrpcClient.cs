using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;

namespace PK.GRPC.Utilities
{
    /// <summary>
    /// Grpc客户端操作类
    /// </summary>
    public sealed class GrpcClient : IDisposable
    {
        /// <summary>
        /// Grpc客户端
        /// </summary>
        private GrpcService.GrpcServiceClient client;

        private readonly IGrpcMultiMessageHandler multiMessageHandler;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="client"></param>
        internal GrpcClient(GrpcService.GrpcServiceClient client, IGrpcMultiMessageHandler multiMessageHandler = null)
        {
            this.client = client;
            this.multiMessageHandler = multiMessageHandler;
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="headers">请求头</param>
        /// <param name="deadline">超时时间</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public string SendString(string data, Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = client.SendString(new StringData()
            {
                Data = data
            }, headers, deadline, cancellationToken);

            return response.Data;
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="options">设置项</param>
        /// <returns></returns>
        public string SendString(string data, CallOptions options)
        {
            var response = client.SendString(new StringData()
            {
                Data = data
            }, options);

            return response.Data;
        }

        /// <summary>
        /// 开始双向流传输
        /// </summary>
        /// <param name="data">触发数据</param>
        /// <param name="headers"></param>
        /// <param name="deadline"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartMultiStreamAsync(
            byte[] data = null,
            Metadata headers = null, 
            DateTime? deadline = null, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (this.multiMessageHandler == null)
            {
                throw new Exception("MultiMessageHandler is null");
            }

            using (var call = client.MultiStream(headers, deadline, cancellationToken))
            {
                if (data != null && data.Length > 0)
                {
                    await call.RequestStream.WriteAsync(new RequestStreamData()
                    {
                        Data = ByteString.CopyFrom(data)
                    });
                }

                while (await call.ResponseStream.MoveNext())
                {
                    var note = call.ResponseStream.Current;

                    var result = await this.multiMessageHandler.MultiHandle(note.Data.ToByteArray(), await call.ResponseHeadersAsync);

                    if (result.Item1 != null && result.Item1.Length > 0)
                    {
                        await call.RequestStream.WriteAsync(new RequestStreamData()
                        {
                            Data = ByteString.CopyFrom(result.Item1)
                        });
                    }

                    if (result.Item2 == true)
                        break;
                }

                await call.RequestStream.CompleteAsync();
            }
        }

        /// <summary>
        /// 异步发送数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="headers">请求头</param>
        /// <param name="deadline">超时时间</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<string> SendStringAsync(string data, Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = await client.SendStringAsync(new StringData()
            {
                Data = data
            }, headers, deadline, cancellationToken);

            return response.Data;
        }

        /// <summary>
        /// 异步发送数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="options">设置项</param>
        /// <returns></returns>
        public async Task<string> SendStringAsync(string data, CallOptions options)
        {
            var response = await client.SendStringAsync(new StringData()
            {
                Data = data
            }, options);

            return response.Data;
        }

        public void Dispose()
        {
            this.client = null;
        }
    }
}
