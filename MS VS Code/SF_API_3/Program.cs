using System;


using System.Net.Http;
using System.Collections.Generic;
//add ref to Newtonsoft.Json.dll found in epicor folder
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


//https://stackoverflow.com/questions/9497228/is-there-a-c-sharp-wrapper-available-for-the-salesforce-rest-api
//using System.Text;
using System.IO;
//using System.Web;
using System.Net;
using System.Text.Json;
using System.Text;
//using Newtonsoft;
//using Newtonsoft.Json.Bson;
using System.Json;


// https://www.forcetalks.com/blog/integrate-net-console-application-with-salesforce-rest-api/
using System.Threading.Tasks;

//https://celedonpartners.com/blog/calling-a-custom-salesforce-rest-endpoint-using-c/
//using Newtonsoft.Json;
//using Salesforce.Common;
//using Salesforce.Force;
//using System;
//using System.Threading.Tasks;


namespace SF_API_3
{

    class Program
    {
        //error: DON'T USE THIS ONE, IS GENERIC
        //public const string LoginEndpoint = "https://test.salesforce.com/services/oauth2/token";

        //works better, but not actually loggin in:
        //public const string LoginEndpoint = "https://cdi-dev-ed.my.salesforce.com";
        //public const string LoginEndpoint = "https://cdi-dev-ed.my.salesforce.com/services/oauth2/token";
        //public const string LoginEndpoint = "https://eb1-dev-ed.my.salesforce.com/";
        //public const string LoginEndpoint = "https://eb1-dev-ed.lightning.force.com/";
        //400 authentication error:
        public const string LoginEndpoint = "https://eb1-dev-ed.my.salesforce.com/services/oauth2/token";
        //public const string LoginEndpoint = "https://eb1-dev-ed.lightning.force.com/";


        //public const string ApiEndpoint = "/services/data/v36.0/"; //Use your org's version number
        public const string ApiEndpoint = "/services/data/v51.0/"; //Use your org's version number

        static private string Username { get; set; }
        static private string Password { get; set; }
        static private string Token { get; set; }
        static private string ClientId { get; set; }
        static private string ClientSecret { get; set; }
        static public string AuthToken { get; set; }
        static public string ServiceUrl { get; set; }

        //static readonly HttpClient;
        static readonly HttpClient Client = new HttpClient();

        //static MyController()
        static void MyController()
        {
            //Client = new HttpClient();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

//            Login();

//zzz
            //https://dev.to/rachelsoderberg/integrating-c-net-and-salesforce-s-rest-api-d00
            Username = "frmartell3@gmail.com";
            Password = "Spider#8.3";
            Token = "VVrWKdSl9I9TJzL4bFvYmDdJ";
            ClientId = "3MVG9KsVczVNcM8wV1h42C3_gZKVYrH4JVYFw.PctBiGFGMp.bzzVIWB2wW.PK2tYBMDlwFd8FzAwJQ7nWkwm";
            ClientSecret = "FE27E7A8FA60FCBF36D6395557BD8622335A02C7C0EE97F73FF4A8F1BEE625A0";
            string PwdToken = Password + Token;

            HttpContent content = new FormUrlEncodedContent(new Dictionary<string, string>
              {
                  {"grant_type", "password"},
                  {"client_id", ClientId},
                  {"client_secret", ClientSecret},
                  {"username", Username},
                  {"password", PwdToken}
              });
            //{ "password", Password}

            //error with test.salesforce.com:
            //  400 bad request / invalid_grant / authentication failure
            HttpResponseMessage message = Client.PostAsync(LoginEndpoint, content).Result;

 //           HttpResponseMessage message2 = Client.PostAsync("https://eb1-dev-ed.my.salesforce.com/services/oauth2/token", "?grant_type=password&username=frmartell3@gmail.com&password=Spider#8.3").Result;
        

            //error with cdi-dev-ed.my.salesforce.com:
            //  401 unauthorized / session expired or invalid / INVALID_SESSION_ID
            //error with test.salesforce.com:
            //  400 bad request / destination url not reset. the url returned from login must be set
            //HttpResponseMessage message = Client.PostAsync(LoginEndpoint + ApiEndpoint, content).Result;


            string response = message.Content.ReadAsStringAsync().Result;

            //ERROR: 'Unexpected character encountered while parsing value: <. Path '', line 0, position 0.'
            JObject obj = JObject.Parse(response);


            AuthToken = (string)obj["access_token"];
            // >> "00D1U0000010xom!AQMAQJZKPjCuneGyOKh8HAV7GnYKpASbALVqfMvGMwVUUAK6U0MPzZShVDNdwiw.rBRZPo9j9YAJTahknyetSK.NMM3dGPbk"
            ServiceUrl = (string)obj["instance_url"];
            // >> "https://eb1-dev-ed.my.salesforce.com"

            // Get an sf record
            string companyName = "Acme North";
            //string companyName = "Acme";
            string queryMessage = $"SELECT Id, Name, Phone, Type FROM Account WHERE Name = '{companyName}'";
            //string queryMessage = $"SELECT Id, Name, Phone, Type FROM Account WHERE Name like \'%acme%\'";
            //string queryMessage = $"SELECT Id, Name, Phone, Type FROM Account WHERE Name LIKE \'%" + companyName + "%\'";
            //string queryMessage = $"SELECT Id, Name, Phone, Type FROM Account WHERE Name = 'Acme North' OR Name = 'Acme South'";
            //string queryMessage = $"SELECT Id, Name FROM Account WHERE Name = '{companyName}'";
            JObject obj2 = JObject.Parse(QueryRecord(Client, queryMessage));
            //!! need to parse out the json records here
            //obj2.to

            //using System.Text.Json;
            //using var doc = JsonDocument.Parse(QueryRecord(Client, queryMessage));
            //using var doc = JsonDocument.Parse(obj2.ToString());
            using var doc = JsonDocument.Parse(obj2["records"].ToString());

            string accountId = "";

            JsonElement root = doc.RootElement;
            var users = root.EnumerateArray();
            while (users.MoveNext())
            {
                var user = users.Current;
                System.Console.WriteLine(user);

                //string x = users.GetType().ToString();//  JsonTokenType

                var props = user.EnumerateObject();

                //loop through records:
                while (props.MoveNext())
                {
                    var prop = props.Current;
                    Console.WriteLine($"{prop.Name}: {prop.Value}");

                    if(prop.Name == "Id")
                    {
                        accountId = prop.Value.ToString();
                    }

                }
            }



            // Update a SF record
            string phone = "123-123-9999";
            string updateMessage = $"<root>" +
                $"<Phone>{phone}</Phone>" +
                $"</root>";
            string result = UpdateRecord(Client, updateMessage, "Account", accountId);
            if (result != "")
            {
                //logger.SalesforceError("Update", "Account");
                //return null;
            }
            //logger.SalesforceSuccess("Update", "Account", accountId);
            //return accountId;


            // Create an SF record
            string companyName2 = "ACME New";
            string phone2 = "123-456-7890";

            string createMessage = $"<root>" +
                $"<Name>{companyName2}</Name>" +
                $"<Phone>{phone2}</Phone>" +
                $"</root>";

            string result2 = CreateRecord(Client, createMessage, "Account");


        }

        public static string CreateRecord(HttpClient client, string createMessage, string recordType)
        {
            HttpContent contentCreate = new StringContent(createMessage, Encoding.UTF8, "application/xml");
            string uri = $"{ServiceUrl}{ApiEndpoint}sobjects/{recordType}";

            HttpRequestMessage requestCreate = new HttpRequestMessage(HttpMethod.Post, uri);
            requestCreate.Headers.Add("Authorization", "Bearer " + AuthToken);
            requestCreate.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/xml"));
            requestCreate.Content = contentCreate;

            HttpResponseMessage response = client.SendAsync(requestCreate).Result;
            return response.Content.ReadAsStringAsync().Result;
        }

        public static string UpdateRecord(HttpClient client, string updateMessage, string recordType, string recordId)
        {
            HttpContent contentUpdate = new StringContent(updateMessage, Encoding.UTF8, "application/xml");

            string uri = $"{ServiceUrl}{ApiEndpoint}sobjects/{recordType}/{recordId}?_HttpMethod=PATCH";

            HttpRequestMessage requestUpdate = new HttpRequestMessage(HttpMethod.Post, uri);
            requestUpdate.Headers.Add("Authorization", "Bearer " + AuthToken);
            requestUpdate.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/xml"));
            requestUpdate.Content = contentUpdate;

            HttpResponseMessage response = client.SendAsync(requestUpdate).Result;
            return response.Content.ReadAsStringAsync().Result;
        }

        public static string QueryRecord(HttpClient client, string queryMessage)
        {
            string restQuery = $"{ServiceUrl}{ApiEndpoint}query?q={queryMessage}";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, restQuery);
            request.Headers.Add("Authorization", "Bearer " + AuthToken);
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = client.SendAsync(request).Result;
            return response.Content.ReadAsStringAsync().Result;
        }

        //https://stackoverflow.com/questions/9497228/is-there-a-c-sharp-wrapper-available-for-the-salesforce-rest-api
        public static string Login()
        {
            string responseJsonString = string.Empty;


            /*
            //public const string LoginEndpoint = "https://cdi-dev-ed.my.salesforce.com";
            //public const string LoginEndpoint = "https://cdi-dev-ed.my.salesforce.com/services/oauth2/token";
            public const string LoginEndpoint = "https://eb1-dev-ed.my.salesforce.com/";
            //400 authentication error:
            //public const string LoginEndpoint = "https://eb1-dev-ed.my.salesforce.com/services/oauth2/token";
            //public const string LoginEndpoint = "https://eb1-dev-ed.lightning.force.com/";
            */
            //string LoginOAuthUrl = "https://eb1-dev-ed.my.salesforce.com";
            //string LoginOAuthUrl = "https://login.salesforce.com/services/oauth2/token";
            //string LoginOAuthUrl = "https://cdi-dev-ed.my.salesforce.com/";
            string LoginOAuthUrl = "https://eb1-dev-ed.lightning.force.com/";
        

            string ClientID = "3MVG9KsVczVNcM8wV1h42C3_gZKVYrH4JVYFw.PctBiGFGMp.bzzVIWB2wW.PK2tYBMDlwFd8FzAwJQ7nWkwm";
            ClientSecret = "FE27E7A8FA60FCBF36D6395557BD8622335A02C7C0EE97F73FF4A8F1BEE625A0";
                            
            string ClientUserName = "frmartell3@gmail.com";
            string ClientPassword = "Spider#8.3";
            //string ClientUserName = "eric.blackwelder2@compudata.com";
            //string ClientPassword = "c0mpudata321";

            StringBuilder str = new StringBuilder();
            str.AppendFormat("{0}?grant_type=password&client_id={1}&client_secret={2}&username={3}&password={4}"
                             , LoginOAuthUrl, ClientID, ClientSecret, ClientUserName, ClientPassword
                             //, "https://eb1-dev-ed.my.salesforce.com/",
                             //"3MVG9KsVczVNcM8wV1h42C3_gZKVYrH4JVYFw.PctBiGFGMp.bzzVIWB2wW.PK2tYBMDlwFd8FzAwJQ7nWkwm", 
                             //"FE27E7A8FA60FCBF36D6395557BD8622335A02C7C0EE97F73FF4A8F1BEE625A0",
                             //"frmartell3@gmail.com",
                             //"Spider#8.3"
                             );

            // Request token
            try
            {

                //https://dev.to/rachelsoderberg/querying-an-account-using-c-net-and-the-salesforce-rest-api-4o5k
                string companyName = "Acme North";
                string queryMessage = $"SELECT Id, Name FROM Account WHERE Name = '{companyName}'";
                //ServiceUrl = "https://cdi-dev-ed.my.salesforce.com";
                ServiceUrl = "https://eb1-dev-ed.lightning.force.com";
            
                string restQuery = $"{ServiceUrl}{ApiEndpoint}query?q={queryMessage}";

                HttpRequestMessage request2 = new HttpRequestMessage(HttpMethod.Get, restQuery);
                //NOTE: got AuthToken value by clicking generate token in the manage connected apps page for TestAPIConnect
                AuthToken = "6Cel800D1U0000010xom8881U000002CCrAmdj4L7RiwEw7kqbu5XE59um63YW3fKAKffZyN1jocY2N5y4Mto6VOiyQznKiVK8wXoWoFh9J";
                request2.Headers.Add("Authorization", "Bearer " + AuthToken);
                request2.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //public const string LoginEndpoint = "https://eb1-dev-ed.my.salesforce.com/services/oauth2/token";
                //public const string LoginEndpoint = "https://eb1-dev-ed.lightning.force.com/";
                //String myUrl = "https://eb1-dev-ed.my.salesforce.com/";
                //String myUrl = "https://eb1-dev-ed.my.salesforce.com";
                //String myUrl = "https://eb1-dev-ed.my.salesforce.com";
                String myUrl = "https://eb1-dev-ed.lightning.force.com/";

                Uri myURI = new Uri(myUrl);
                Client.BaseAddress = myURI;
                //error 401 unauthorized
                HttpResponseMessage response2 = Client.SendAsync(request2).Result;
                //return response2.Content.ReadAsStringAsync().Result;
                // >> 	"[{\"message\":\"Session expired or invalid\",\"errorCode\":\"INVALID_SESSION_ID\"}]"	string





                HttpWebRequest request = WebRequest.Create(str.ToString()) as HttpWebRequest;

                if (request != null)
                {
                    request.Method = "POST";
                    using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            // Get the "access_token" and "instance_url" from the response.
                            // Convert the JSON response into a token object


                            // Here we get the response as a stream.
                            using (StreamReader responseStream = new StreamReader(response.GetResponseStream()))
                            {
                                responseJsonString = responseStream.ReadToEnd();
                                // Deserialize JSON response into an Object.
                                JsonValue value = JsonValue.Parse(responseJsonString);
                                JsonObject responseObject = value as JsonObject;
                                string AccessToken = (string)responseObject["access_token"];
                                string  InstanceUrl = (string)responseObject["instance_url"];
                                return "We have an access token: " + AccessToken + "\n" + "Using instance " + InstanceUrl + "\n\n";
                            }
                        }
                    }
                }
                return responseJsonString;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Unable to login to salesforce: {0}", str), ex);
            }
        }

    }


}
