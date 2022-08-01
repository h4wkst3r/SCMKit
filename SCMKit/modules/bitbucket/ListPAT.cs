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
    class ListPAT
    {

        // hashtable to store mapping of PAT and permissions
        private static Dictionary<string, string> patMapping = new Dictionary<string, string>();

        public static async Task execute(string credential, string url, string options, string system)
        {

            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("listpat", credential, url, options, system));

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
                string tableHeader = string.Format("{0,15} | {1,30} | {2,50}", "ID", "Name", "Permissions");
                Console.WriteLine(tableHeader);
                Console.WriteLine(new String('-', tableHeader.Length));

                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                await library.Utils.HeartbeatRequest(url);

                string[] splitCred = credential.Split(':');
                string theUser = "";
                if (options.Equals(""))
                {
                    theUser = splitCred[0];
                }
                else
                {
                    theUser = options;
                }

                // web request to get pats via REST api
                var webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/rest/access-tokens/1.0/users/" + theUser);
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


                    if (validCreds)
                    {
                        // get all instances of PAT ID's
                        IEnumerable<int> startingIndexesPATids = library.Utils.AllIndexesOf(content, "\"id\":\"");
                        IEnumerable<int> endingIndexesPATids = library.Utils.AllIndexesOf(content, "\"createdDate\":");

                        List<string> listOfPatIDs = new List<string>();
                        for (int i = 0; i < startingIndexesPATids.Count(); i++)
                        {

                            string patID = "";
                            patID = content.Substring(startingIndexesPATids.ElementAt(i) + "\"id\":\"".Length, endingIndexesPATids.ElementAt(i) - startingIndexesPATids.ElementAt(i) - "\"id\":\"".Length);
                            patID = patID.Replace("\"", "");
                            patID = patID.Replace(",", "");
                            listOfPatIDs.Add(patID);

                        }




                        // get all instances of the token name
                        IEnumerable<int> startingIndexesName = library.Utils.AllIndexesOf(content, "\"createdDate\":");
                        IEnumerable<int> endingIndexesName = library.Utils.AllIndexesOf(content, "\"permissions\":");

                        List<string> listOfPatNames = new List<string>();

                        for (int i = 0; i < startingIndexesName.Count(); i++)
                        {

                            string patName = "";
                            patName = content.Substring(startingIndexesName.ElementAt(i) + "\"createdDate\":".Length, endingIndexesName.ElementAt(i) - startingIndexesName.ElementAt(i) - "\"createdDate\"".Length);
                            patName = patName.Replace("\"", "");
                            patName = patName.Replace(",", "");
                            string[] patNameArray = patName.Split(':');
                            listOfPatNames.Add(patNameArray[patNameArray.Length - 1]);

                        }


                        // get all instances of the token permissions
                        IEnumerable<int> startingIndexesPermissions = library.Utils.AllIndexesOf(content, "\"permissions\":");
                        IEnumerable<int> endingIndexesPermissions = library.Utils.AllIndexesOf(content, "\"user\":");

                        List<string> listOfPatPermissions = new List<string>();


                        for (int i = 0; i < startingIndexesPermissions.Count(); i++)
                        {

                            string patPermissions = "";
                            patPermissions = content.Substring(startingIndexesPermissions.ElementAt(i) + "\"permissions\":".Length, endingIndexesPermissions.ElementAt(i) - startingIndexesPermissions.ElementAt(i) - "\"permissions\"".Length);
                            patPermissions = patPermissions.Replace("\"],\"", "");
                            patPermissions = patPermissions.Replace("[", "");
                            patPermissions = patPermissions.Replace("]", "");
                            patPermissions = patPermissions.Replace("\"", "");
                            listOfPatPermissions.Add(patPermissions);

                        }


                        // the pat names and permissions lists with have same counts, each index is associated with the other list, so list the permissions for each token
                        for (int i = 0; i < listOfPatNames.Count(); i++)
                        {
                            Console.WriteLine("{0,15} | {1,30} | {2,50}", listOfPatIDs[i], listOfPatNames[i], listOfPatPermissions[i]);



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
                Console.WriteLine("[-] ERROR: Could not retrieve listing of PATs. Exception: " + ex.ToString());
                Console.WriteLine("");
            }

        }


    }
}
