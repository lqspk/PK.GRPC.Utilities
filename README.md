# PK.GRPC.Utilities
简单封装的Grpc通用操作类库，为了更方便的使用它。



**服务端简单示例：**

```c#
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
```





**客户端简单示例：**

```c#
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
```

