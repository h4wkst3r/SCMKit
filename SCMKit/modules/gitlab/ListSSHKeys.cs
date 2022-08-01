using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace SCMKit.modules.gitlab
{
    class ListSSHKeys
    {

        public static async Task execute(string credential, string url, string options, string system)
        {

            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("listsshkey", credential, url, options, system));

            // dictionary to hold lookup table user ID's and usernames
            Dictionary<string, string> userMapping = new Dictionary<string, string>();

            try
            {
                await library.Utils.HeartbeatRequest(url);


                // create table header
                string tableHeader = string.Format("{0,12} | {1,25} | {2,20}", "SSH Key ID", "SSH Key Value", "Title");
                Console.WriteLine(tableHeader);
                Console.WriteLine(new String('-', tableHeader.Length));

                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // proceed with request to list ssh keys for given user
                string accessToken = "";


                // if username/password auth being used, get the access token first. this is needed for subsequent API requests
                if (credential.Contains(":"))
                {

                    accessToken = library.GitLabUtils.GetAccessToken(credential, url);

                }



                // perform request to list SSH keys
                var webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/api/v4/user/keys");

                if (webRequest != null)
                {
                    // set header values
                    webRequest.Method = "GET";
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


                    // get all instances of the SSH key id
                    IEnumerable<int> startingIndexesID = library.Utils.AllIndexesOf(result, "\"id\":");
                    IEnumerable<int> endingIndexesID = library.Utils.AllIndexesOf(result, "\"title\":");

                    List<string> listOfIndexIDs = new List<string>();

                    for (int i = 0; i < startingIndexesID.Count(); i++)
                    {

                        string sshKeyID = "";
                        sshKeyID = result.Substring(startingIndexesID.ElementAt(i) + "\"id\":".Length, endingIndexesID.ElementAt(i) - startingIndexesID.ElementAt(i) - "\"id\"".Length);
                        sshKeyID = sshKeyID.Replace("\"", "");
                        sshKeyID = sshKeyID.Replace(",", "");
                        listOfIndexIDs.Add(sshKeyID);

                    }


                    // get all instances of the SSH key value
                    IEnumerable<int> startingIndexesValue = library.Utils.AllIndexesOf(result, "\"key\":");
                    IEnumerable<int> endingIndexesValue = library.Utils.AllIndexesOf(result, "\"}");

                    List<string> listOfIndexValues = new List<string>();

                    for (int i = 0; i < startingIndexesValue.Count(); i++)
                    {

                        string sshKeyValue = "";
                        sshKeyValue = result.Substring(startingIndexesValue.ElementAt(i) + "\"key\":".Length, endingIndexesValue.ElementAt(i) - startingIndexesValue.ElementAt(i) - "\"key\"".Length);
                        sshKeyValue = sshKeyValue.Replace("\"", "");
                        sshKeyValue = sshKeyValue.Replace(",", "");
                        listOfIndexValues.Add(sshKeyValue);


                    }



                    // get all instances of the SSH key title
                    IEnumerable<int> startingIndexesTitle = library.Utils.AllIndexesOf(result, "\"title\":");
                    IEnumerable<int> endingIndexesTitle = library.Utils.AllIndexesOf(result, "\"created_at\":");

                    List<string> listOfIndexTitles = new List<string>();

                    for (int i = 0; i < startingIndexesTitle.Count(); i++)
                    {

                        string sshKeyTitle = "";
                        sshKeyTitle = result.Substring(startingIndexesTitle.ElementAt(i) + "\"title\":".Length, endingIndexesTitle.ElementAt(i) - startingIndexesTitle.ElementAt(i) - "\"ketitley\"".Length);
                        sshKeyTitle = sshKeyTitle.Replace("\"", "");
                        sshKeyTitle = sshKeyTitle.Replace(",", "");
                        listOfIndexTitles.Add(sshKeyTitle);


                    }


                    // iterate through and print the ssh key ID's and contents
                    for (int i = 0; i < listOfIndexIDs.Count(); i++)
                    {

                        string[] sshKeyArray = listOfIndexValues[i].Split(' ');
                        string justSSHKey = sshKeyArray[1].Substring(sshKeyArray[1].Length - 20, 20);


                        Console.WriteLine("{0,12} | {1,25} | {2,20}", listOfIndexIDs[i], "....." + justSSHKey, listOfIndexTitles[i]);
                    }





                }


            }

            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Could not list SSH keys for user. Exception: " + ex.ToString());
                Console.WriteLine("");
            }

        }



    }
}
