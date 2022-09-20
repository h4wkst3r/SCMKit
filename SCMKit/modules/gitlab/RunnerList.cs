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


    // custom class to store a runner
    public class aRunner
    {

        public string id { get; set; }
        public string name { get; set; }
        public string project { get; set; }


        public aRunner(string id, string name, string project)
        {
            this.id = id;
            this.name = name;
            this.project = project;
        }

    }


    class RunnerList
    {

        private static List<aRunner> theRunners = new List<aRunner>();

        private static List<aRunner> nameAndIDs = new List<aRunner>();


        public static async Task execute(string credential, string url, string options, string system)
        {

            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("listrunner", credential, url, options, system));



            try
            {

                await library.Utils.HeartbeatRequest(url);


                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // proceed with request to list runners accessible for a given user
                string accessToken = "";


                // if username/password auth being used, get the access token first. this is needed for subsequent API requests
                if (credential.Contains(":"))
                {

                    accessToken = library.GitLabUtils.GetAccessToken(credential, url);

                }


                // create table header
                string tableHeader = string.Format("{0,5} | {1,20} | {2,50}", "ID", "Name", "Repo Assigned");
                Console.WriteLine(tableHeader);
                Console.WriteLine(new String('-', tableHeader.Length));


                // perform request to get list of runners as we will need the ID of the runners to get more details
                var webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/api/v4/runners/all");
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



                    // get the response 
                    var httpResponse = (HttpWebResponse)webRequest.GetResponse();
                    var streamReader = new StreamReader(httpResponse.GetResponseStream());
                    string result = streamReader.ReadToEnd();


                    // get all instances of the id field for the id of the runners
                    IEnumerable<int> startingIndexesId = library.Utils.AllIndexesOf(result, "\"id\":");
                    IEnumerable<int> endingIndexesId = library.Utils.AllIndexesOf(result, "\"description\":");

                    List<string> listOfIds = new List<string>();

                    for (int i = 0; i < startingIndexesId.Count(); i++)
                    {

                        string theId = "";
                        theId = result.Substring(startingIndexesId.ElementAt(i) + "\"id\":".Length, endingIndexesId.ElementAt(i) - startingIndexesId.ElementAt(i) - "\"id\"".Length);
                        theId = theId.Replace("\"],\"", "");
                        theId = theId.Replace("[", "");
                        theId = theId.Replace("]", "");
                        theId = theId.Replace("\"", "");
                        theId = theId.Replace(",", "");
                        listOfIds.Add(theId);

                    }


                    // get all instances of the name field for the name of the runners
                    IEnumerable<int> startingIndexesName = library.Utils.AllIndexesOf(result, "\"name\":");
                    IEnumerable<int> endingIndexesName = library.Utils.AllIndexesOf(result, "\"online\":");

                    List<string> listofNames = new List<string>();

                    for (int i = 0; i < startingIndexesId.Count(); i++)
                    {

                        string theName = "";
                        theName = result.Substring(startingIndexesName.ElementAt(i) + "\"name\":".Length, endingIndexesName.ElementAt(i) - startingIndexesName.ElementAt(i) - "\"name\"".Length);
                        theName = theName.Replace("\"],\"", "");
                        theName = theName.Replace("[", "");
                        theName = theName.Replace("]", "");
                        theName = theName.Replace("\"", "");
                        theName = theName.Replace(",", "");
                        listofNames.Add(theName);

                    }


                    // go through and add the id and name to the runner list array
                    for (int i = 0; i < listOfIds.Count(); i++)
                    {
                        nameAndIDs.Add(new aRunner(listOfIds[i], listofNames[i], ""));

                    }


                    // get details of each runner
                    for (int i = 0; i < nameAndIDs.Count(); i++)
                    {

                        GetRunnerDetails(credential, url, nameAndIDs[i].id, accessToken, nameAndIDs[i].name);

                    }


                    // print the details of each runner
                    for (int i = 0; i < theRunners.Count(); i++)
                    {
                        // as long as projects assigned is not blank, display
                        if (!theRunners[i].project.Equals(""))
                        {
                            Console.WriteLine("{0,5} | {1,20} | {2,50}", theRunners[i].id, theRunners[i].name, theRunners[i].project);
                        }



                    }


                }


            }

            catch (Exception ex)
            {
                // if user is not permitted to list all runners, then just list runners owned by current user
                if (ex.Message.Contains("(403) Forbidden"))
                {
                    await getOwnedRunners(credential, url, options, "gitlab");
                }

                // otherwise display normal error message and return
                else
                {
                    Console.WriteLine("");
                    Console.WriteLine("[-] ERROR: Could not get listing of all GitLab runners. Exception: " + ex.ToString());
                    Console.WriteLine("");
                }
                


            }

        }

        // get runners owned by current user (non-admin)
        public static async Task getOwnedRunners(string credential, string url, string options, string system)
        {

            try
            {

                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // proceed with request to list runners accessible for a given user
                string accessToken = "";


                // if username/password auth being used, get the access token first. this is needed for subsequent API requests
                if (credential.Contains(":"))
                {

                    accessToken = library.GitLabUtils.GetAccessToken(credential, url);

                }



                // perform request to get list of runners as we will need the ID of the runners to get more details
                var webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/api/v4/runners");
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



                    // get the response 
                    var httpResponse = (HttpWebResponse)webRequest.GetResponse();
                    var streamReader = new StreamReader(httpResponse.GetResponseStream());
                    string result = streamReader.ReadToEnd();


                    // get all instances of the id field for the id of the runners
                    IEnumerable<int> startingIndexesId = library.Utils.AllIndexesOf(result, "\"id\":");
                    IEnumerable<int> endingIndexesId = library.Utils.AllIndexesOf(result, "\"description\":");

                    List<string> listOfIds = new List<string>();

                    for (int i = 0; i < startingIndexesId.Count(); i++)
                    {

                        string theId = "";
                        theId = result.Substring(startingIndexesId.ElementAt(i) + "\"id\":".Length, endingIndexesId.ElementAt(i) - startingIndexesId.ElementAt(i) - "\"id\"".Length);
                        theId = theId.Replace("\"],\"", "");
                        theId = theId.Replace("[", "");
                        theId = theId.Replace("]", "");
                        theId = theId.Replace("\"", "");
                        theId = theId.Replace(",", "");
                        listOfIds.Add(theId);

                    }


                    // get all instances of the name field for the name of the runners
                    IEnumerable<int> startingIndexesName = library.Utils.AllIndexesOf(result, "\"name\":");
                    IEnumerable<int> endingIndexesName = library.Utils.AllIndexesOf(result, "\"online\":");

                    List<string> listofNames = new List<string>();

                    for (int i = 0; i < startingIndexesId.Count(); i++)
                    {

                        string theName = "";
                        theName = result.Substring(startingIndexesName.ElementAt(i) + "\"name\":".Length, endingIndexesName.ElementAt(i) - startingIndexesName.ElementAt(i) - "\"name\"".Length);
                        theName = theName.Replace("\"],\"", "");
                        theName = theName.Replace("[", "");
                        theName = theName.Replace("]", "");
                        theName = theName.Replace("\"", "");
                        theName = theName.Replace(",", "");
                        listofNames.Add(theName);

                    }


                    // go through and add the id and name to the runner list array
                    for (int i = 0; i < listOfIds.Count(); i++)
                    {
                        nameAndIDs.Add(new aRunner(listOfIds[i], listofNames[i], ""));

                    }


                    // get details of each runner
                    for (int i = 0; i < nameAndIDs.Count(); i++)
                    {

                        GetRunnerDetails(credential, url, nameAndIDs[i].id, accessToken, nameAndIDs[i].name);

                    }


                    // print the details of each runner
                    for (int i = 0; i < theRunners.Count(); i++)
                    {
                        // as long as projects assigned is not blank, display
                        if (!theRunners[i].project.Equals(""))
                        {
                            Console.WriteLine("{0,5} | {1,20} | {2,50}", theRunners[i].id, theRunners[i].name, theRunners[i].project);
                        }



                    }


                }


            }

            catch (Exception ex)
            {

                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Could not get listing of GitLab runners. Exception: " + ex.ToString());
                Console.WriteLine("");


            }

        }


        // get the details for a runner based on ID
        public static void GetRunnerDetails(string credential, string url, string id, string accessToken, string name)
        {

            try
            {

                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;


                // perform request to get runner details by ID
                var webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/api/v4/runners/" + id);
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



                    // get the response 
                    var httpResponse = (HttpWebResponse)webRequest.GetResponse();
                    var streamReader = new StreamReader(httpResponse.GetResponseStream());
                    string result = streamReader.ReadToEnd();

                    // get the url to the repo
                    string[] splitResults = result.Split(',');
                    for (int i = 0; i < splitResults.Length; i++)
                    {
                        if (splitResults[i].Contains("http_url_to_repo"))
                        {

                            string theUrl = splitResults[i].Replace("\"http_url_to_repo\":\"", "");
                            theUrl = theUrl.Replace("\"", "");
                            theRunners.Add(new aRunner(id, name, theUrl));

                        }
                    }

                }


            }

            catch (Exception ex)
            {

                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Could not get listing of GitLab runners. Exception: " + ex.ToString());
                Console.WriteLine("");


            }


        }


    }


}