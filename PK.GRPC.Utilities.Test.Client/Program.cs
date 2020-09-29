using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace PK.GRPC.Utilities.Test.Client
{
    class Program
    {
        private static GrpcClientFactoryHelper grpcClientFactoryHelper = new GrpcClientFactoryHelper("127.0.0.1:5000");

        static void Main(string[] args)
        {
            var data = JObject.FromObject(new {@data = "测试简单模式"}).ToString();
            var client = grpcClientFactoryHelper.Create();
            var response = client.SendString(data);
            Console.WriteLine(response);

            Console.ReadKey();


            //双向流测试
            var bytes = Encoding.UTF8.GetBytes("START");
            client = grpcClientFactoryHelper.Create(new GrpcMultiMessageHandler());
            Console.WriteLine($"multi start at {DateTime.Now.ToString($"yyyy-MM-dd HH:mm:ss.fff")}");
            client.StartMultiStreamAsync(bytes).Wait();
            Console.WriteLine($"multi end at {DateTime.Now.ToString($"yyyy-MM-dd HH:mm:ss.fff")}");


            Console.ReadKey();

            grpcClientFactoryHelper.Dispose();
        }
    }

    /// <summary>
    /// 服务端请求消息处理者
    /// </summary>
    public class GrpcMultiMessageHandler : IGrpcMultiMessageHandler
    {
        private int receiveCount = 0;
        public async Task<Tuple<byte[], bool>> MultiHandle(byte[] responseData, Metadata responseHeaders)
        {
            string data = Encoding.UTF8.GetString(responseData);
            Console.WriteLine($"双向流接收数据：{data}");

            receiveCount++;

            if (data == "END")
            {
                return new Tuple<byte[], bool>(null, true);
            }

            if (receiveCount == 100)
            {
                return new Tuple<byte[], bool>(Encoding.UTF8.GetBytes("END"), false);
            }

            return new Tuple<byte[], bool>(Encoding.UTF8.GetBytes($"{receiveCount}"), false);
        }
    }
}
