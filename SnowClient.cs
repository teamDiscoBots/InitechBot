using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Web;
using System.Collections.Specialized;
using Newtonsoft.Json;
using System.Collections.Generic;
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
    class SnowQueryTool {
        public static string getLumbergIncidents() {
            SnowQueryTool qt = new SnowQueryTool();
            SnowUser user = qt.findUser("billlumberg");

            StringBuilder sb = new StringBuilder("Number|Desc|Opened At <br>");
            if (user != null) {
                List<SnowIncident> incs = qt.findAssignedIncidents(user);
                if (incs==null || incs.Count<1){
                    sb.Append("No Incidents Found");
                } else {
                    foreach (SnowIncident inc in incs) {
                        sb.Append(inc.number)
                        .Append("|")
                        .Append(inc.short_decsription)
                        .Append("|")
                        .Append(inc.opened_at)
                        .Append("<br>");
                    }
                }
            } else {
                sb.Append("Error fetching SNow! incidents<br>");
            }
            return sb.ToString();
        }
        public SnowUser findUser(string userName) {
            if (userName ==null) {
                throw new MissingFieldException("must provide user name");
            }
            SnowClient client = new SnowClient("sys_user");
            client.Query="user_name=" +userName;
            client.ReturnFieldList="sys_id,user_name,email,name";
            Task<string> result = client.RunQuery();
            string json = extractJson(result);

            return JsonConvert.DeserializeObject<SnowUser>(json);
        }

        public List<SnowIncident> findAssignedIncidents(string user_sysid){
            if (user_sysid==null) {
                throw new MissingFieldException("Must provide system user id");
            }
            SnowClient client = new SnowClient("incident");
            client.Query="assigned_to="+user_sysid;
            client.ReturnFieldList=
                "number,sys_id,description,short_description,state,severity,"
                +"close_code,sys_created_on,opened_at,closed_at,closed_by,"
                +"assigned_to,caller,resolved_by,closed";
            Task<string> result = client.RunQuery();
            string json = extractJson(result);

            List<SnowIncident> incidents = JsonConvert.DeserializeObject<List<SnowIncident>>(json);
            return incidents;
        }

        public List<SnowIncident> findAssignedIncidents(SnowUser user) {
            return findAssignedIncidents(user.sys_id);
        }

        public List<SnowIncident> findAssignedIncidentsForUserId(string user_id) {
            return findAssignedIncidents(findUser(user_id).sys_id);
        }

        private string extractJson(Task<string> result) {
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
                return result.Result;
            }
            else if (result.IsFaulted)
            {
                throw result.Exception;
            }
            else
            {
                throw new TimeoutException("Failed to read back from SNOW! in timeout period!");
            }
        }
    }

    class SnowUser {
        private string _userName;
        private string _systemId;
        private string _email;
        private string _name;

        public string name { get => _name; set => _name = value; }
        public string user_id { get => _userName; set => _userName = value; }
        public string sys_id { get => _systemId; set => _systemId = value; }
        public string email { get => _email; set => _email = value; }

        public SnowUser(string userid,string FullName, string email, string sysid) {
            this._email=email;
            this._name=FullName;
            this._systemId=sysid;
            this._name=userid;
        }
    }

    class SnowIncident{
        private string _created;
        private string _resolved;
        private string _opened;
        private string _desc;
        private string _number;
        private string _severity;
        private string _state;
        private string _shortDesc;
        private string _closeCode;
        private string _assigned_to;
        private string _closed_by;
        private string _caller;
        private string _resolved_by;
        private string _sysId;
        private string _closed;
        public string sys_created_on { get => _created; set => _created = value; }
        public string resolved_at { get => _resolved; set => _resolved = value; }
        public string opened_at { get => _opened; set => _opened = value; }
        public string closed_at { get => _closed; set => _closed = value; }
        public string description { get => _desc; set => _desc = value; }
        public string number { get => _number; set => _number = value; }
        public string severity { get => _severity; set => _severity = value; }
        public string state { get => _state; set => _state = value; }
        public string short_decsription { get => _shortDesc; set => _shortDesc = value; }
        public string close_code { get => _closeCode; set => _closeCode = value; }
        public string assigned_to { get => _assigned_to; set => _assigned_to = value; }
        public string closed_by { get => _closed_by; set => _closed_by = value; }
        public string caller { get => _caller; set => _caller = value; }
        public string resolved_by { get => _resolved_by; set => _resolved_by = value; }
        public string sys_id { get => _sysId; set => _sysId = value; }
        public string closed { get => _closed; set => _closed = value; }

        public SnowIncident(){}
    }
    class SnowClient
    {
        private string tableName;
        private string query;
        private string returnFieldList;
        private const string snowBaseUrl = "https://tempdiscover.service-now.com/api/now/table/";
        private const string user = "HackathonTeam7Beta";
        private const string pw = "Delta3alpha";
        private bool excludeRefernceLink = true;

        public bool ExcludeReferenceLink {get => excludeRefernceLink; set => excludeRefernceLink=value;}
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
            if (excludeRefernceLink) {
                qstring["sysparm_exclude_reference_link"] = Boolean.TrueString;
            }
            return qstring;
        }
    }

}