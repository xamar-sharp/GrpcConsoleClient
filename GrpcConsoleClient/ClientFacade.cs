using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Grpc.Net.Client;
using Grpc.Core.Interceptors;
using Grpc.Core;
using System.Diagnostics;
namespace GrpcConsoleClient
{
    public sealed class ClientFacade
    {
        private readonly GrpcChannel _channel;
        private readonly Telemetry.TelemetryClient _telemetryClient;
        private readonly Messaging.MessagingClient _messagingClient;
        /// <summary>
        /// Constructor for client facade
        /// </summary>
        /// <param name="uri">Https Uri to server with gRPC services!</param>
        public ClientFacade(string uri)
        {
            _channel = GrpcChannel.ForAddress(uri);
            var caller = _channel.Intercept(new ExampleClientInterceptor());
            _telemetryClient = new Telemetry.TelemetryClient(caller);
            _messagingClient = new Messaging.MessagingClient(caller);
        }
        public async Task RequestServerUsage()
        {
            Console.WriteLine(await _telemetryClient.RequestServerUsageAsync(new RequestOptions()));
        }
        public async Task RequestClientUsageDelta()
        {
            using var call = _telemetryClient.ComputeClientUsageDelta();
            for (int x = 0; x < 5; x++)
            {
                await call.RequestStream.WriteAsync(new MemoryInfo() { PhysicalMemory = Process.GetCurrentProcess().WorkingSet64, VirtualMemory = Process.GetCurrentProcess().VirtualMemorySize64 });
                await Task.Delay(2000);
            }
            await call.RequestStream.CompleteAsync();
            var result = await call;
            Console.WriteLine($"Deltas:P={result.PhysicalMemoryDelta}\nV={result.VirtualMemoryDelta}\n");
        }
        public async Task StartTelephony()
        {
            using var call = _messagingClient.Speak();
            var task = Task.Run(async () =>
            {
                while (await call.ResponseStream.MoveNext())
                {
                    Console.WriteLine(call.ResponseStream.Current.Text);
                }
            });
            for (int x = 0; x < 3; x++)
            {
                await call.RequestStream.WriteAsync(new ClientMessage() { Text = $"Client: {x}" });
            }
            await call.RequestStream.CompleteAsync();
            await task;
        }
        public async Task ReadFileFromServer()
        {
            using var call = _messagingClient.LoadFile(new FileAbout() { FileName = "tool.png" });
            List<byte> buckets = new List<byte>(2);
            await foreach (var bucket in call.ResponseStream.ReadAllAsync())
            {
                buckets.AddRange(bucket.Bucket.Span.ToArray());
            }
            await File.WriteAllBytesAsync($"C:\\COPY_{DateTime.Now.Minute}", buckets.ToArray());
        }
    }
}
