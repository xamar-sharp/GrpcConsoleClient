using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core.Interceptors;
using Grpc.Core;
namespace GrpcConsoleClient
{
    public sealed class ExampleClientInterceptor : Interceptor
    {
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest,TResponse>(TRequest request,ClientInterceptorContext<TRequest,TResponse> ctx, AsyncUnaryCallContinuation<TRequest,TResponse> continuation)
        {
            Console.WriteLine($"Client interceptor invoked with {ctx.Method.Name}!");
            return continuation(request, ctx);
        }
    }
}
