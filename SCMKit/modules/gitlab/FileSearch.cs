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


    // custom class to handle the JSON output of file search result
    public class fileSearchResult
    {

        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string path { get; set; }
        public string mode { get; set; }


        public fileSearchResult()
        {

        }

    }

    class FileSearch
    {

        public static async Task execute(string credential, string url, string options, string system)
        {

            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("searchfile", credential, url, options, system));

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

                // perform the search for files with search term against each project using the project search API
                foreach (string projectToSearch in listOfProjectIds)
                {
                    var webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/api/v4/projects/" + projectToSearch + "/repository/tree?recursive=true&per_page=100");
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
                        List<fileSearchResult> searchResults = JsonConvert.DeserializeObject<List<fileSearchResult>>(content);
                        foreach (fileSearchResult item in searchResults)
                        {
                            if (item.name.ToLower().Contains(options.ToLower()))
                            {
                                if (item.type.ToLower().Equals("blob"))
                                {
                                    Console.WriteLine("\n[>] URL: " + projectMapping[projectToSearch] + "/" + item.path);
                                    searchMatchCount++;
                                }
                            }


                        }

                    }
                }

                Console.WriteLine("");
                Console.WriteLine("Total number of items matching file search: " + searchMatchCount);

            }

            catch (Exception ex)
            {

                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Could not perform file search for search term given. Exception: " + ex.ToString());
                Console.WriteLine("");


            }

        }
    }
}
