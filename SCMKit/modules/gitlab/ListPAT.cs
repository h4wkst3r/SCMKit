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

    // custom class to handle the JSON output of personal access token
    public class patResult
    {

        public int id { get; set; }
        public string name { get; set; }
        public bool revoked { get; set; }
        public string createdAt { get; set; }
        public string[] scopes { get; set; }
        public int user_id { get; set; }
        public bool active { get; set; }
        public string expires_at { get; set; }


        public patResult()
        {

        }

    }
    class ListPAT
    {

        public static async Task execute(string credential, string url, string options, string system)
        {



            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("listpat", credential, url, options, system));

            // dicationary to hold lookup table user ID's and usernames
            Dictionary<string, string> userMapping = new Dictionary<string, string>();

            try
            {

                await library.Utils.HeartbeatRequest(url);

                // auth to GitLab and get list of all users
                Task<GitLabClient> authTask = library.GitLabUtils.AuthToGitLabAsync(credential, url);
                GitLabClient client = authTask.Result;
                var users = await client.Users.GetAsync();

                // add associated user and user id to the dictionary for subsequent requests
                foreach (var user in users)
                {
                    userMapping.Add(user.Username, user.Id.ToString());
                }


                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;


                // proceed with request to list PATs for the user given
                string accessToken = "";


                // if username/password auth being used, get the access token first. this is needed for subsequent API requests
                if (credential.Contains(":"))
                {

                    accessToken = library.GitLabUtils.GetAccessToken(credential, url);

                }

                string theUserID = userMapping[options.ToLower()];


                // get listing of all personal access tokens for the user
                // create table header
                string tableHeader = string.Format("{0,5} | {1,20} | {2,10} | {3,50}", "ID", "Name", "Active?", "Scopes");
                Console.WriteLine(tableHeader);
                Console.WriteLine(new String('-', tableHeader.Length));


                // get personal access tokens for user
                var webRequestPersonalAccessTokens = (HttpWebRequest)System.Net.WebRequest.Create(url + "/api/v4/personal_access_tokens?user_id=" + theUserID);
                if (webRequestPersonalAccessTokens != null)
                {
                    // set header values
                    webRequestPersonalAccessTokens.Method = "GET";
                    webRequestPersonalAccessTokens.ContentType = "application/json";
                    webRequestPersonalAccessTokens.UserAgent = "SCMKIT-5dc493ada400c79dd318abbe770dac7c";

                    // if we need to pass the token obtained from the username/password auth
                    if (credential.Contains(":"))
                    {
                        webRequestPersonalAccessTokens.Headers.Add("Authorization", "Bearer " + accessToken);
                    }
                    // if user just specified personal access token
                    else
                    {
                        webRequestPersonalAccessTokens.Headers.Add("PRIVATE-TOKEN", credential);
                    }

                    // get web response
                    WebResponse myWebResponsePersonalAccessToken = await webRequestPersonalAccessTokens.GetResponseAsync();
                    string contentPersonalAccessToken;
                    var readerPersonalAccessToken = new StreamReader(myWebResponsePersonalAccessToken.GetResponseStream());
                    contentPersonalAccessToken = readerPersonalAccessToken.ReadToEnd();

                    // if more than 1 page, we need to track what next URL will be
                    string nextURL = "";


                    // parse the JSON output and display result to retrieve the PAT
                    List<patResult> patResults = JsonConvert.DeserializeObject<List<patResult>>(contentPersonalAccessToken);
                    foreach (patResult item in patResults)
                    {

                        string scopesToList = "";
                        foreach (string a in item.scopes)
                        {
                            scopesToList += a + ", ";
                        }

                        scopesToList = scopesToList.Substring(0, scopesToList.Length - 2);


                        // only display an active PAT
                        if (item.active == true)
                        {
                            Console.WriteLine("{0,5} | {1,20} | {2,10} | {3,50}", item.id, item.name, item.active, scopesToList);
                        }

                    }


                    // figure out if there are more results due to paging and we need to send another request for the next page
                    for (int i = 0; i < myWebResponsePersonalAccessToken.Headers.Count; ++i)
                    {

                        string[] splitValues = myWebResponsePersonalAccessToken.Headers[i].Split(',');
                        foreach (string val in splitValues)
                        {

                            if (val.Contains("rel=\"next\""))
                            {
                                nextURL = val;
                                nextURL = nextURL.Replace(">; rel=\"next\"", "");
                                nextURL = nextURL.Replace("<", "");

                            }

                        }
                    }

                    // make subsequent requests if more than 1 page
                    if (!nextURL.Equals(""))
                    {
                        await makeSubsequentRequestAsync(credential, nextURL, options, accessToken);
                    }


                }


            }

            catch (KeyNotFoundException ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: User provided does not exist to list personal acccess tokens. Exception: " + ex.ToString());
                Console.WriteLine("");
            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Could not list personal access token for user. Exception: " + ex.ToString());
                Console.WriteLine("");
            }

        }

        // this is just placeholder for making more than 1 request due to paging of results. will have better global solution at some point than this band-aid
        public static async Task makeSubsequentRequestAsync(string credential, string url, string options, string token)
        {

            try
            {

                string accessToken = token;


                // get personal access tokens for user
                var webRequestPersonalAccessTokens = (HttpWebRequest)System.Net.WebRequest.Create(url);
                if (webRequestPersonalAccessTokens != null)
                {
                    // set header values
                    webRequestPersonalAccessTokens.Method = "GET";
                    webRequestPersonalAccessTokens.ContentType = "application/json";
                    webRequestPersonalAccessTokens.UserAgent = "SCMKIT-5dc493ada400c79dd318abbe770dac7c";

                    // if we need to pass the token obtained from the username/password auth
                    if (credential.Contains(":"))
                    {
                        webRequestPersonalAccessTokens.Headers.Add("Authorization", "Bearer " + accessToken);
                    }
                    // if user just specified personal access token
                    else
                    {
                        webRequestPersonalAccessTokens.Headers.Add("PRIVATE-TOKEN", credential);
                    }

                    // get web response
                    WebResponse myWebResponsePersonalAccessToken = await webRequestPersonalAccessTokens.GetResponseAsync();
                    string contentPersonalAccessToken;
                    var readerPersonalAccessToken = new StreamReader(myWebResponsePersonalAccessToken.GetResponseStream());
                    contentPersonalAccessToken = readerPersonalAccessToken.ReadToEnd();


                    string nextURL = "";



                    // parse the JSON output and display result to retrieve the personal access token id(s)
                    List<patResult> patResults = JsonConvert.DeserializeObject<List<patResult>>(contentPersonalAccessToken);
                    foreach (patResult item in patResults)
                    {

                        string scopesToList = "";
                        foreach (string a in item.scopes)
                        {
                            scopesToList += a + ", ";
                        }

                        scopesToList = scopesToList.Substring(0, scopesToList.Length - 2);

                        // only display an active PAT
                        if (item.active == true)
                        {
                            Console.WriteLine("{0,5} | {1,20} | {2,10} | {3,50}", item.id, item.name, item.active, scopesToList);
                        }

                    }


                    // figure out if there are more results due to paging and we need to send another request for the next page
                    for (int i = 0; i < myWebResponsePersonalAccessToken.Headers.Count; ++i)
                    {

                        string[] splitValues = myWebResponsePersonalAccessToken.Headers[i].Split(',');
                        foreach (string val in splitValues)
                        {

                            if (val.Contains("rel=\"next\""))
                            {
                                nextURL = val;
                                nextURL = nextURL.Replace(">; rel=\"next\"", "");
                                nextURL = nextURL.Substring(1, nextURL.Length - 1);
                                nextURL = nextURL.Replace("<", "");

                            }

                        }
                    }

                    // make subsequent requests if more pages
                    if (!nextURL.Equals(""))
                    {
                        await makeSubsequentRequestAsync(credential, nextURL, options, accessToken);
                    }


                }

            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: User provided does not exist to list personal acccess tokens. Exception: " + ex.ToString());
                Console.WriteLine("");
            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Could not list personal access token for user. Exception: " + ex.ToString());
                Console.WriteLine("");
            }


        }



    }
}
