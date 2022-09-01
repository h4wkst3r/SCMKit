using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Octokit;

namespace SCMKit.library
{
    class GitHubUtils
    {

       /**
       * authenticate to GitHub
       * 
       * */
        public static dynamic AuthToGitHub(string credentials, string url)
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

                GitHubClient client = null;
                Credentials auth = null;
                Uri uri = new Uri(url);

                client = new GitHubClient(new ProductHeaderValue("SCMKIT-5dc493ada400c79dd318abbe770dac7c"), uri);

                // if username/password auth being used
                if (credentials.Contains(":"))
                {
                    string[] theCreds = credentials.Split(':');
                    auth = new Credentials(theCreds[0], theCreds[1]);
                }

                // if token auth being used
                else
                {
                    auth = new Credentials(credentials);
                }

                client.Credentials = auth;

                return client;

            }

            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Could not authenticate to URL with credentials provided. Exception: " + ex.ToString());
                Console.WriteLine("");
                Environment.Exit(1);
                return null;
            }

        }

        public static async Task<string> callGitHubApiGet(string credential, string baseUrl, string path, string query)
        {
            string content = "";
            var webRequest = (HttpWebRequest)WebRequest.Create(baseUrl + path + query);
            // set header values
            webRequest.Method = "GET";
            webRequest.ContentType = "application/json";
            webRequest.UserAgent = "SCMKIT-5dc493ada400c79dd318abbe770dac7c";

            // if we need to pass the username/password
            if (credential.Contains(":"))
            {
                var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(credential));
                webRequest.Headers.Add("Authorization", "Basic " + base64EncodedAuthenticationString);
            }
            // if user just specified personal access token
            else
            {
                webRequest.Headers.Add("Authorization", "Bearer " + credential);
            }

            // get web response
            WebResponse response = await webRequest.GetResponseAsync();
            var reader = new StreamReader(response.GetResponseStream());
            content = reader.ReadToEnd();
            return content;
        }
    }
}
