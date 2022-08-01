using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace SCMKit.modules.bitbucket
{
    class CreatePAT
    {

        // hashtable to store mapping of PAT and permissions
        private static Dictionary<string, string> patMapping = new Dictionary<string, string>();

        public static async Task execute(string credential, string url, string options, string system)
        {

            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("createpat", credential, url, options, system));

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


                // create table header
                string tableHeader = string.Format("{0,25} | {1,15} | {2,50}", "ID", "Name", "Token");
                Console.WriteLine(tableHeader);
                Console.WriteLine(new String('-', tableHeader.Length));

                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                await library.Utils.HeartbeatRequest(url);

                string[] splitCred = credential.Split(':');

                // web request to create PAT via REST api
                var webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/rest/access-tokens/1.0/users/" + splitCred[0]);
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


                    // set body and send request
                    using (var streamWriter = new StreamWriter(webRequest.GetRequestStream()))
                    {

                        // create random token name
                        Random rd = new Random();
                        const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
                        char[] chars = new char[5];

                        for (int i = 0; i < 5; i++)
                        {
                            chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
                        }
                        string personalAccessTokenName = new string(chars);
                        personalAccessTokenName = "SCMKIT-" + personalAccessTokenName;


                        string json = "{\"name\": \"" + personalAccessTokenName + "\",\"permissions\": [\"REPO_ADMIN\",\"PROJECT_ADMIN\"],\"expiryDays\": \"\"}";
                        streamWriter.Write(json);
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

                        // get the PAT name and id
                        string patName = "";
                        int startIndex = content.IndexOf("\"id\":");
                        int endIndex = content.IndexOf("\"permissions\":");
                        patName = content.Substring(startIndex + "\"id\":".Length, endIndex - startIndex - "\"id\"".Length);
                        patName = patName.Replace("\"", "");
                        patName = patName.Replace(",", "");
                        string[] patNameArray = patName.Split(':');
                        patMapping.Add(patNameArray[0].Replace("createdDate", ""), patNameArray[patNameArray.Length - 1]);


                        //  parse the actual token
                        string tokenContent = "";
                        int startIndexContent = content.IndexOf("\"token\":");
                        int endIndexContent = content.Length - 1;
                        tokenContent = content.Substring(startIndexContent + "\"token\":".Length, endIndexContent - startIndexContent - "\"token\":".Length);
                        tokenContent = tokenContent.Replace("\"", "");


                        foreach (var item in patMapping)
                        {
                            Console.WriteLine("{0,25} | {1,15} | {2,50}", item.Key, item.Value, tokenContent);
                        }

                        Console.WriteLine("");
                        Console.WriteLine("[+] SUCCESS: The " + splitCred[0] + " user personal access token was successfully added.");
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
                Console.WriteLine("[-] ERROR: Could not create PAT. Exception: " + ex.ToString());
                Console.WriteLine("");
            }

        }


    }
}
