using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using GitLabApiClient;


namespace SCMKit.modules.gitlab
{

    // custom class to handle the JSON output of search result
    public class gitlabSearchResult
    {

        public string basename { get; set; }
        public string data { get; set; }
        public string path { get; set; }
        public string filename { get; set; }
        public string id { get; set; }
        public string reference { get; set; }
        public int startLine { get; set; }
        public int project_id { get; set; }

        public gitlabSearchResult()
        {

        }

    }


    class CodeSearch
    {

        public static async Task execute(string credential, string url, string options, string system)
        {

            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("searchcode", credential, url, options, system));

            // lists to hold project ID's and mappings
            List<string> listOfProjectIds = new List<string>();
            Dictionary<string, string> projectMapping = new Dictionary<string, string>();


            try
            {

                await library.Utils.HeartbeatRequest(url);

                // auth to GitLab and get list of all projects
                Task<GitLabClient> authTask = library.GitLabUtils.AuthToGitLabAsync(credential, url);
                GitLabClient client = authTask.Result;
                var projects = await client.Projects.GetAsync();

                foreach (var project in projects)
                {

                    listOfProjectIds.Add(project.Id.ToString());
                    projectMapping.Add(project.Id.ToString(), project.WebUrl);

                }


                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // proceed with performing the search since we have all the project ID's and mappings
                string accessToken = "";


                // if username/password auth being used, get the access token first. this is needed for subsequent API requests
                if (credential.Contains(":"))
                {

                    accessToken = library.GitLabUtils.GetAccessToken(credential, url);

                }


                int searchMatchCount = 0;

                // perform the search for code with search term against each project using the project search API
                foreach (string projectToSearch in listOfProjectIds)
                {

                    var webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/api/v4/projects/" + projectToSearch + "/search?scope=blobs&search=" + options);
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
                        List<gitlabSearchResult> searchResults = JsonConvert.DeserializeObject<List<gitlabSearchResult>>(content);
                        foreach (gitlabSearchResult item in searchResults)
                        {

                            Console.WriteLine("\n[>] URL: " + projectMapping[projectToSearch] + "/" + item.path);

                            // get the actual line where the match happened
                            string[] lines = item.data.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                            foreach (string line in lines)
                            {
                                if (line.ToLower().Contains(options.ToLower()))
                                {
                                    searchMatchCount++;
                                    Console.WriteLine("    |_ " + line.Trim());

                                }
                            }

                        }

                    }
                }

                Console.WriteLine("");
                Console.WriteLine("Total number of items matching code search: " + searchMatchCount);

            }

            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Could not perform code search for search term given. Exception: " + ex.ToString());
                Console.WriteLine("");
            }

        }
    }

}
