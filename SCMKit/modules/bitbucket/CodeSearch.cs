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


    // custom class to handle URL results
    public class URLResult
    {

        public URLResult(string link, string matchingLine, string fileName)
        {
            this.url = link;
            this.matchingLine = matchingLine;
            this.fileName = fileName;
        }

        public string url { get; set; }
        public string matchingLine { get; set; }
        public string fileName { get; set; }

    } // end URLResult class


    class CodeSearch
    {

        // dictionary to hold the list of repos and their URLs
        private static List<URLResult> urlResults = new List<URLResult>();

        private static int matchCount = 0;

        public static async Task execute(string credential, string url, string options, string system)
        {

            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("searchcode", credential, url, options, system));

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


                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                await library.Utils.HeartbeatRequest(url);

                // web request to search via rest API
                var webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/rest/search/latest/search");
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
                        string json = "{\"query\":\"" + options + "\",\"entities\":{\"code\":{}},\"limits\":{\"primary\":100,\"secondary\":100}}";
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

                        // parse the JSON output and display results
                        JsonTextReader jsonResult = new JsonTextReader(new StringReader(content));

                        string matchingLine = "";
                        string propName = "";
                        string projKey = "";
                        string repoName = "";
                        string fileName = "";
                        string nextPageStart = "";
                        string isLastPage = "";

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
                                    // add the match to the list
                                    if (!matchingLine.Equals("") && !fileName.Equals(""))
                                    {
                                        bool alreadyExists = false;
                                        string fullURL = await library.BitbucketUtils.GetFullBitbucketRepoURLAsync(credential, sessID, projKey, repoName, url);

                                        URLResult singleResults = new URLResult(fullURL, matchingLine, fileName);

                                        // only add if not already found before
                                        foreach (URLResult item in urlResults)
                                        {
                                            if (item.fileName.Equals(fileName) && item.matchingLine.Equals(matchingLine) && item.url.Equals(fullURL))
                                            {

                                                alreadyExists = true;
                                            }
                                        }

                                        if (!alreadyExists)
                                        {
                                            urlResults.Add(singleResults);

                                        }

                                    }
                                    break;
                                case "PropertyName":
                                    propName = jsonResult.Value.ToString();
                                    break;
                                case "String":

                                    // get actual values
                                    if (propName.ToLower().Equals("text"))
                                    {

                                        string filteredResult = jsonResult.Value.ToString().ToLower();
                                        filteredResult = filteredResult.Replace("<em>", "");
                                        filteredResult = filteredResult.Replace("</em>", "");


                                        if (filteredResult.Contains(options.ToLower()))
                                        {
                                            matchingLine = jsonResult.Value.ToString();
                                        }
                                    }
                                    if (propName.ToLower().Equals("key"))
                                    {
                                        projKey = jsonResult.Value.ToString();
                                    }
                                    if (propName.ToLower().Equals("slug"))
                                    {
                                        repoName = jsonResult.Value.ToString();
                                    }
                                    if (propName.ToLower().Equals("file"))
                                    {
                                        fileName = jsonResult.Value.ToString();
                                    }
                                    break;
                                case "Integer":
                                    if (propName.ToLower().Equals("nextstart"))
                                    {
                                        nextPageStart = jsonResult.Value.ToString();

                                    }
                                    break;
                                case "Boolean":
                                    if (propName.ToLower().Equals("islastpage"))
                                    {
                                        isLastPage = jsonResult.Value.ToString();

                                    }
                                    break;
                                default:
                                    break;


                            }

                        }

                        // iterate through the dictionary of matching lines and print them
                        foreach (var item in urlResults)
                        {

                            string theMatch = item.matchingLine;
                            theMatch = theMatch.Replace("<em>", "");
                            theMatch = theMatch.Replace("</em>", "");
                            theMatch = WebUtility.HtmlDecode(theMatch);
                            Console.WriteLine("\n[>] REPO: " + item.url);
                            Console.WriteLine("    [>] FILE: " + item.fileName);
                            Console.WriteLine("            |_ " + theMatch);
                            matchCount++;

                        }

                        // if there are more pages, then make subsequent requests
                        if (!nextPageStart.Equals("") && isLastPage.Equals("False"))
                        {
                            await makeSubsequentRequestAsync(credential, url, options, sessID, nextPageStart);
                        }



                        Console.WriteLine("");
                        Console.WriteLine("Total matching results: " + matchCount);

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
                Console.WriteLine("[-] ERROR: Could not perform code search. Exception: " + ex.ToString());
                Console.WriteLine("");
            }

        }

        // this is just placeholder for making more than 1 request due to paging of results. will have better global solution at some point than this band-aid
        public static async Task makeSubsequentRequestAsync(string credential, string url, string options, string sessID, string nextPage)
        {

            try
            {

                urlResults.Clear();

                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // web request to search via rest API
                var webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/rest/search/latest/search");
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
                        string json = "{\"query\":\"" + options + "\",\"entities\":{\"code\":{\"start\":" + nextPage + "}},\"limits\":{\"primary\":100,\"secondary\":100}}";
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

                        // parse the JSON output and display results
                        JsonTextReader jsonResult = new JsonTextReader(new StringReader(content));

                        string matchingLine = "";
                        string propName = "";
                        string projKey = "";
                        string repoName = "";
                        string fileName = "";
                        string nextPageStart = "";
                        string isLastPage = "";

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
                                    // add the match to the list
                                    if (!matchingLine.Equals("") && !fileName.Equals(""))
                                    {
                                        bool alreadyExists = false;
                                        string fullURL = await library.BitbucketUtils.GetFullBitbucketRepoURLAsync(credential, sessID, projKey, repoName, url);

                                        URLResult singleResults = new URLResult(fullURL, matchingLine, fileName);

                                        // only add if not already found before
                                        foreach (URLResult item in urlResults)
                                        {
                                            if (item.fileName.Equals(fileName) && item.matchingLine.Equals(matchingLine) && item.url.Equals(fullURL))
                                            {

                                                alreadyExists = true;
                                            }
                                        }

                                        if (!alreadyExists)
                                        {
                                            urlResults.Add(singleResults);

                                        }

                                    }
                                    break;
                                case "PropertyName":
                                    propName = jsonResult.Value.ToString();
                                    break;
                                case "String":

                                    // get actual values
                                    if (propName.ToLower().Equals("text"))
                                    {

                                        string filteredResult = jsonResult.Value.ToString().ToLower();
                                        filteredResult = filteredResult.Replace("<em>", "");
                                        filteredResult = filteredResult.Replace("</em>", "");


                                        if (filteredResult.Contains(options.ToLower()))
                                        {
                                            matchingLine = jsonResult.Value.ToString();
                                        }
                                    }
                                    if (propName.ToLower().Equals("key"))
                                    {
                                        projKey = jsonResult.Value.ToString();
                                    }
                                    if (propName.ToLower().Equals("slug"))
                                    {
                                        repoName = jsonResult.Value.ToString();
                                    }
                                    if (propName.ToLower().Equals("file"))
                                    {
                                        fileName = jsonResult.Value.ToString();
                                    }
                                    break;
                                case "Integer":
                                    if (propName.ToLower().Equals("nextstart"))
                                    {
                                        nextPageStart = jsonResult.Value.ToString();

                                    }
                                    break;
                                case "Boolean":
                                    if (propName.ToLower().Equals("islastpage"))
                                    {
                                        isLastPage = jsonResult.Value.ToString();

                                    }
                                    break;
                                default:
                                    break;


                            }

                        }

                        // iterate through the dictionary of matching lines and print them
                        foreach (var item in urlResults)
                        {

                            string theMatch = item.matchingLine;
                            theMatch = theMatch.Replace("<em>", "");
                            theMatch = theMatch.Replace("</em>", "");
                            theMatch = WebUtility.HtmlDecode(theMatch);
                            Console.WriteLine("\n[>] REPO: " + item.url);
                            Console.WriteLine("    [>] FILE: " + item.fileName);
                            Console.WriteLine("            |_ " + theMatch);
                            matchCount++;

                        }

                        // if there are more pages, then make subsequent requests
                        if (!nextPageStart.Equals("") && isLastPage.Equals("False"))
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
                Console.WriteLine("[-] ERROR: Could not perform code search. Exception: " + ex.ToString());
                Console.WriteLine("");
            }


        }


    }

}
