using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Swisschain.Sirius.Brokerage.ApiClient;
using Swisschain.Sirius.Brokerage.ApiContract;
using Swisschain.Sirius.Brokerage.ApiContract.Common;

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

            try
            {
                var sw = new Stopwatch();
                sw.Start();
                var result = await client.Monitoring.IsAliveAsync(new IsAliveRequest());

                {
                    var tenantId = "c06c3f79-7e34-44f9-aac9-94c6ef438f58";
                    {
                        var brokerAccount = await client.BrokerAccounts.CreateAsync(new CreateRequest
                        {
                            Name = "Broker_2",
                            RequestId = requestId,
                            TenantId = tenantId,
                            VaultId = 100002,
                            BlockchainIds =
                                {
                                    "bitcoin-private", "stellar-test"
                                }
                        });

                        var brokerAccountId = brokerAccount.Response.Id;

                        Console.WriteLine("Press enter to continue");
                        Console.ReadLine();

                        for (int i = 0; i < 15; i++)
                        {
                            var account = await client.Accounts.CreateAsync(new CreateAccountRequest
                            {
                                RequestId = requestId + i,
                                ReferenceId = "some ref" + i,
                                BrokerAccountId = brokerAccountId
                            });
                        }

                        Console.WriteLine("Press enter to continue");
                        Console.ReadLine();

                        var response = await client.BrokerAccounts.AddBlockchainAsync(new AddBlockchainRequest()
                        {
                            BrokerAccountId = brokerAccountId,
                            RequestId = requestId,
                            BlockchainIds = { "litecoin-private" },
                            TenantId = tenantId
                        });
                    }

                    //{
                    //    var brokerAccount = await client.BrokerAccounts.CreateAsync(new CreateRequest
                    //    {
                    //        Name = "Broker_1",
                    //        RequestId = requestId,
                    //        TenantId = tenantId,
                    //        VaultId = 100002,
                    //    });

                    //    var account = await client.Accounts.CreateAsync(new CreateAccountRequest
                    //    {
                    //        RequestId = requestId,
                    //        ReferenceId = "some ref",
                    //        BrokerAccountId = brokerAccount.Response.Id
                    //    });

                    //    var response = await client.BrokerAccounts.AddBlockchainAsync(new AddBlockchainRequest()
                    //    {
                    //        BrokerAccountId = brokerAccount.Response.Id,
                    //        RequestId = requestId,
                    //        BlockchainIds = { "litecoin-private", "bitcoin-private" },
                    //        TenantId = tenantId
                    //    });
                    //}

                    //var account2 = await client.Accounts.CreateAsync(new CreateAccountRequest
                    //{
                    //    RequestId = requestId,
                    //    ReferenceId = "some ref",
                    //    BrokerAccountId = 1
                    //});

                    //var resultч = await client.Withdrawals.ExecuteAsync(new ExecuteWithdrawalRequest()
                    //{
                    //    BrokerAccountId = 10000007,
                    //    AssetId = 100_000,
                    //    Amount = 0.1m,
                    //    TenantId = "c06c3f79-7e34-44f9-aac9-94c6ef438f58",
                    //    DestinationDetails = new DestinationDetails()
                    //    {
                    //        Address = "2N3PkwDpEUwdb2Fm58v4x4XZGcaeMX9h93b",
                    //        Tag = null,
                    //        TagType = new NullableDestinationTagType()
                    //        {
                    //            Null = NullValue.NullValue,
                    //        }
                    //    },
                    //    RequestId = "Api:c06c3f79-7e34-44f9-aac9-94c6ef438f58:jhk,uib",
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Thread.Sleep(1000);
        }
    }
}
