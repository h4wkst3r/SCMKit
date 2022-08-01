using System;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace SCMKit.modules.bitbucket
{
    class AddAdmin
    {
        public static async Task execute(string credential, string url, string options, string system)
        {

            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("addadmin", credential, url, options, system));

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

                // web request to add admin via rest API
                var webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/rest/api/1.0/admin/permissions/users?name=" + options + "&permission=ADMIN");
                if (webRequest != null)
                {
                    // set header values
                    webRequest.Method = "PUT";
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

                    bool addAdminSuccessful = false;

                    // figure out if request was successful
                    for (int i = 0; i < myWebResponse.Headers.Count; ++i)
                    {
                        if (myWebResponse.Headers.Keys[i].ToString().ToLower().Equals("x-auserid"))
                        {
                            addAdminSuccessful = true;
                        }

                    }


                    if (addAdminSuccessful)
                    {
                        Console.WriteLine("");
                        Console.WriteLine("[+] SUCCESS: Successfully added " + options + " user to the admin role.");
                        Console.WriteLine("");

                    }

                    // if not successful
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
                Console.WriteLine("[-] ERROR: Could not add user to admin group. Exception: " + ex.ToString());
                Console.WriteLine("");
            }

        }


    }

}
