﻿using System;
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
            var client = new BrokerageClient("http://localhost:5031", true);
            var requestId = Guid.NewGuid().ToString();

            while (true)
            {
                try
                {
                    var sw = new Stopwatch();
                    sw.Start();
                    var result = await client.Monitoring.IsAliveAsync(new IsAliveRequest());

                    {
                        var tenantId = "cbb04866-eeb7-4c13-abc0-eae7c73bbc94";

                        var brokerAccount = await client.BrokerAccounts.CreateAsync(new CreateRequest
                        {
                            Name = "Broker_1",
                            RequestId = requestId,
                            TenantId = tenantId,
                            VaultId = 100007
                        });

                        var account = await client.Accounts.CreateAsync(new CreateAccountRequest
                        {
                            RequestId = requestId,
                            ReferenceId = "some ref",
                            BrokerAccountId = brokerAccount.Response.Id
                        });

                        //var account2 = await client.Accounts.CreateAsync(new CreateAccountRequest
                        //{
                        //    RequestId = requestId,
                        //    ReferenceId = "some ref",
                        //    BrokerAccountId = 1
                        //});

                        //var resultx = await client.Withdrawals.ExecuteAsync(new ExecuteWithdrawalRequest()
                        //{
                        //    BrokerAccountId = 999_999,
                        //    AssetId = 100_000,
                        //    Amount = 0.1m,
                        //    TenantId = "abel-tenant-100",
                        //    DestinationDetails = new DestinationDetails() {Address = "2N3PkwDpEUwdb2Fm58v4x4XZGcaeMX9h93b" },
                        //    RequestId = "requestId",
                        //});

                        //var serializedBrokerAccount = Newtonsoft.Json.JsonConvert.SerializeObject(brokerAccount);
                        //var serializedAccount = Newtonsoft.Json.JsonConvert.SerializeObject(account);
                        
                        //Console.WriteLine(serializedBrokerAccount);
                        //Console.WriteLine();
                        //Console.WriteLine(serializedAccount);
                    }

                    sw.Stop();
                    Console.WriteLine($"{result.Name} {sw.ElapsedMilliseconds} ms");
                    Thread.Sleep(100_000);
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
