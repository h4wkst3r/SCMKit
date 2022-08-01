using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using GitLabApiClient;

namespace SCMKit.library
{
    class GitLabUtils
    {


       /**
       * Get the GitLab project visibilty based on project ID
       * 
       * */
        public static async Task<string> GetGitLabProjectVisibility(string credential, string accessToken, string projID, string url)
        {
            string visibility = "public";

            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;


                // web request to get details of a project
                var webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/api/v4/projects/" + projID);
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

                    // parse response to get visibility
                    int startingIndex = content.IndexOf("\"visibility\":");
                    int endingIndex = content.IndexOf("\"owner\":{");
                    visibility = content.Substring(startingIndex + "\"visibility\":".Length, endingIndex - startingIndex - "\"visibility\":".Length);
                    visibility = visibility.Replace("\"", "");
                    visibility = visibility.Replace(",", "");


                }
            }

            catch (Exception ex)
            {
                return visibility;
            }

            return visibility;


        }


       /**
       * Authenticate to GitLab
       * 
       * */
        public static async Task<GitLabClient> AuthToGitLabAsync(string credentials, string url)
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

                GitLabClient client = null;
                Uri uri = new Uri(url);

                // if username/password auth being used
                if (credentials.Contains(":"))
                {
                    string[] theCreds = credentials.Split(':');
                    client = new GitLabClient(uri.ToString());
                    await client.LoginAsync(theCreds[0], theCreds[1]);
                }

                // if token auth being used
                else
                {
                    client = new GitLabClient(uri.ToString(), credentials);
                }


                return client;

            }

            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Could not authenticate to URL with credentials provided. Exception: " + ex.ToString());
                Console.WriteLine("");
                return null;
            }

        }



        /**
        * get access token for GitLab based on username/password given
        * 
        * */
        public static string GetAccessToken(string credential, string url)
        {

            string accessToken = "";

            // get personal access tokens for user
            var webRequestOAuthToken = System.Net.WebRequest.Create(url + "/oauth/token");
            if (webRequestOAuthToken != null)
            {
                // set header values
                webRequestOAuthToken.Method = "POST";
                webRequestOAuthToken.ContentType = "application/json";


                // set body and sent request
                using (var streamWriter = new StreamWriter(webRequestOAuthToken.GetRequestStream()))
                {
                    string[] theCreds = credential.Split(':');
                    string json = "{\"grant_type\":\"password\",\"scope\":\"api\",\"username\":\"" + theCreds[0] + "\",\"password\":\"" + theCreds[1] + "\"}";

                    streamWriter.Write(json);
                }

                // get the response and the access token
                var httpResponse = (HttpWebResponse)webRequestOAuthToken.GetResponse();
                var streamReader = new StreamReader(httpResponse.GetResponseStream());
                string result = streamReader.ReadToEnd();
                int startIndex = result.IndexOf("\"access_token\":\"");
                int endIndex = result.IndexOf("\",\"token_type\":");
                accessToken = result.Substring(startIndex + "\"access_token\":\"".Length, endIndex - startIndex - "\", \"token_type\":".Length);

            }

            return accessToken;

        }


    }

}
