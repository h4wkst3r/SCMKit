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
    class RepoSearch
    {
        private static int searchMatchCount = 0;

        public static async Task execute(string credential, string url, string options, string system)
        {

            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("searchrepo", credential, url, options, system));



            try
            {

                await library.Utils.HeartbeatRequest(url);

                // create table header
                string tableHeader = string.Format("{0,40} | {1,10} | {2,50}", "Name", "Visibility", "URL");
                Console.WriteLine(tableHeader);
                Console.WriteLine(new String('-', tableHeader.Length));


                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                string accessToken = "";


                // if username/password auth being used, get the access token first. this is needed for subsequent API requests
                if (credential.Contains(":"))
                {

                    accessToken = library.GitLabUtils.GetAccessToken(credential, url);

                }


                // proceed with repo search
                var webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/api/v4/search?scope=projects&search=" + options);
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

                    // get web response
                    WebResponse myWebResponse = await webRequest.GetResponseAsync();
                    string result;
                    var reader = new StreamReader(myWebResponse.GetResponseStream());
                    result = reader.ReadToEnd();


                    // get all instances of the id
                    IEnumerable<int> startingIndexesID = AllIndexesOf(result, "[{\"id\":", ",{\"id\":");
                    IEnumerable<int> endingIndexesID = library.Utils.AllIndexesOf(result, "\"description\":");

                    List<string> listOfIDs = new List<string>();

                    for (int i = 0; i < startingIndexesID.Count(); i++)
                    {

                        string theID = "";
                        theID = result.Substring(startingIndexesID.ElementAt(i) + "[{\"id\":".Length, endingIndexesID.ElementAt(i) - startingIndexesID.ElementAt(i) - "[{\"id\":".Length);
                        theID = theID.Replace(",", "");
                        theID = theID.Replace(" ", "");
                        listOfIDs.Add(theID);

                    }


                    // get all instances of the name
                    IEnumerable<int> startingIndexesName = library.Utils.AllIndexesOf(result, "\"name_with_namespace\":");
                    IEnumerable<int> endingIndexesName = library.Utils.AllIndexesOf(result, "\"path_with_namespace\":");

                    List<string> listOfNames = new List<string>();

                    for (int i = 0; i < startingIndexesName.Count(); i++)
                    {

                        string theName = "";
                        theName = result.Substring(startingIndexesName.ElementAt(i) + "\"name_with_namespace\":".Length, endingIndexesName.ElementAt(i) - startingIndexesName.ElementAt(i) - "\"name_with_namespace\":".Length);
                        string[] splitNameArray = theName.Split(',');
                        theName = splitNameArray[0];
                        theName = theName.Replace("\"", "");
                        string[] splitAgain = theName.Split('/');
                        theName = splitAgain[1];
                        theName.TrimStart();
                        listOfNames.Add(theName);

                    }


                    // get all instances of the URL
                    IEnumerable<int> startingIndexesURL = library.Utils.AllIndexesOf(result, "\"http_url_to_repo\":");
                    IEnumerable<int> endingIndexesURL = library.Utils.AllIndexesOf(result, "\"readme_url\":");

                    List<string> listOfURLs = new List<string>();

                    for (int i = 0; i < startingIndexesURL.Count(); i++)
                    {
                        string theURL = "";
                        theURL = result.Substring(startingIndexesURL.ElementAt(i) + "\"http_url_to_repo\":".Length, endingIndexesURL.ElementAt(i) - startingIndexesURL.ElementAt(i) - "\"http_url_to_repo\":".Length);
                        string[] splitURLArray = theURL.Split(',');
                        theURL = splitURLArray[0];
                        theURL = theURL.Replace("\"", "");
                        listOfURLs.Add(theURL);
                    }


                    // print the results
                    for (int i = 0; i < listOfIDs.Count; i++)
                    {
                        string visibility = await library.GitLabUtils.GetGitLabProjectVisibility(credential, accessToken, listOfIDs[i], url);
                        Console.WriteLine("{0,40} | {1,10} | {2,50}", listOfNames[i], visibility, listOfURLs[i]);
                        searchMatchCount++;
                    }


                    string nextPage = "";

                    // determine if there are more results and subsequent requests need made
                    for (int i = 0; i < myWebResponse.Headers.Count; ++i)
                    {
                        if (myWebResponse.Headers.Keys[i].ToString().ToLower().Equals("x-next-page") && myWebResponse.Headers.GetValues(i).Length == 1)
                        {
                            nextPage = myWebResponse.Headers.GetValues(i)[0];

                        }

                    }

                    // if there are more pages, then make subsequent requests
                    if (!nextPage.Equals(""))
                    {
                        await makeSubsequentRequestAsync(credential, url, options, accessToken, nextPage);
                    }

                }


                Console.WriteLine("");
                Console.WriteLine("Total number of items matching repo search: " + searchMatchCount);

            }

            catch (Exception ex)
            {

                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Could not perform repo search for search term given. Exception: " + ex.ToString());
                Console.WriteLine("");


            }

        }



        /**
         * method to get all indexes where a value exists in a string. this one is specialized for ID field of GitLab parsing
         * 
         * */
        public static int[] AllIndexesOf(string str, string substrOne, string substrTwo, bool ignoreCase = false)
        {
            if (string.IsNullOrWhiteSpace(str) ||
                string.IsNullOrWhiteSpace(substrOne) ||
                string.IsNullOrWhiteSpace(substrTwo))
            {
                throw new ArgumentException("String or substring is not specified.");
            }

            var indexes = new List<int>();
            int index = 0;

            while ((index = str.IndexOf(substrOne, index, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)) != -1)
            {
                indexes.Add(index++);

            }

            int indexTwo = 0;

            while ((indexTwo = str.IndexOf(substrTwo, indexTwo, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)) != -1)
            {
                indexes.Add(indexTwo++);

            }

            return indexes.ToArray();
        }



        // this is just placeholder for making more than 1 request due to paging of results. will have better global solution at some point than this band-aid
        public static async Task makeSubsequentRequestAsync(string credential, string url, string options, string accessToken, string nextPage)
        {

            try
            {


                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // proceed with repo search
                var webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/api/v4/search?scope=projects&page=" + nextPage + "&search=" + options);
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

                    // get web response
                    WebResponse myWebResponse = await webRequest.GetResponseAsync();
                    string result;
                    var reader = new StreamReader(myWebResponse.GetResponseStream());
                    result = reader.ReadToEnd();


                    // get all instances of the id
                    IEnumerable<int> startingIndexesID = AllIndexesOf(result, "[{\"id\":", ",{\"id\":");
                    IEnumerable<int> endingIndexesID = library.Utils.AllIndexesOf(result, "\"description\":");

                    List<string> listOfIDs = new List<string>();

                    for (int i = 0; i < startingIndexesID.Count(); i++)
                    {

                        string theID = "";
                        theID = result.Substring(startingIndexesID.ElementAt(i) + "[{\"id\":".Length, endingIndexesID.ElementAt(i) - startingIndexesID.ElementAt(i) - "[{\"id\":".Length);
                        theID = theID.Replace(",", "");
                        theID = theID.Replace(" ", "");
                        listOfIDs.Add(theID);

                    }


                    // get all instances of the name
                    IEnumerable<int> startingIndexesName = library.Utils.AllIndexesOf(result, "\"name_with_namespace\":");
                    IEnumerable<int> endingIndexesName = library.Utils.AllIndexesOf(result, "\"path_with_namespace\":");

                    List<string> listOfNames = new List<string>();

                    for (int i = 0; i < startingIndexesName.Count(); i++)
                    {

                        string theName = "";
                        theName = result.Substring(startingIndexesName.ElementAt(i) + "\"name_with_namespace\":".Length, endingIndexesName.ElementAt(i) - startingIndexesName.ElementAt(i) - "\"name_with_namespace\":".Length);
                        string[] splitNameArray = theName.Split(',');
                        theName = splitNameArray[0];
                        theName = theName.Replace("\"", "");
                        string[] splitAgain = theName.Split('/');
                        theName = splitAgain[1];
                        theName.TrimStart();
                        listOfNames.Add(theName);

                    }


                    // get all instances of the URL
                    IEnumerable<int> startingIndexesURL = library.Utils.AllIndexesOf(result, "\"http_url_to_repo\":");
                    IEnumerable<int> endingIndexesURL = library.Utils.AllIndexesOf(result, "\"readme_url\":");

                    List<string> listOfURLs = new List<string>();

                    for (int i = 0; i < startingIndexesURL.Count(); i++)
                    {
                        string theURL = "";
                        theURL = result.Substring(startingIndexesURL.ElementAt(i) + "\"http_url_to_repo\":".Length, endingIndexesURL.ElementAt(i) - startingIndexesURL.ElementAt(i) - "\"http_url_to_repo\":".Length);
                        string[] splitURLArray = theURL.Split(',');
                        theURL = splitURLArray[0];
                        theURL = theURL.Replace("\"", "");
                        listOfURLs.Add(theURL);
                    }


                    // print the results
                    for (int i = 0; i < listOfIDs.Count; i++)
                    {
                        string visibility = await library.GitLabUtils.GetGitLabProjectVisibility(credential, accessToken, listOfIDs[i], url);
                        Console.WriteLine("{0,40} | {1,10} | {2,50}", listOfNames[i], visibility, listOfURLs[i]);
                        searchMatchCount++;
                    }



                    // determine if there are more results and subsequent requests need made
                    for (int i = 0; i < myWebResponse.Headers.Count; ++i)
                    {
                        if (myWebResponse.Headers.Keys[i].ToString().ToLower().Equals("x-next-page") && myWebResponse.Headers.GetValues(i).Length == 1)
                        {
                            nextPage = myWebResponse.Headers.GetValues(i)[0];

                        }

                    }


                    // if there are more pages, then make subsequent requests
                    if (!nextPage.Equals(""))
                    {
                        await makeSubsequentRequestAsync(credential, url, options, accessToken, nextPage);
                    }


                }

            }

            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Could not perform repo search. Exception: " + ex.ToString());
                Console.WriteLine("");
            }


        }



    }
}
