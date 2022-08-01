using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace SCMKit.modules.bitbucket
{
    class RepoList
    {

        // dictionary to hold the list of repos and their URLs
        private static Dictionary<string, string> repoMapping = new Dictionary<string, string>();

        public static async Task execute(string credential, string url, string options, string system)
        {

            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("listrepo", credential, url, options, system));

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
                string tableHeader = string.Format("{0,30} | {1,50}", "Name", "URL");
                Console.WriteLine(tableHeader);
                Console.WriteLine(new String('-', tableHeader.Length));

                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                await library.Utils.HeartbeatRequest(url);

                // web request to get repos via REST api
                var webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/rest/api/1.0/repos?limit=25");
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

                        // parse the JSON output and display results
                        JsonTextReader jsonResult = new JsonTextReader(new StringReader(content));

                        string name = "";
                        string link = "";
                        string propName = "";
                        string nextPageStart = "";

                        // read the json results
                        while (jsonResult.Read())
                        {
                            switch (jsonResult.TokenType.ToString())
                            {
                                case "StartObject":
                                    break;
                                case "EndObject":
                                    break;
                                case "StartArray":
                                    break;
                                case "EndArray":

                                    // add repo name and link to dictionary
                                    if (!name.Equals("") && !link.Equals(""))
                                    {
                                        if (!repoMapping.ContainsKey(name.Trim()) && !repoMapping.ContainsValue(link.Trim()))
                                        {
                                            repoMapping.Add(name.Trim(), link);
                                        }
                                    }

                                    break;
                                case "PropertyName":
                                    propName = jsonResult.Value.ToString();
                                    break;
                                case "String":

                                    if (propName.ToLower().Equals("slug"))
                                    {
                                        name = jsonResult.Value.ToString();
                                    }
                                    if (propName.ToLower().Equals("href") && jsonResult.Value.ToString().EndsWith(".git") && jsonResult.Value.ToString().StartsWith("http"))
                                    {
                                        link = jsonResult.Value.ToString();
                                    }
                                    break;
                                case "Integer":

                                    if (propName.ToLower().Equals("nextpagestart"))
                                    {
                                        nextPageStart = jsonResult.Value.ToString();
                                    }

                                    break;
                                default:
                                    break;

                            }

                        }


                        // iterate throught the dictionary of repos and print them
                        foreach (var item in repoMapping)
                        {
                            Console.WriteLine("{0,30} | {1,50}", item.Key, item.Value);
                        }


                        // if there are more pages, then make subsequent requests
                        if (!nextPageStart.Equals(""))
                        {
                            await makeSubsequentRequestAsync(credential, url, options, sessID, nextPageStart);
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
                Console.WriteLine("[-] ERROR: Could not retrieve listing of repos. Exception: " + ex.ToString());
                Console.WriteLine("");
            }

        }




        // this is just placeholder for making more than 1 request due to paging of results. will have better global solution at some point than this band-aid
        public static async Task makeSubsequentRequestAsync(string credential, string url, string options, string sessID, string nextPage)
        {


            try
            {

                repoMapping.Clear();


                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // web request to get repos via REST api
                var webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/rest/api/1.0/repos?limit=25&start=" + nextPage);
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

                        // parse the JSON output and display results
                        JsonTextReader jsonResult = new JsonTextReader(new StringReader(content));

                        string name = "";
                        string link = "";
                        string propName = "";
                        string nextPageStart = "";

                        // read the json results
                        while (jsonResult.Read())
                        {
                            switch (jsonResult.TokenType.ToString())
                            {
                                case "StartObject":
                                    break;
                                case "EndObject":
                                    break;
                                case "StartArray":
                                    break;
                                case "EndArray":

                                    // add repo name and link to dictionary
                                    if (!name.Equals("") && !link.Equals(""))
                                    {
                                        if (!repoMapping.ContainsKey(name.Trim()) && !repoMapping.ContainsValue(link.Trim()))
                                        {
                                            repoMapping.Add(name.Trim(), link);
                                        }
                                    }

                                    break;
                                case "PropertyName":
                                    propName = jsonResult.Value.ToString();
                                    break;
                                case "String":

                                    if (propName.ToLower().Equals("slug"))
                                    {
                                        name = jsonResult.Value.ToString();
                                    }
                                    if (propName.ToLower().Equals("href") && jsonResult.Value.ToString().EndsWith(".git") && jsonResult.Value.ToString().StartsWith("http"))
                                    {
                                        link = jsonResult.Value.ToString();
                                    }
                                    break;
                                case "Integer":

                                    if (propName.ToLower().Equals("nextpagestart"))
                                    {
                                        nextPageStart = jsonResult.Value.ToString();
                                    }

                                    break;
                                default:
                                    break;

                            }

                        }


                        // iterate throught the dictionary of repos and print them
                        foreach (var item in repoMapping)
                        {
                            Console.WriteLine("{0,30} | {1,50}", item.Key, item.Value);
                        }

                        // if there are more pages, then make subsequent requests
                        if (!nextPageStart.Equals(""))
                        {
                            await makeSubsequentRequestAsync(credential, url, options, sessID, nextPageStart);
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
                Console.WriteLine("[-] ERROR: Could not retrieve listing of repos. Exception: " + ex.ToString());
                Console.WriteLine("");
            }


        }



    }
}
