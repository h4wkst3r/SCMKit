using System;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace SCMKit.modules.bitbucket
{
    class CreateSSHKey
    {


        public static async Task execute(string credential, string url, string options, string system)
        {

            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("createsshkey", credential, url, options, system));

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
                string tableHeader = string.Format("{0,12}", "SSH Key ID");
                Console.WriteLine(tableHeader);
                Console.WriteLine(new String('-', tableHeader.Length));

                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                await library.Utils.HeartbeatRequest(url);

                string[] splitCred = credential.Split(':');


                // web request to add SSH key via REST API
                var webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/rest/ssh/1.0/keys");
                if (webRequest != null)
                {
                    // set header values
                    webRequest.Method = "POST";
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

                        // create random SSH Key label
                        Random rd = new Random();
                        const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
                        char[] chars = new char[5];

                        for (int i = 0; i < 5; i++)
                        {
                            chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
                        }
                        string sshKeyLabel = new string(chars);
                        sshKeyLabel = "SCMKIT-" + sshKeyLabel;

                        string json = "{\"" + "text" + "\": \"" + options + " " + sshKeyLabel + "\"}";
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

                        // get the SSH key ID
                        string sshKeyID = "";
                        int startIndex = content.IndexOf("\"id\":");
                        int endIndex = content.IndexOf("\"text\":");
                        sshKeyID = content.Substring(startIndex + "\"id\":".Length, endIndex - startIndex - "\"id\"".Length);
                        sshKeyID = sshKeyID.Replace("\"", "");
                        sshKeyID = sshKeyID.Replace(",", "");

                        Console.WriteLine("{0,12}", sshKeyID);
                        Console.WriteLine("");
                        Console.WriteLine("[+] SUCCESS: The " + splitCred[0] + " user SSH key was successfully added.");
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
                Console.WriteLine("[-] ERROR: Could not create SSH Key. Exception: " + ex.ToString());
                Console.WriteLine("");
            }

        }


    }
}
