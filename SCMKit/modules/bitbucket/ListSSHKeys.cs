using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;


namespace SCMKit.modules.bitbucket
{


    class ListSSHKeys
    {


        public static async Task execute(string credential, string url, string options, string system)
        {

            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("listsshkey", credential, url, options, system));

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

                // create table header
                string tableHeader = string.Format("{0,12} | {1,25} | {2,20}", "SSH Key ID", "SSH Key Value", "Label");
                Console.WriteLine(tableHeader);
                Console.WriteLine(new String('-', tableHeader.Length));

                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                await library.Utils.HeartbeatRequest(url);

                string[] splitCred = credential.Split(':');


                // web request to list SSH keys via REST API
                var webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/rest/ssh/1.0/keys");
                if (webRequest != null)
                {
                    // set header values
                    webRequest.Method = "GET";
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


                    // only proceed if valid creds
                    if (validCreds)
                    {



                        // get all instances of the SSH key id
                        IEnumerable<int> startingIndexesID = library.Utils.AllIndexesOf(content, "\"id\":");
                        IEnumerable<int> endingIndexesID = library.Utils.AllIndexesOf(content, "\"text\":");

                        List<string> listOfIndexIDs = new List<string>();

                        for (int i = 0; i < startingIndexesID.Count(); i++)
                        {

                            string sshKeyID = "";
                            sshKeyID = content.Substring(startingIndexesID.ElementAt(i) + "\"id\":".Length, endingIndexesID.ElementAt(i) - startingIndexesID.ElementAt(i) - "\"id\"".Length);
                            sshKeyID = sshKeyID.Replace("\"", "");
                            sshKeyID = sshKeyID.Replace(",", "");
                            listOfIndexIDs.Add(sshKeyID);

                        }


                        // get all instances of the SSH key value
                        IEnumerable<int> startingIndexesValue = library.Utils.AllIndexesOf(content, "\"text\":");
                        IEnumerable<int> endingIndexesValue = library.Utils.AllIndexesOf(content, "\"label\":");

                        List<string> listOfIndexValues = new List<string>();

                        for (int i = 0; i < startingIndexesValue.Count(); i++)
                        {

                            string sshKeyValue = "";
                            sshKeyValue = content.Substring(startingIndexesValue.ElementAt(i) + "\"text\":".Length, endingIndexesValue.ElementAt(i) - startingIndexesValue.ElementAt(i) - "\"text\"".Length);
                            sshKeyValue = sshKeyValue.Replace("\"", "");
                            sshKeyValue = sshKeyValue.Replace(",", "");
                            listOfIndexValues.Add(sshKeyValue);


                        }


                        // iterate through and print the ssh key ID's and contents
                        for (int i = 0; i < listOfIndexIDs.Count(); i++)
                        {

                            string[] sshKeyArray = listOfIndexValues[i].Split(' ');
                            string justSSHKey = sshKeyArray[1].Substring(sshKeyArray[1].Length - 20, 20);
                            string justSSHLabel = "";
                            //the ssh key label is array value [2]
                            if (sshKeyArray.Length == 3)
                            {
                                justSSHLabel = sshKeyArray[2];
                            }

                            Console.WriteLine("{0,12} | {1,20} | {2,20}", listOfIndexIDs[i], "....." + justSSHKey, justSSHLabel);
                        }


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
