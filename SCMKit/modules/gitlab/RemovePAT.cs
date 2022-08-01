using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.IO;

namespace SCMKit.modules.gitlab
{
    class RemovePAT
    {

        public static async Task execute(string credential, string url, string options, string system)
        {



            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("removepat", credential, url, options, system));



            try
            {


                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                await library.Utils.HeartbeatRequest(url);


                // proceed with request to remove PAT for the user given
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
                    Console.WriteLine("[-] ERROR: Must supply ID of PAT to remove.");
                    Console.WriteLine("");
                    return;
                }


                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("[*] INFO: Revoking personal access token of ID: " + options);
                Console.WriteLine("");

                // perform request to remove PAT
                var webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/api/v4/personal_access_tokens/" + options);
                if (webRequest != null)
                {
                    // set header values
                    webRequest.Method = "DELETE";
                    webRequest.ContentType = "application/x-www-form-urlencoded";
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


                    // set body and sent request
                    using (var streamWriter = new StreamWriter(webRequest.GetRequestStream()))
                    {

                        string body = "";
                        streamWriter.Write(body);
                    }

                    // get the response and the access token
                    var httpResponse = (HttpWebResponse)webRequest.GetResponse();
                    var streamReader = new StreamReader(httpResponse.GetResponseStream());
                    string result = streamReader.ReadToEnd();

                    Console.WriteLine("");
                    Console.WriteLine("[+] SUCCESS: The personal access token of ID " + options + " was successfully revoked.");
                    Console.WriteLine("");

                }


            }

            catch (KeyNotFoundException ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: User provided does not exist to revoke personal access token. Exception: " + ex.ToString());
                Console.WriteLine("");
            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Could not revoke personal access token for user. Exception: " + ex.ToString());
                Console.WriteLine("");
            }

        }
    }
}
