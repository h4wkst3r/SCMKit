using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SCMKit.library
{
    class BitbucketUtils
    {


        /**
        * authenticate to Bitbucket to get SESSION ID if not using API auth
        * 
        * */
        public static string GetSessionID(string credentials, string url)
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

                string sessID = "";
                bool authValid = false;
                var webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/j_atl_security_check");

                if (webRequest != null)
                {
                    // set header values
                    webRequest.Method = "POST";
                    webRequest.ContentType = "application/x-www-form-urlencoded";
                    webRequest.UserAgent = "SCMKIT-5dc493ada400c79dd318abbe770dac7c";
                    webRequest.AllowAutoRedirect = false;
                    string[] theCreds = credentials.Split(':');


                    // set body and send request
                    using (var streamWriter = new StreamWriter(webRequest.GetRequestStream()))
                    {

                        string body = "j_username=" + theCreds[0] + "&j_password=" + theCreds[1] + "&_atl_remember_me=on&submit=Log+in";
                        streamWriter.Write(body);
                    }

                    // get the response and the Bitbucket session ID
                    var httpResponse = (HttpWebResponse)webRequest.GetResponse();
                    var streamReader = new StreamReader(httpResponse.GetResponseStream());
                    string result = streamReader.ReadToEnd();

                    for (int i = 0; i < httpResponse.Headers.Count; ++i)
                    {

                        string[] splitValues = httpResponse.Headers[i].Split(',');
                        foreach (string val in splitValues)
                        {

                            // this header is set only if auth request is valid
                            if (val.Contains("_atl_bitbucket_remember_me"))
                            {
                                authValid = true;
                            }

                            // get the bitbucket session ID
                            if (val.Contains("BITBUCKETSESSIONID=") && authValid)
                            {

                                string[] splitValsAgain = val.Split(';');
                                sessID = splitValsAgain[0];
                                sessID = sessID.Replace("BITBUCKETSESSIONID=", "");

                            }
                        }


                    }

                }

                // if auth was valid return the session ID
                if (authValid)
                {
                    return sessID;

                }

                // if auth wasn't valid, return error and exit
                else
                {
                    Console.WriteLine("");
                    Console.WriteLine("[-] ERROR: Could not authenticate to URL with credentials provided.");
                    Console.WriteLine("");
                    return null;
                }


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
         * Get the full Bitbucket URL, based on project key and repo slug
         * 
         * */
        public static async Task<string> GetFullBitbucketRepoURLAsync(string credential, string sessID, string projKey, string repoSlug, string url)
        {
            string urlToReturn = "Unable to Determine";

            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;


                // web request to get a repo details in Bitbucket via API
                var webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/rest/api/1.0/projects/" + projKey + "/repos/" + repoSlug + "");
                if (webRequest != null)
                {
                    // set header values
                    webRequest.Method = "GET";
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

                    // get web response
                    WebResponse myWebResponse = await webRequest.GetResponseAsync();
                    string content;
                    var reader = new StreamReader(myWebResponse.GetResponseStream());
                    content = reader.ReadToEnd();

                    // parse the JSON output and display results
                    JsonTextReader jsonResult = new JsonTextReader(new StringReader(content));

                    string propName = "";

                    // read the json results
                    while (jsonResult.Read())
                    {

                        switch (jsonResult.TokenType.ToString())
                        {
                            case "PropertyName":
                                propName = jsonResult.Value.ToString();
                                break;
                            case "String":

                                if (propName.ToLower().Equals("href") && jsonResult.Value.ToString().ToLower().Contains(repoSlug) && jsonResult.Value.ToString().EndsWith(".git") && jsonResult.Value.ToString().StartsWith("http"))
                                {
                                    urlToReturn = jsonResult.Value.ToString();
                                    urlToReturn = urlToReturn.Replace(".git", "");
                                }

                                break;
                            default:
                                break;

                        }

                    }

                }
            }

            catch (Exception ex)
            {
                return urlToReturn;
            }

            return urlToReturn;


        }


    }

}
