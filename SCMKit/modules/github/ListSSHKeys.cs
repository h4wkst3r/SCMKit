using Octokit;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;


namespace SCMKit.modules.github
{
    class ListSSHKeys
    {

        public static async Task execute(string credential, string url, string options, string system)
        {

            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("listsshkey", credential, url, options, system));

            try
            {
                await library.Utils.HeartbeatRequest(url);


                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

                GitHubClient client = library.GitHubUtils.AuthToGitHub(credential, url);



                var currentSSHKeys = await client.User.GitSshKey.GetAllForCurrent();

                IEnumerator<Octokit.PublicKey> enumeratorCurrentSSHKeys = currentSSHKeys.GetEnumerator();

                // create table header
                string tableHeader = string.Format("{0,12} | {1,25} | {2,20}", "SSH Key ID", "SSH Key Value", "Title");
                Console.WriteLine(tableHeader);
                Console.WriteLine(new String('-', tableHeader.Length));

                while (enumeratorCurrentSSHKeys.MoveNext())
                {
                    string[] sshKeyArray = enumeratorCurrentSSHKeys.Current.Key.Split(' ');
                    string justSSHKey = sshKeyArray[1].Substring(sshKeyArray[1].Length - 20, 20);

                    Console.WriteLine("{0,12} | {1,25} | {2,20}", enumeratorCurrentSSHKeys.Current.Id, "....." + justSSHKey, enumeratorCurrentSSHKeys.Current.Title);



                }




            }

            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Could not list user SSH keys. Exception: " + ex.ToString());
                Console.WriteLine("");
            }


        }


    }
}
