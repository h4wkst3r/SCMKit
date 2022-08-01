using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace SCMKit.modules.gitlab
{

    // custom class to handle the JSON output of API token permissions
    public class apiPermissions
    {
        public string id { get; set; }
        public string name { get; set; }
        public bool revoked { get; set; }
        public string createdAt { get; set; }
        public string[] scopes { get; set; }
        public string user_id { get; set; }
        public bool active { get; set; }
        public string expiresAt { get; set; }

        public apiPermissions()
        {

        }


    }


    class Privs
    {

        public static async Task execute(string credential, string url, string options, string system)
        {

            Dictionary<string, string> valuePairs = new Dictionary<string, string>();
            valuePairs.Add("api", "Read-write for the complete API, including all groups and projects, the Container Registry, and the Package Registry.");
            valuePairs.Add("read_user", "Read-only for endpoints under /users. Essentially, access to any of the GET requests in the Users API.");
            valuePairs.Add("read_api", "Read-only for the complete API, including all groups and projects, the Container Registry, and the Package Registry.");
            valuePairs.Add("read_repository", "	Read-only (pull) for the repository through git clone.");
            valuePairs.Add("write_repository", "Read-write (pull, push) for the repository through git clone. Required for accessing Git repositories over HTTP when 2FA is enabled.");
            valuePairs.Add("read_registry", "Read-only (pull) for Container Registry images if a project is private and authorization is required.");
            valuePairs.Add("write_registry", "Read-write (push) for Container Registry images if a project is private and authorization is required.");
            valuePairs.Add("sudo", "API actions as any user in the system (if the authenticated user is an administrator).");

            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("privs", credential, url, options, system));
            List<String> listOfPermissions = new List<String>();

            // if username/password auth being used
            if (credential.Contains(":"))
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Privs module only supports API key authentication to determine privs of the API key given.");
                Console.WriteLine("");
            }

            // if token auth being used
            else
            {

                try
                {
                    ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    await library.Utils.HeartbeatRequest(url);


                    // get the users id to be passed in subsequent request
                    string userId = "";
                    var webRequestUserID = (HttpWebRequest)System.Net.WebRequest.Create(url + "/api/v4/user");
                    if (webRequestUserID != null)
                    {
                        // set header values
                        webRequestUserID.Method = "GET";
                        webRequestUserID.ContentType = "application/json";
                        webRequestUserID.Headers.Add("PRIVATE-TOKEN", credential);
                        webRequestUserID.UserAgent = "SCMKIT-5dc493ada400c79dd318abbe770dac7c";

                        // get web response
                        WebResponse myWebResponseUserID = await webRequestUserID.GetResponseAsync();
                        string contentUserID;
                        var readerUserID = new StreamReader(myWebResponseUserID.GetResponseStream());
                        contentUserID = readerUserID.ReadToEnd();

                        int startIndex = contentUserID.IndexOf("\"id\":");
                        int endIndex = contentUserID.IndexOf("\"name\":");
                        userId = contentUserID.Substring(startIndex + "\"id\":".Length, endIndex - startIndex - "\"name\"".Length);
                        myWebResponseUserID.Close();

                    }


                    // create table header
                    string tableHeader = string.Format("{0,20} | {1,10} | {2,20} | {3,70}", "Token Name", "Active?", "Privilege", "Description");
                    Console.WriteLine(tableHeader);
                    Console.WriteLine(new String('-', tableHeader.Length));


                    // get personal access tokens for user
                    var webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/api/v4/personal_access_tokens");
                    if (webRequest != null)
                    {
                        // set header values
                        webRequest.Method = "GET";
                        webRequest.ContentType = "application/json";
                        webRequest.Headers.Add("PRIVATE-TOKEN", credential);
                        webRequest.UserAgent = "SCMKIT-5dc493ada400c79dd318abbe770dac7c";

                        // get web response
                        WebResponse myWebResponse = await webRequest.GetResponseAsync();
                        string content;
                        var reader = new StreamReader(myWebResponse.GetResponseStream());
                        content = reader.ReadToEnd();

                        // if more than 1 page, we need to track what next URL will be
                        string nextURL = "";


                        // parse the JSON output and display results
                        List<apiPermissions> apiPerms = JsonConvert.DeserializeObject<List<apiPermissions>>(content);

                        foreach (apiPermissions item in apiPerms)
                        {
                            string keyName = item.name;
                            string uid = item.user_id;


                            // if user is not filtering and just wants to see there tokens
                            if (options.Equals("") && uid.Equals(userId))
                            {

                                if (uid.Equals(userId))
                                {
                                    string[] theScopes = item.scopes;
                                    for (int i = 0; i < theScopes.Length; i++)
                                    {
                                        string theKeyScope = theScopes[i];

                                        // only display an active PAT
                                        if (item.active == true)
                                        {

                                            Console.WriteLine("{0,20} | {1,10} | {2,20} | {3,70}", item.name, item.active, theKeyScope, valuePairs[theKeyScope]);
                                        }
                                    }
                                }
                            }

                            // if user is filtering on specific PAT
                            if (!options.Equals(""))
                            {

                                string[] theScopes = item.scopes;
                                for (int i = 0; i < theScopes.Length; i++)
                                {
                                    string theKeyScope = theScopes[i];

                                    // if user is filtering for specific PAT
                                    if (!options.Equals(""))
                                    {
                                        if (options.ToLower().Equals(keyName.ToLower()))
                                        {

                                            // only display an active PAT
                                            if (item.active == true)
                                            {
                                                Console.WriteLine("{0,20} | {1,10} | {2,20} | {3,70}", item.name, item.active, theKeyScope, valuePairs[theKeyScope]);
                                            }
                                        }
                                    }

                                    // if user just wants to see all PATs it can see
                                    else
                                    {
                                        // only display an active PAT
                                        if (item.active == true)
                                        {
                                            Console.WriteLine("{0,20} | {1,10} | {2,20} | {3,70}", item.name, item.active, theKeyScope, valuePairs[theKeyScope]);
                                        }
                                    }


                                }
                            }

                            // figure out if there are more results due to paging and we need to send another request for the next page
                            for (int i = 0; i < myWebResponse.Headers.Count; ++i)
                            {

                                string[] splitValues = myWebResponse.Headers[i].Split(',');
                                foreach (string val in splitValues)
                                {

                                    if (val.Contains("rel=\"next\""))
                                    {
                                        nextURL = val;
                                        nextURL = nextURL.Replace(">; rel=\"next\"", "");
                                        nextURL = nextURL.Substring(1, nextURL.Length - 1);

                                    }

                                }
                            }

                            // make subsequent requests if more than 1 page
                            if (!nextURL.Equals(""))
                            {
                                await makeSubsequentRequestAsync(credential, nextURL, options, userId, valuePairs);
                            }
                        }

                    }

                }

                catch (Exception ex)
                {
                    Console.WriteLine("");
                    Console.WriteLine("[-] ERROR: Could not retrieve listing of privileges for current API token. Exception: " + ex.ToString());
                    Console.WriteLine("");
                }
            }


        }



        // this is just placeholder for making more than 1 request due to paging of results. will have better global solution at some point than this band-aid
        public static async Task makeSubsequentRequestAsync(string credential, string url, string options, string userId, Dictionary<string, string> valuePairs)
        {

            try
            {

                string accessToken = "";

                // if username/password auth being used, get the access token first. this is needed for subsequent API requests
                if (credential.Contains(":"))
                {

                    accessToken = library.GitLabUtils.GetAccessToken(credential, url);

                }



                // get personal access tokens for user
                var webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url);
                if (webRequest != null)
                {
                    // set header values
                    webRequest.Method = "GET";
                    webRequest.ContentType = "application/json";
                    webRequest.Headers.Add("PRIVATE-TOKEN", credential);
                    webRequest.UserAgent = "SCMKIT-5dc493ada400c79dd318abbe770dac7c";

                    // get web response
                    WebResponse myWebResponse = await webRequest.GetResponseAsync();
                    string content;
                    var reader = new StreamReader(myWebResponse.GetResponseStream());
                    content = reader.ReadToEnd();

                    string nextURL = "";


                    // parse the JSON output and display results
                    List<apiPermissions> apiPerms = JsonConvert.DeserializeObject<List<apiPermissions>>(content);

                    foreach (apiPermissions item in apiPerms)
                    {
                        string keyName = item.name;
                        string uid = item.user_id;


                        // if user is not filtering and just wants to see there tokens
                        if (options.Equals("") && uid.Equals(userId))
                        {

                            if (uid.Equals(userId))
                            {
                                string[] theScopes = item.scopes;
                                for (int i = 0; i < theScopes.Length; i++)
                                {
                                    string theKeyScope = theScopes[i];

                                    // only display an active PAT
                                    if (item.active == true)
                                    {
                                        Console.WriteLine("{0,20} | {1,10} | {2,20} | {3,70}", item.name, item.active, theKeyScope, valuePairs[theKeyScope]);
                                    }
                                }
                            }
                        }

                        // if user is filtering on specific PAT
                        if (!options.Equals(""))
                        {

                            string[] theScopes = item.scopes;
                            for (int i = 0; i < theScopes.Length; i++)
                            {
                                string theKeyScope = theScopes[i];

                                // if user is filtering for specific PAT
                                if (!options.Equals(""))
                                {
                                    if (options.ToLower().Equals(keyName.ToLower()))
                                    {
                                        // only display an active PAT
                                        if (item.active == true)
                                        {
                                            Console.WriteLine("{0,20} | {1,10} | {2,20} | {3,70}", item.name, item.active, theKeyScope, valuePairs[theKeyScope]);
                                        }
                                    }
                                }

                                // if user just wants to see all PATs it can see
                                else
                                {
                                    // only display an active PAT
                                    if (item.active == true)
                                    {
                                        Console.WriteLine("{0,20} | {1,10} | {2,20} | {3,70}", item.name, item.active, theKeyScope, valuePairs[theKeyScope]);
                                    }
                                }


                            }



                            // figure out if there are more results due to paging and we need to send another request for the next page
                            for (int i = 0; i < myWebResponse.Headers.Count; ++i)
                            {

                                string[] splitValues = myWebResponse.Headers[i].Split(',');
                                foreach (string val in splitValues)
                                {

                                    if (val.Contains("rel=\"next\""))
                                    {
                                        nextURL = val;
                                        nextURL = nextURL.Replace(">; rel=\"next\"", "");
                                        nextURL = nextURL.Substring(1, nextURL.Length - 1);
                                        nextURL = nextURL.Replace("<", "");

                                    }

                                }
                            }

                            // make subsequent requests if more pages
                            if (!nextURL.Equals(""))
                            {
                                await makeSubsequentRequestAsync(credential, nextURL, options, userId, valuePairs);
                            }


                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR:  Could not retrieve listing of privileges for current API token. Exception: " + ex.ToString());
                Console.WriteLine("");
            }


        }

    }

}
