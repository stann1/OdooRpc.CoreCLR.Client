using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OdooRpc.CoreCLR.Client.Interfaces;
using OdooRpc.CoreCLR.Client.Models;
using OdooRpc.CoreCLR.Client.Models.Parameters;

namespace OdooRpc.CoreCLR.Client.Samples
{
    public class MyClient1
    {
        public OdooConnectionInfo ConnectionInfo { get; private set; }
        public IOdooRpcClient OdooRpcClient { get; private set; }

        public MyClient1(OdooConnectionInfo connectionInfo)
        {
            ConnectionInfo = connectionInfo;
        }

        // needs to be called before any other method
        public async Task LoginToOdoo()
        {
            try
            {
                this.OdooRpcClient = new OdooRpcClient(this.ConnectionInfo);

                var odooVersion = await this.OdooRpcClient.GetOdooVersion();

                Console.WriteLine("Odoo Version: {0} - {1}", odooVersion.ServerVersion, odooVersion.ProtocolVersion);

                await this.OdooRpcClient.Authenticate();

                if (this.OdooRpcClient.SessionInfo.IsLoggedIn)
                {
                    Console.WriteLine("Login successful => User Id: {0}", this.OdooRpcClient.SessionInfo.UserId);
                }
                else
                {
                    Console.WriteLine("Login failed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error connecting to Odoo: {0}", ex.Message);
            }
        }

        public async Task GetAllInvoices()
        {
            try
            {
                var fieldParams = new OdooFieldParameters
                {
                    "id",
                    "name",
                    "partner_id",
                    "company_id",
                    "move_type",
                    "state"
                };

                var departments = await this.OdooRpcClient.GetAll<JObject[]>("account.move", fieldParams, new OdooPaginationParameters().OrderByDescending("name"));

                Array.ForEach<JObject>(departments, Console.WriteLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting invoices from Odoo: {0}", ex.Message);
            }
        }

        public async Task CreateInvoice(string customerName = null)
        {
            try
            {
                long customerId = 0;
                if (customerName != null)
                {
                    var reqParams = new OdooSearchParameters(
                        "res.partner", 
                        new OdooDomainFilter().Filter("name", "like", customerName)
                    );

                    var count = await this.OdooRpcClient.SearchCount(reqParams);
                    var customers = await this.OdooRpcClient.Search<long[]>(reqParams, new OdooPaginationParameters(0, 1));

                    var foundCustomer = customers.Any() ? customers[0] : 0;
                    
                    if (foundCustomer > 0)
                    {
                        customerId = foundCustomer;
                    }
                    else
                    {
                        // TODO: create customer
                    }
                    
                    Console.WriteLine(foundCustomer);
                }

                var id = await this.OdooRpcClient.Create<dynamic>("account.move", new
                {
                    partner_id = customerId,
                    move_type = "out_invoice"
                });

                Console.WriteLine("Created " + id);

                // await this.OdooRpcClient.Delete("account.move", 2);
            }
            catch (RpcCallException ex)
            {
                Console.WriteLine("Error creating invoice: {0}", ex.Message);
                Console.WriteLine(JsonConvert.SerializeObject(ex.RpcErrorData));
            }
        }
    }
}