using Octokit;
using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace SCMKit.modules.github
{
    class CreateSSHKey
    {


        public static async Task execute(string credential, string url, string options, string system)
        {

            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("createsshkey", credential, url, options, system));

            try
            {
                await library.Utils.HeartbeatRequest(url);


                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

                GitHubClient client = library.GitHubUtils.AuthToGitHub(credential, url);

                // create random key name
                Random rd = new Random();
                const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
                char[] chars = new char[5];

                for (int i = 0; i < 5; i++)
                {
                    chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
                }
                string sshKeyName = new string(chars);
                sshKeyName = "SCMKIT-" + sshKeyName;

                await client.User.GitSshKey.Create(new NewPublicKey(sshKeyName, options));

                Console.WriteLine("");
                Console.WriteLine("[+] SUCCESS: The user SSH key was successfully added.");
                Console.WriteLine("");




            }

            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Could not add user SSH key. Exception: " + ex.ToString());
                Console.WriteLine("");
            }


        }


    }
}
