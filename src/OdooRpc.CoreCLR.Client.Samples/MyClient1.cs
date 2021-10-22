using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        public async Task GetCustomers(IEnumerable<long> ids)
        {
            try
            {
                var searchParams = new OdooGetParameters("res.partner", ids);

                var departments = await this.OdooRpcClient.Get<JObject[]>(searchParams);

                Array.ForEach<JObject>(departments, Console.WriteLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting customers from Odoo: {0}", ex.Message);
            }
        }

        public async Task GetInvoiceFull(long id)
        {
            try
            {
                var searchParams = new OdooGetParameters("account.move", new long[]{id});

                var invoices = await this.OdooRpcClient.Get<JObject[]>(searchParams);
                foreach (var inv in invoices)
                {
                    System.Console.WriteLine(inv);
                    var ids = (JArray)inv["line_ids"];
                    var lineIds = ids.Select(l => (long)l).ToList();
                    var invLines = await this.OdooRpcClient.Get<JObject[]>(new OdooGetParameters("account.move.line", lineIds));
                    foreach (var line in invLines)
                    {
                        System.Console.WriteLine(line);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting invoices from Odoo: {0}", ex.Message);
            }
        }

        public async Task CreateInvoice(string customerName, string reason, decimal amount)
        {
            long createdId = 0;
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
                        string searchCountry = "bulgaria";
                        var matchingCountries = await this.OdooRpcClient.Search<long[]>(new OdooSearchParameters
                        (
                            "res.country",
                            new OdooDomainFilter().Filter("name", "ilike", searchCountry)
                        ));

                        long countryId = matchingCountries.FirstOrDefault();
                        if (countryId <= 0)
                        {
                            throw new ArgumentException("Cannot find country " + searchCountry);
                        }
                        // TODO: create customer
                        customerId = await this.OdooRpcClient.Create<dynamic>("res.partner", new {
                            name = customerName,
                            street = "Made-up address 1",
                            city = "Vratza",
                            country_id = countryId,
                            email = "madeupemail1@abv.bg",
                            x_nickname = $"{customerName} accountId"
                        });
                    }
                    
                    Console.WriteLine("Creating invoice for customer " + customerId);
                }

                // create invoice
                createdId = await this.OdooRpcClient.Create<dynamic>("account.move", new
                {
                    partner_id = customerId,
                    move_type = "out_invoice",
                    x_external_creator = "portal Admin3"
                });

                // create invoice lines
                // var salesAccounts = await this.OdooRpcClient.Search<long[]>(new OdooSearchParameters
                // (
                //     "account.account",
                //     new OdooDomainFilter().Filter("code", "=", 700000)
                // ));
                // long salesAccId = salesAccounts.FirstOrDefault();
                long salesAccId = 1;
                long receivablesAccId = 2;  // TODO: get from response

                var createLines = new List<dynamic>(){
                    new 
                    {
                        move_id = createdId,
                        name = reason,
                        quantity = 1.0,
                        price_unit = amount,
                        credit = amount,
                        partner_id = customerId,
                        //tax_ids = new object[]{new object[]{6, false, new object[]{1}}},
                        account_id = salesAccId  // 700000 - Sales
                    },
                    new 
                    {
                        move_id = createdId,
                        name = reason,
                        quantity = 1.0,
                        debit = 1.0m * amount,
                        exclude_from_invoice_tab = true,
                        partner_id = customerId,
                        //tax_ids = new object[]{new object[]{6, false, new object[]{}}},
                        //tax_tag_ids = new object[]{new object[]{6, false, new object[]{}}},
                        account_id = receivablesAccId  // 410000 - Receivables
                    },
                    // new 
                    // {
                    //     move_id = createdId,
                    //     name = "VAT 20%",
                    //     quantity = 1.0,
                    //     credit = 0.2m * amount,
                    //     tax_base_amount = amount,
                    //     exclude_from_invoice_tab = true,
                    //     partner_id = customerId,
                    //     tax_ids = new object[]{new object[]{6, false, new object[]{}}},
                    //     tax_repartition_line_id = 2,
                    //     account_id = 13  // VAT
                    // }
                };
                await this.OdooRpcClient.CreateMulti<List<dynamic>>("account.move.line", createLines); 
                // var salesLineId = await this.OdooRpcClient.Create<dynamic>("account.move.line", );
                // var receivesLineId = await this.OdooRpcClient.Create<dynamic>("account.move.line", );

                Console.WriteLine("Created invoice " + createdId);

                // await this.OdooRpcClient.Delete("account.move", 2);
            }
            catch (RpcCallException ex)
            {
                Console.WriteLine("Error creating invoice: {0}", ex.Message);
                var valueErr = Regex.Match(JsonConvert.SerializeObject(ex.RpcErrorData), "message\":.*"); 
                // System.Console.WriteLine(JsonConvert.SerializeObject(ex.RpcErrorData));
                Console.WriteLine(valueErr);

                if (createdId > 0)
                {
                    await this.OdooRpcClient.Delete("account.move", createdId);
                }
            }
        }
    }
}