using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace SCMKit.modules.github
{
    class Privs
    {


        public static async Task execute(string credential, string url, string options, string system)
        {

            Dictionary<string, string> valuePairs = new Dictionary<string, string>();
            valuePairs.Add("repo", "Grants full access to repositories, including private repositories.");
            valuePairs.Add("repo:status", "Grants read/write access to public and private repository commit statuses.");
            valuePairs.Add("repo_deployment", "Grants access to deployment statuses for public and private repositories.");
            valuePairs.Add("public_repo", "Limits access to public repositories.");
            valuePairs.Add("repo:invite", "Grants accept/decline abilities for invitations to collaborate on a repository.");
            valuePairs.Add("security_events", "RW access to security events in code scanning and secret scanning APIs.");
            valuePairs.Add("admin:repo_hook", "Grants read, write, ping, and delete access to repository hooks in public and private repositories.");
            valuePairs.Add("write:repo_hook", "Grants read, write, and ping access to hooks in public or private repositories.");
            valuePairs.Add("read:repo_hook", "Grants read and ping access to hooks in public or private repositories.");
            valuePairs.Add("admin:org", "Fully manage the organization and its teams, projects, and memberships.");
            valuePairs.Add("write:org", "Read and write access to organization membership, organization projects, and team membership.");
            valuePairs.Add("read:org", "Read-only access to organization membership, organization projects, and team membership.");
            valuePairs.Add("admin:public_key", "Fully manage public keys.");
            valuePairs.Add("write:public_key", "Create, list, and view details for public keys.");
            valuePairs.Add("read:public_key", "List and view details for public keys.");
            valuePairs.Add("admin:org_hook", "	Grants read, write, ping, and delete access to organization hooks");
            valuePairs.Add("gist", "Grants write access to gists.");
            valuePairs.Add("notifications", "Read access to user's notifications");
            valuePairs.Add("user", "Grants read/write access to profile info only");
            valuePairs.Add("read:user", "Grants access to read a user's profile data.");
            valuePairs.Add("user:email", "Grants read access to a user's email addresses.");
            valuePairs.Add("user:follow", "Grants access to follow or unfollow other users.");
            valuePairs.Add("delete_repo", "Grants access to delete adminable repositories.");
            valuePairs.Add("write:discussion", "Allows read and write access for team discussions.");
            valuePairs.Add("read:discussion", "Allows read access for team discussions.");
            valuePairs.Add("write:packages", "Grants access to upload or publish a package in GitHub Packages. ");
            valuePairs.Add("read:packages", "Grants access to download or install packages from GitHub Packages.");
            valuePairs.Add("delete:packages", "Grants access to delete packages from GitHub Packages.");
            valuePairs.Add("admin:gpg_key", "Fully manage GPG keys.");
            valuePairs.Add("write:gpg_key", "Create, list, and view details for GPG keys.");
            valuePairs.Add("read:gpg_key", "List and view details for GPG keys.");
            valuePairs.Add("workflow", "Grants the ability to add and update GitHub Actions workflow files.");
            valuePairs.Add("site_admin", "Full site administrator access.");
            valuePairs.Add("admin:enterprise", "Full control of enterprise. One step below site admin.");


            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("privs", credential, url, options, system));
            List<String> listOfPermissions = new List<String>();


            // if username/password auth being used
            if (credential.Contains(":"))
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Privs module only supports API key authentication to determine privs of the API key given.");
                Console.WriteLine("");
            }

            // if token auth being used
            else
            {




                // create table header
                string tableHeader = string.Format("{0,20} | {1,70}", "Name", "Description");
                Console.WriteLine(tableHeader);
                Console.WriteLine(new String('-', tableHeader.Length));


                try
                {
                    ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    await library.Utils.HeartbeatRequest(url);

                    var webRequest = System.Net.WebRequest.Create(url + "/api/v3");
                    if (webRequest != null)
                    {
                        webRequest.Method = "GET";
                        webRequest.ContentType = "application/json";
                        webRequest.Headers.Add("Authorization", "Token " + credential);
                        WebResponse myWebResponse = await webRequest.GetResponseAsync();


                        // Display each header and it's key , associated with the response object.
                        for (int i = 0; i < myWebResponse.Headers.Count; ++i)
                        {

                            if (myWebResponse.Headers.Keys[i].ToString().ToLower().Equals("x-oauth-scopes"))
                            {
                                string[] splitValues = myWebResponse.Headers[i].Split(',');
                                foreach (string val in splitValues)
                                {
                                    foreach (var item in valuePairs)
                                    {


                                        if (item.Key.Trim().ToLower().Equals(val.Trim().ToLower()))
                                        {
                                            Console.WriteLine("{0,20} | {1,70}", val.Trim(), item.Value);
                                        }
                                    }


                                }
                            }


                        }

                        // Release resources of response object.
                        myWebResponse.Close();
                    }


                }

                catch (Exception ex)
                {
                    Console.WriteLine("");
                    Console.WriteLine("[-] ERROR: Could not retrieve listing of privileges for current API token. Exception: " + ex.ToString());
                    Console.WriteLine("");
                }
            }


        }



    }
}
