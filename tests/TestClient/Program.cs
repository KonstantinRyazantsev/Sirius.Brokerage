using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Swisschain.Sirius.Brokerage.ApiClient;
using Swisschain.Sirius.Brokerage.ApiContract;

namespace TestClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Press enter to start");
            Console.ReadLine();
            var client = new BrokerageClient("http://localhost:5001", true);
            var requestId = Guid.NewGuid().ToString();

            while (true)
            {
                try
                {
                    var sw = new Stopwatch();
                    sw.Start();
                    var result = await client.Monitoring.IsAliveAsync(new IsAliveRequest());

                    {
                        var response = await client.BrokerAccount.CreateAsync(new CreateRequest()
                        {
                            BlockchainId = "Bitcoin",
                            Name = "Broker_1",
                            RequestId = requestId,
                            TenantId = "Tenant_1",
                            NetworkId = "RegTest"
                        });

                        var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(response);
                        Console.WriteLine(serialized);
                    }

                    {
                        var response = await client.BrokerAccount.CreateAsync(new CreateRequest()
                        {
                            BlockchainId = "Bitcoin",
                            Name = "Broker_1",
                            RequestId = requestId,
                            TenantId = "Tenant_1",
                            NetworkId = "RegTest2"
                        });

                        var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(response);
                        Console.WriteLine(serialized);
                    }

                    sw.Stop();
                    Console.WriteLine($"{result.Name}  {sw.ElapsedMilliseconds} ms");
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                Thread.Sleep(1000);
            }
        }
    }
}
