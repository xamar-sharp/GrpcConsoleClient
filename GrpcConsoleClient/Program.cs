using System;
using Grpc.Net.Client;
namespace GrpcConsoleClient
{
    internal class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            ClientFacade facade = new ClientFacade("https://localhost:5001/");
            await facade.ReadFileFromServer();
            await facade.RequestClientUsageDelta();
            await facade.RequestServerUsage();
            await facade.StartTelephony();
        }
    }
}
