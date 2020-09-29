using System;
using Grpc.Core;
using System.Threading;
using System.Threading.Tasks;

namespace PK.GRPC.Utilities
{
    public interface IGrpcMultiMessageHandler
    {
        /// <summary>
        /// 双向流处理方法
        /// </summary>
        /// <param name="responseData">返回数据</param>
        /// <param name="responseHeaders">返回头</param>
        /// <param name="cancellationToken">通知取消</param>
        /// <returns>Item1：返回数据；Item2：是否完成推送消息</returns>
        Task<Tuple<byte[], bool>> MultiHandle(byte[] responseData, Metadata responseHeaders);
    }
}
