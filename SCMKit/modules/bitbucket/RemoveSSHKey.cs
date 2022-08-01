using System;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace SCMKit.modules.bitbucket
{
    class RemoveSSHKey
    {

        public static async Task execute(string credential, string url, string options, string system)
        {

            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("removesshkey", credential, url, options, system));

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
                    Console.WriteLine("[-] ERROR: Must supply ID of SSH key to remove.");
                    Console.WriteLine("");
                    return;
                }


                // web request to remove SSH key via REST API
                var webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/rest/ssh/1.0/keys/" + options);
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


                    // set body and send request
                    using (var streamWriter = new StreamWriter(webRequest.GetRequestStream()))
                    {


                        string json = "{\"" + "text" + "\": \"" + options + "\"}";
                        streamWriter.Write(json);
                    }

                    // get web response
                    HttpWebResponse myWebResponse = (HttpWebResponse)await webRequest.GetResponseAsync();
                    string content;
                    var reader = new StreamReader(myWebResponse.GetResponseStream());
                    content = reader.ReadToEnd();

                    // if we got 204 response, SSH key was removed
                    if (myWebResponse.StatusCode.ToString().ToLower().Equals("nocontent"))
                    {
                        Console.WriteLine("");
                        Console.WriteLine("[+] SUCCESS: The SSH key of ID " + options + " was successfully revoked.");
                        Console.WriteLine("");
                    }
                    else
                    {
                        Console.WriteLine("");
                        Console.WriteLine("[-] ERROR: There was an error revoking the SSH key.");
                        Console.WriteLine("");
                    }


                }

            }

            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Could not remove SSH Key. Exception: " + ex.ToString());
                Console.WriteLine("");
            }

        }
    }
}
