using Grpc.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;

namespace PK.GRPC.Utilities.Test.Server
{
    class Program
    {
        private static GrpcServerHelper grpcServerHelper = new GrpcServerHelper(new GrpcRequestMessageHandler(), 5000);
        static void Main(string[] args)
        {
            grpcServerHelper.Start();

            Console.ReadKey();

            grpcServerHelper.Dispose();
        }
    }

    /// <summary>
    /// 服务端请求消息处理者
    /// </summary>
    public class GrpcRequestMessageHandler : IGrpcMessageHandler
    {
        /// <summary>
        /// 简单模式处理方法
        /// </summary>
        /// <param name="requestData"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<string> Handle(string requestData, ServerCallContext context)
        {
            Console.WriteLine($"Client：{context.Peer}。简单模式接收数据：{requestData}");
            return requestData;
        }

        public async Task<byte[]> MultiHandle(byte[] requestData, ServerCallContext context)
        {
            string data = Encoding.UTF8.GetString(requestData);

            Console.WriteLine($"Client：{context.Peer}。双向流接收数据：{data}");

            return requestData;
        }
    }
}
