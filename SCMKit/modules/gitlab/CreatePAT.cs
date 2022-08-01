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
    class CreatePAT
    {

        public static async Task execute(string credential, string url, string options, string system)
        {

            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("createpat", credential, url, options, system));

            // dictionary to hold lookup table user ID's and usernames
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

                // proceed with request to create PAT for the user given
                string accessToken = "";


                // if username/password auth being used, get the access token first. this is needed for subsequent API requests
                if (credential.Contains(":"))
                {

                    accessToken = library.GitLabUtils.GetAccessToken(credential, url);

                }

                string theUserID = userMapping[options.ToLower()];

                // perform request to create personal access token
                var webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/api/v4/users/" + theUserID + "/personal_access_tokens");
                if (webRequest != null)
                {
                    // set header values
                    webRequest.Method = "POST";
                    webRequest.ContentType = "application/x-www-form-urlencoded";
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

                        // create random token name
                        Random rd = new Random();
                        const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
                        char[] chars = new char[5];

                        for (int i = 0; i < 5; i++)
                        {
                            chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
                        }
                        string personalAccessTokenName = new string(chars);
                        personalAccessTokenName = "SCMKIT-" + personalAccessTokenName;


                        string body = "name=" + personalAccessTokenName + "&scopes[]=api&scopes[]=read_repository&scopes[]=write_repository";
                        streamWriter.Write(body);
                    }

                    // get the response and the access token
                    var httpResponse = (HttpWebResponse)webRequest.GetResponse();
                    var streamReader = new StreamReader(httpResponse.GetResponseStream());
                    string result = streamReader.ReadToEnd();

                    // create table header
                    string tableHeader = string.Format("{0,5} | {1,12} | {2,30}", "ID", "Name", "Token");
                    Console.WriteLine(tableHeader);
                    Console.WriteLine(new String('-', tableHeader.Length));


                    int startIndexTokenID = result.IndexOf("\"id\":");
                    int endIndexTokenID = result.IndexOf("\"name\":");
                    string tokenID = result.Substring(startIndexTokenID + "\"id\":".Length, endIndexTokenID - startIndexTokenID - "\"name\"".Length);

                    int startIndexTokenName = result.IndexOf("\"name\":");
                    int endIndexTokenName = result.IndexOf("\"revoked\":");
                    string tokenName = result.Substring(startIndexTokenName + "\"name\":".Length, endIndexTokenName - startIndexTokenName - "\"revoked\"".Length).Replace("\"", "");

                    int startIndexTokenContent = result.IndexOf("\"token\":");
                    int endIndexTokenContent = result.IndexOf("\"}");
                    string tokenContent = result.Substring(startIndexTokenContent + "\"token\":".Length, endIndexTokenContent - startIndexTokenContent - "\"token\":".Length).Replace("\"", "");


                    Console.WriteLine("{0,5} | {1,12} | {2,30}", tokenID, tokenName, tokenContent);


                    Console.WriteLine("");
                    Console.WriteLine("[+] SUCCESS: The " + options + " user personal access token was successfully added.");
                    Console.WriteLine("");

                }


            }

            catch (KeyNotFoundException ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: User provided does not exist to create personal access token for. Exception: " + ex.ToString());
                Console.WriteLine("");
            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Could not create personal access token for user. Exception: " + ex.ToString());
                Console.WriteLine("");
            }

        }
    }
}
