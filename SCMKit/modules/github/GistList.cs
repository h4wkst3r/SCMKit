using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;

namespace SCMKit.modules.github
{
    class GistList
    {


        public static async Task execute(string credential, string url, string options, string system)
        {

            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("listgist", credential, url, options, system));

            try
            {
                await library.Utils.HeartbeatRequest(url);

                GitHubClient client = library.GitHubUtils.AuthToGitHub(credential, url);

                var currentGists = await client.Gist.GetAll();
                IEnumerator<Gist> currentUserGists = currentGists.GetEnumerator();

                // create table header
                string tableHeader = string.Format("{0,40} | {1,10} | {2,50}", "Description", "Visibility", "URL");
                Console.WriteLine(tableHeader);
                Console.WriteLine(new String('-', tableHeader.Length));

                while (currentUserGists.MoveNext())
                {
                    string visibility = "private";
                    if (currentUserGists.Current.Public)
                    {
                        visibility = "public";
                    }

                    Console.WriteLine("{0,40} | {1,10} | {2,50}", currentUserGists.Current.Description, visibility, currentUserGists.Current.HtmlUrl);


                }



            }

            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Could not retrieve listing of gists. Exception: " + ex.ToString());
                Console.WriteLine("");
            }


        }

    }
}
