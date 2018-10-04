using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Web;
using System.Collections.Specialized;
/**
  Query Service Now via the table API.
  Currently hard codes the creds, this probably works

  Provide a table name to this guy and then the query filter of the form 
  name=value,[name=value] to filter records and then provide the return fields
  you want as a comma-sep string.

Here is the wrapped console app code to show how it was invoked and result read:

 static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("SnowClientTest tableName <query> <fieldList>");
                Environment.Exit(0);
            }
            string tableName = args[0];

            string query = null;
            if (args.Length > 1)
            {
                query = args[1];
            }

            string fieldList = null;
            if (args.Length > 2)
            {
                fieldList = args[2];
            }

            SnowClient snow = new SnowClient(tableName);
            snow.Query = query;
            snow.ReturnFieldList = fieldList;

            Task<string> result = snow.RunQuery();
            int time = 0;
            while (!result.IsCompleted 
                    && !result.IsFaulted
                    && !result.IsCanceled
                    && time < 10000)
            {
                Thread.Sleep(500);
                time += 500;
            }

            if (result.IsCompleted && !result.IsFaulted)
            {
                Console.WriteLine("Result returned:");
                Console.WriteLine(result.Result);
                Environment.Exit(0);
            } else if (result.IsFaulted) {
                Console.WriteLine("Fault returned:");
                Exception e = result.Exception;
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Environment.Exit(2);
            }
            else
            {
                Console.WriteLine("Request failed or timed out");
                Environment.Exit(1);
            }
        }
    }

 */
namespace Snow
{
    class SnowClient
    {
        private string tableName;
        private string query;
        private string returnFieldList;
        private const string snowBaseUrl = "https://tempdiscover.service-now.com/api/now/table/";
        private const string user = "HackathonTeam7Beta";
        private const string pw = "Delta3alpha";

        public string ReturnFieldList { get => returnFieldList; set => returnFieldList = value; }
        public string Query { get => query; set => query = value; }

        public SnowClient(string tableName)
        {
            this.tableName = tableName;
            this.Query = Query;
        }

        public async Task<string> RunQuery()
        {
            if (tableName == null)
            {
                throw new ArgumentNullException("Missing table name to query");
            }

            HttpClient curl = new HttpClient();

            byte[] authBytes = Encoding.ASCII.GetBytes(user + ":" + pw);
            curl.DefaultRequestHeaders.Authorization
                = new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Basic", Convert.ToBase64String(authBytes)
                    );
            string contextPath = tableName;

            Uri snowUri = buildSnowUri();
            HttpResponseMessage response = await curl.GetAsync(snowUri.ToString());
            if (response.IsSuccessStatusCode)
            {
                HttpContent content = response.Content;
                return content.ReadAsStringAsync().Result;
            }
            else
            {
                return response.StatusCode.ToString();
            }
        }

        private Uri buildSnowUri()
        {
            UriBuilder u = new UriBuilder(snowBaseUrl);
            u.Path += tableName;
            NameValueCollection qsParms = this.asQueryString();
            u.Query = qsParms.ToString();

            return u.Uri;
        }

        private NameValueCollection asQueryString()
        {
            NameValueCollection qstring = HttpUtility.ParseQueryString("");
            if (this.ReturnFieldList != null && !this.ReturnFieldList.Equals(""))
            {
                qstring["sysparm_fields"] = this.ReturnFieldList;
            }
            if (this.Query != null && !this.Query.Equals(""))
            {
                qstring["sysparm_query"] = this.Query;
            }
            return qstring;
        }
    }

}