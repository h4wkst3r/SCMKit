using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.IO;

namespace SCMKit.modules.gitlab
{
    class CreateSSHKey
    {

        public static async Task execute(string credential, string url, string options, string system)
        {

            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("createsshkey", credential, url, options, system));

            // dictionary to hold lookup table user ID's and usernames
            Dictionary<string, string> userMapping = new Dictionary<string, string>();

            try
            {
                await library.Utils.HeartbeatRequest(url);


                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // proceed with request to add ssh key for given user
                string accessToken = "";


                // if username/password auth being used, get the access token first. this is needed for subsequent API requests
                if (credential.Contains(":"))
                {

                    accessToken = library.GitLabUtils.GetAccessToken(credential, url);

                }



                // perform request to create SSH key
                var webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/api/v4/user/keys");

                if (webRequest != null)
                {
                    // set header values
                    webRequest.Method = "POST";
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


                    // set body and sent request
                    using (var streamWriter = new StreamWriter(webRequest.GetRequestStream()))
                    {

                        // create random key name
                        Random rd = new Random();
                        const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
                        char[] chars = new char[5];

                        for (int i = 0; i < 5; i++)
                        {
                            chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
                        }
                        string sshKeyName = new string(chars);
                        sshKeyName = "SCMKIT-" + sshKeyName;


                        string json = "{\"" + "title" + "\": \"" + sshKeyName + "\",\"key\":\"" + options + "\"}";
                        streamWriter.Write(json);
                    }

                    // get the response and the access token
                    var httpResponse = (HttpWebResponse)webRequest.GetResponse();
                    var streamReader = new StreamReader(httpResponse.GetResponseStream());
                    string result = streamReader.ReadToEnd();


                    // create table header
                    string tableHeader = string.Format("{0,12} | {1,15}", "SSH Key ID", "SSH Key Name");
                    Console.WriteLine(tableHeader);
                    Console.WriteLine(new String('-', tableHeader.Length));

                    int startIndexTokenID = result.IndexOf("\"id\":");
                    int endIndexTokenID = result.IndexOf("\"title\":");
                    string tokenID = result.Substring(startIndexTokenID + "\"id\":".Length, endIndexTokenID - startIndexTokenID - "\"id\"".Length);
                    tokenID = tokenID.Replace("\"", "");
                    tokenID = tokenID.Replace(",", "");

                    int startIndexTokenName = result.IndexOf("\"title\":");
                    int endIndexTokenName = result.IndexOf("\"created_at\":");
                    string tokenName = result.Substring(startIndexTokenName + "\"title\":".Length, endIndexTokenName - startIndexTokenName - "\"title\"".Length).Replace("\"", "");
                    tokenName = tokenName.Replace(",", "");



                    Console.WriteLine("{0,12} | {1,15}", tokenID, tokenName);




                    Console.WriteLine("");
                    Console.WriteLine("[+] SUCCESS: The user SSH key was successfully added.");
                    Console.WriteLine("");

                }


            }

            catch (KeyNotFoundException ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: User provided does not exist to create SSH key for. Exception: " + ex.ToString());
                Console.WriteLine("");
            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Could not create SSH key for user. Exception: " + ex.ToString());
                Console.WriteLine("");
            }

        }
    }
}
