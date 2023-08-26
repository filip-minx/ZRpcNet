using System;
using NetMQ;

namespace Minx.zRPC.NET
{
    public static class NetMQFrameExtensions
    {
        public static string ConvertToBase64String(this NetMQFrame frame)
        {
            return Convert.ToBase64String(frame.Buffer);
        }
    }
}
