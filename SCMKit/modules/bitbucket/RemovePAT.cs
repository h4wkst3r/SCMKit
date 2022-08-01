using System;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace SCMKit.modules.bitbucket
{
    class RemovePAT
    {

        public static async Task execute(string credential, string url, string options, string system)
        {

            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("removepat", credential, url, options, system));

            try
            {

                string sessID = "";

                // if username/password auth is used, get session ID for remainder of requests
                if (credential.Contains(":"))
                {
                    sessID = library.BitbucketUtils.GetSessionID(credential, url);
                    if (sessID == null)
                    {
                        Console.WriteLine("");
                        Console.WriteLine("[-] ERROR: Credentials provided are not valid.");
                        Console.WriteLine("");
                        return;

                    }
                }

                // if API token was provided, display message and return
                else
                {
                    Console.WriteLine("");
                    Console.WriteLine("[-] ERROR: API token authentication is not supported for this module. Please provide username/password with the appropriate permissions.");
                    Console.WriteLine("");
                    return;

                }




                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                await library.Utils.HeartbeatRequest(url);

                string[] splitCred = credential.Split(':');

                // if user didn't specify an ID, display message and return
                if (options.Equals(""))
                {
                    Console.WriteLine("");
                    Console.WriteLine("[-] ERROR: Must supply ID of PAT to remove.");
                    Console.WriteLine("");
                    return;
                }

                // web request to get pats via REST api
                var webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/rest/access-tokens/1.0/users/" + splitCred[0] + "/" + options);
                if (webRequest != null)
                {
                    // set header values
                    webRequest.Method = "DELETE";
                    webRequest.ContentType = "application/json";
                    webRequest.UserAgent = "SCMKIT-5dc493ada400c79dd318abbe770dac7c";

                    // if username/password auth was used, then pass the sessionID
                    if (credential.Contains(":"))
                    {
                        webRequest.Headers.Add("Cookie", "BITBUCKETSESSIONID= " + sessID);
                    }
                    // if user just specified http access token
                    else
                    {
                        webRequest.Headers.Add("Authorization", "Bearer " + credential);
                    }


                    // get web response
                    WebResponse myWebResponse = await webRequest.GetResponseAsync();
                    string content;
                    var reader = new StreamReader(myWebResponse.GetResponseStream());
                    content = reader.ReadToEnd();

                    bool validCreds = false;

                    // figure out if creds valid
                    for (int i = 0; i < myWebResponse.Headers.Count; ++i)
                    {
                        if (myWebResponse.Headers.Keys[i].ToString().ToLower().Equals("x-auserid"))
                        {
                            validCreds = true;
                        }

                    }


                    if (validCreds)
                    {

                        Console.WriteLine("");
                        Console.WriteLine("[+] SUCCESS: The personal access token of ID " + options + " was successfully revoked.");
                        Console.WriteLine("");


                    }

                    // if creds invalid
                    else
                    {
                        Console.WriteLine("");
                        Console.WriteLine("[-] ERROR: Credentials (username/password OR API token) provided are not valid.");
                        Console.WriteLine("");
                    }

                }

            }

            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Could not remove PAT. Exception: " + ex.ToString());
                Console.WriteLine("");
            }

        }


    }
}
