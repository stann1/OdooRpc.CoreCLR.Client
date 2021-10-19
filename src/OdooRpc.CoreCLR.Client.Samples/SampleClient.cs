using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OdooRpc.CoreCLR.Client;
using OdooRpc.CoreCLR.Client.Interfaces;
using OdooRpc.CoreCLR.Client.Models;
using OdooRpc.CoreCLR.Client.Models.Parameters;

namespace OdooRpc.CoreCLR.Client.Samples
{
    public class SampleClient
    {
        public OdooConnectionInfo ConnectionInfo { get; private set; }
        public IOdooRpcClient OdooRpcClient { get; private set; }

        public SampleClient(OdooConnectionInfo connectionInfo)
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

        public async Task GetAllDepartments()
        {
            try
            {
                var fieldParams = new OdooFieldParameters();
                fieldParams.Add("name");
                fieldParams.Add("company_id");
                fieldParams.Add("color");

                var departments = await this.OdooRpcClient.GetAll<JObject[]>("hr.department", fieldParams, new OdooPaginationParameters().OrderByDescending("name"));

                Console.WriteLine(departments.FirstOrDefault());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting departments from Odoo: {0}", ex.Message);
            }
        }

        public async Task GetDepartments()
        {
            try
            {
                var reqParams = new OdooGetParameters("hr.department");
                reqParams.Ids.Add(6);
                //reqParams.Ids.Add(7);

                var fieldParams = new OdooFieldParameters();
                fieldParams.Add("name");
                fieldParams.Add("company_id");

                var departments = await this.OdooRpcClient.Get<JObject[]>(reqParams, fieldParams);

                Console.WriteLine(departments.FirstOrDefault());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting departments from Odoo: {0}", ex.Message);
            }
        }

        public async Task SearchDepartments()
        {
            try
            {
                var reqParams = new OdooSearchParameters(
                    "hr.department", 
                    new OdooDomainFilter().Filter("name", "like", "SIC")
                );

                var count = await this.OdooRpcClient.SearchCount(reqParams);
                var departments = await this.OdooRpcClient.Search<long[]>(reqParams, new OdooPaginationParameters(0, 1));

                Console.WriteLine(departments.FirstOrDefault());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting departments from Odoo: {0}", ex.Message);
            }
        }

        public async Task GetDepartmentsFields()
        {
            try
            {
                var reqParams = new OdooGetModelFieldsParameters(
                    "hr.department"
                );

                var fields = await this.OdooRpcClient.GetModelFields<dynamic>(reqParams);

                fields.ToList().ForEach(f => Console.WriteLine(f));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting partners from Odoo: {0}", ex.Message);
            }
        }

        public async Task CreateDeleteDepartment()
        {
            try
            {
                var id = await this.OdooRpcClient.Create<dynamic>("hr.department", new
                {
                    name = "test"
                });

                Console.WriteLine(id);

                await this.OdooRpcClient.Delete("hr.department", id);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting partners from Odoo: {0}", ex.Message);
            }
        }

        public async Task GetMetadata()
        {
            try
            {
                var metaParams = new OdooMetadataParameters("res.groups", new System.Collections.Generic.List<long>() { 4 });

                var resp = await this.OdooRpcClient.GetMetadata(metaParams);

                Console.WriteLine(resp.FirstOrDefault().ExternalId);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting metadata from Odoo: {0}", ex.Message);
            }
        }

    }
}