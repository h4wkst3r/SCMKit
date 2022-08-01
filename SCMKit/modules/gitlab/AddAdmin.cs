using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.IO;
using GitLabApiClient;

namespace SCMKit.modules.gitlab
{
    class AddAdmin
    {


        public static async Task execute(string credential, string url, string options, string system)
        {

            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("addadmin", credential, url, options, system));

            // dictionary to hold lookup table user ID's and usernames
            Dictionary<string, string> userMapping = new Dictionary<string, string>();

            try
            {

                // auth to GitLab and get list of all users
                Task<GitLabClient> authTask = library.GitLabUtils.AuthToGitLabAsync(credential, url);
                GitLabClient client = authTask.Result;
                var users = await client.Users.GetAsync();

                // add associated user and user id to the dictionary for subsequent requests
                foreach (var user in users)
                {
                    userMapping.Add(user.Username, user.Id.ToString());
                }



                // proceed with request to add admin permissions for the user given
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

                string theUserID = userMapping[options.ToLower()];

                // get snippets for user
                var webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/api/v4/users/" + theUserID);
                if (webRequest != null)
                {
                    // set header values
                    webRequest.Method = "PUT";
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


                    // set body and sent request
                    using (var streamWriter = new StreamWriter(webRequest.GetRequestStream()))
                    {
                        string json = "{\"admin\":\"true\"}";
                        streamWriter.Write(json);
                    }

                    // get the response and the access token
                    var httpResponse = (HttpWebResponse)webRequest.GetResponse();
                    var streamReader = new StreamReader(httpResponse.GetResponseStream());
                    string result = streamReader.ReadToEnd();

                    Console.WriteLine("");
                    Console.WriteLine("[+] SUCCESS: The " + options + " user was successfully added to the admin role.");
                    Console.WriteLine("");


                }


            }

            catch (KeyNotFoundException ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: User provided does not exist to modify. Exception: " + ex.ToString());
                Console.WriteLine("");
            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Could not add user to admin role. Exception: " + ex.ToString());
                Console.WriteLine("");
            }

        }

    }

}
