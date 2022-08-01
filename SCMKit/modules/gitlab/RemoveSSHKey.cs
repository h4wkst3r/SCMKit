using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.IO;


namespace SCMKit.modules.gitlab
{
    class RemoveSSHKey
    {

        public static async Task execute(string credential, string url, string options, string system)
        {

            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("removesshkey", credential, url, options, system));

            // dictionary to hold lookup table user ID's and usernames
            Dictionary<string, string> userMapping = new Dictionary<string, string>();

            try
            {
                await library.Utils.HeartbeatRequest(url);


                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;


                // proceed with request to remove ssh key for given user
                string accessToken = "";


                // if username/password auth being used, get the access token first. this is needed for subsequent API requests
                if (credential.Contains(":"))
                {

                    accessToken = library.GitLabUtils.GetAccessToken(credential, url);

                }

                // if user didn't specify an ID, display message and return
                if (options.Equals(""))
                {
                    Console.WriteLine("");
                    Console.WriteLine("[-] ERROR: Must supply ID of SSH key to remove.");
                    Console.WriteLine("");
                    return;
                }


                // perform request to remove SSH key
                var webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/api/v4/user/keys/" + options);

                if (webRequest != null)
                {
                    // set header values
                    webRequest.Method = "DELETE";
                    webRequest.ContentType = "application/json";
                    webRequest.UserAgent = "SCMKIT-5dc493ada400c79dd318abbe770dac7c";

                    // if we need to pass the token obtained from the username/password auth
                    if (credential.Contains(":"))
                    {
                        webRequest.Headers.Add("Authorization", "Bearer " + accessToken);
                    }
                    // if user just specified personal access token
                    else
                    {
                        webRequest.Headers.Add("PRIVATE-TOKEN", credential);
                    }


                    // get the response and the access token
                    var httpResponse = (HttpWebResponse)webRequest.GetResponse();
                    var streamReader = new StreamReader(httpResponse.GetResponseStream());
                    string result = streamReader.ReadToEnd();


                    // if we got 204 response, SSH key was removed
                    if (httpResponse.StatusCode.ToString().ToLower().Equals("nocontent"))
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

            catch (KeyNotFoundException ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: User provided does not exist to remove SSH key for. Exception: " + ex.ToString());
                Console.WriteLine("");
            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Could not remove SSH key for user. Exception: " + ex.ToString());
                Console.WriteLine("");
            }

        }


    }
}
