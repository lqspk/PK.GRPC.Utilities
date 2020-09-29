using Grpc.Core;
using System.Threading.Tasks;

namespace PK.GRPC.Utilities
{
    /// <summary>
    /// Grpc消息处理接口
    /// </summary>
    public interface IGrpcMessageHandler
    {
        /// <summary>
        /// 简单模式处理方法
        /// </summary>
        /// <param name="requestData">请求数据</param>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<string> Handle(string requestData, ServerCallContext context);

        /// <summary>
        /// 双向流处理方法
        /// </summary>
        /// <param name="requestData">请求数据</param>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<byte[]> MultiHandle(byte[] requestData, ServerCallContext context);
    }
}
