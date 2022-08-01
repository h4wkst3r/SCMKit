using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace SCMKit.modules.gitlab
{

    // custom class to handle the JSON output of snippet result
    public class snippetResult
    {

        public string id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string visibility { get; set; }
        public string updatedAt { get; set; }
        public string createdAt { get; set; }
        public string project_id { get; set; }
        public string web_url { get; set; }
        public string raw_url { get; set; }
        public string ssh_url_to_repo { get; set; }
        public string http_url_to_repo { get; set; }
        public Dictionary<string, string> author { get; set; }
        public string fileName { get; set; }
        public Dictionary<string, string>[] files { get; set; }

        public snippetResult()
        {

        }

    }


    class SnippetList
    {


        public static async Task execute(string credential, string url, string options, string system)
        {


            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("listsnippet", credential, url, options, system));


            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                await library.Utils.HeartbeatRequest(url);

                string accessToken = "";


                // if username/password auth being used, get the access token first. this is needed for subsequent API requests
                if (credential.Contains(":"))
                {

                    accessToken = library.GitLabUtils.GetAccessToken(credential, url);

                }


                // create table header
                string tableHeader = string.Format("{0,20} | {1,70}", "Title", "Raw URL");
                Console.WriteLine(tableHeader);
                Console.WriteLine(new String('-', tableHeader.Length));


                // get snippets for user
                var webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/api/v4/snippets");
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
                    string content;
                    var reader = new StreamReader(myWebResponse.GetResponseStream());
                    content = reader.ReadToEnd();

                    // parse the JSON output and display results
                    List<snippetResult> snippetResults = JsonConvert.DeserializeObject<List<snippetResult>>(content);
                    foreach (snippetResult item in snippetResults)
                    {

                        Console.WriteLine("{0,20} | {1,70}", item.title, item.raw_url);

                    }

                }


            }

            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Could not retrieve listing of snippets for current user. Exception: " + ex.ToString());
                Console.WriteLine("");
            }


        }


    }

}